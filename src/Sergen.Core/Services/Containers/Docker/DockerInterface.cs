using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Sergen.Core.Data;
using Sergen.Core.Services.Chat.ChatResponseToken;
using Docker.DotNet;
using Docker.DotNet.Models;
using Sergen.Core.Services.IpGetter;
using Sergen.Core.Services.Chat.StaticHelpers;

namespace Sergen.Core.Services.Containers.Docker 
{
    public class DockerInterface : IContainerInterface 
    {
        ConcurrentDictionary<string, DockerContainer> _containerStore = new ConcurrentDictionary<string, DockerContainer>();

        private DockerClient _client;

        private readonly IIPGetter _ipGetter;

        public DockerInterface (IIPGetter ipGetter) 
        {
            _ipGetter = ipGetter;
            var os = Environment.OSVersion;

            if (os.Platform == PlatformID.Unix) {
                _client = new DockerClientConfiguration (
                        new Uri ("unix:///var/run/docker.sock"))
                    .CreateClient ();
            } else if (os.Platform == PlatformID.Win32NT) {
                _client = new DockerClientConfiguration (
                        new Uri ("npipe://./pipe/docker_engine"))
                    .CreateClient ();
            }
        }

        public async Task<IList<string>> GetRunningContainers () 
        {
            var containers = await _client.Containers.ListContainersAsync (new ContainersListParameters () { Limit = 10 });
            return containers.Select (cnt => $"Image: {cnt.Image} Status: {cnt.Status}").ToList ();
        }

        public async Task<string> Setup(IChatResponseToken icrt, GameServer gameServer)
        {
            var progression = new Progress<JSONMessage>();

            DockerContainer  dcsu = new DockerContainer(icrt, gameServer);
            
            progression.ProgressChanged += dcsu.HandleUpdate;
            
            if (gameServer.ContainerTag == null)
            {
                gameServer.ContainerTag = "latest";
            }
            
            await _client.Images.CreateImageAsync (new ImagesCreateParameters 
                {
                    FromImage = gameServer.ContainerName,
                        Tag = gameServer.ContainerTag
                },
                new AuthConfig (),
                progression);

            _containerStore.TryAdd(dcsu.ID, dcsu);
            
            return dcsu.ID;
        }

        private void CreateDirectoriesIfNotExist(string path)
        {
            if(Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }

        public async Task Run (string serverId, IChatResponseToken icrt, string id) 
        {
            _containerStore.TryGetValue(id, out DockerContainer gameServerContainer);

            var gameServer = gameServerContainer.GameServer;

            var portsToExpose = new Dictionary<string, EmptyStruct>();
            var portBindings = new Dictionary<string, IList<PortBinding>>();
            var mounts = new List<Mount>();

            foreach(var port in gameServer.Ports)
            {
                var portAssignment = port.Key;
                if (port.Value == "udp")
                {
                    portAssignment = $"{port.Key}/{port.Value}";
                }
                
                portsToExpose.Add(portAssignment, default(EmptyStruct));
                portBindings.Add(portAssignment, new List<PortBinding>{ new PortBinding { HostPort = portAssignment} });
            }

            var env = new List<string>();

            if(gameServer.EnvironmentalVariables != null)
            {
                foreach(var variable in gameServer.EnvironmentalVariables)
                {
                    env.Add($"{variable.Key}={variable.Value}");
                }
            }
            
            //Create basic server dirs if they don't exist already
            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var serverFiles = Path.Combine(basePath, "server-files");
            var serverPath = Path.Combine(serverFiles, serverId);
            var gameFilesPath = Path.Combine(serverPath, gameServer.ServerName);
            CreateDirectoriesIfNotExist(serverFiles);
            CreateDirectoriesIfNotExist(serverPath);
            CreateDirectoriesIfNotExist(gameFilesPath);

            foreach (var bindData in gameServer.Binds)
            {
                string actualBind = bindData;
                if (bindData.StartsWith("/"))
                {
                    actualBind = bindData.Remove(0, 1);
                }


                actualBind = Path.Combine(gameFilesPath, actualBind);
                CreateDirectoriesIfNotExist(actualBind);
                
                mounts.Add(new Mount()
                {
                    Target = bindData,
                    Source = actualBind,
                    Type = "bind"
                });
            }

            var response = await _client.Containers.CreateContainerAsync (new CreateContainerParameters 
            {
                Image = $"{gameServer.ContainerName}:{gameServer.ContainerTag}",
                ExposedPorts = portsToExpose,
                HostConfig = new HostConfig {
                    PortBindings = portBindings,
                    PublishAllPorts = true,
                    Mounts = mounts
                },
                Env = env
            });

            await _client.Containers.StartContainerAsync(response.ID, null);

            await icrt.UpdateLastInteractedWithMessage($"{gameServer.ServerName} available at: "
            + $"{await _ipGetter.GetIP()} With Ports: {ObjectToString.Convert(gameServer.Ports)}");
        }
    }
}