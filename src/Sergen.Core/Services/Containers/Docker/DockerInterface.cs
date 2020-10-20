using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Sergen.Core.Data;
using Sergen.Core.Services.Chat.ChatResponseToken;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Sergen.Core.Services.IpGetter;
using Sergen.Core.Services.Chat.StaticHelpers;

namespace Sergen.Core.Services.Containers.Docker 
{
    public class DockerInterface : IContainerInterface 
    {
        ConcurrentDictionary<string, DockerContainer> _containerStore = new ConcurrentDictionary<string, DockerContainer>();

        private DockerClient _client;

        private readonly ILogger _logger;
        private readonly IIpGetter _ipGetter;

        private const int WAIT_FOR_DOCKER_MILLISECONDS = 2500;

        public DockerInterface (ILogger<DockerInterface> logger, IIpGetter ipGetter) 
        {
            _logger = logger;
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

        public async Task<IList<string>> GetRunningContainers (string serverId) 
        {
            //Get all containers that start with the server id
            var containers = await GetAllContainersRanByServer(serverId);
            return containers.Select (cnt => $"Image: {cnt.Image} Status: {cnt.Status}").ToList ();
        }

        public async Task<string> Setup(IChatResponseToken icrt, GameServer gameServer)
        {
            try
            {
                var progression = new Progress<JSONMessage>();

                DockerContainer  dcsu = new DockerContainer(icrt, gameServer);
            
                progression.ProgressChanged += dcsu.HandleUpdate;

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
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting up docker container. {ex.Message}");
                throw;
            }
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
            try
            {
                _containerStore.TryGetValue(id, out DockerContainer gameServerContainer);

                var gameServer = gameServerContainer.GameServer;

                var portsToExpose = new Dictionary<string, EmptyStruct>();
                var portBindings = new Dictionary<string, IList<PortBinding>>();
                var mounts = new List<Mount>();

                foreach (var port in gameServer.Ports)
                {
                    var portAssignment = port.Key;
                    if (port.Value == "udp")
                    {
                        portAssignment = $"{port.Key}/{port.Value}";
                    }

                    portsToExpose.Add(portAssignment, default(EmptyStruct));
                    portBindings.Add(portAssignment,
                        new List<PortBinding> {new PortBinding {HostPort = portAssignment}});
                }

                var env = new List<string>();

                if (gameServer.EnvironmentalVariables != null)
                {
                    foreach (var variable in gameServer.EnvironmentalVariables)
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

                foreach (var bindData in gameServer?.Binds ?? Enumerable.Empty<string>())
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

                var response = await _client.Containers.CreateContainerAsync(new CreateContainerParameters
                {
                    Image = $"{gameServer.ContainerName}:{gameServer.ContainerTag}",
                    Name = $"{serverId}-{gameServer.ServerName.Replace(" ", "")}",
                    ExposedPorts = portsToExpose,
                    HostConfig = new HostConfig
                    {
                        PortBindings = portBindings,
                        PublishAllPorts = true,
                        Mounts = mounts
                    },
                    Env = env,
                    Cmd = gameServer.Commands
                });

                await _client.Containers.StartContainerAsync(response.ID, null);

                await icrt.UpdateLastInteractedWithMessage($"{gameServer.ServerName} available at: "
                                                           + $"{await _ipGetter.GetIp()} With Ports: {ObjectToString.Convert(gameServer.Ports)}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error running docker container. {ex.Message}");
            }
        }

        public async Task Stop(string serverId, IChatResponseToken icrt, GameServer gameServer)
        {
            var allContainers = await GetAllContainersRanByServer(serverId);

            var gameContainers = allContainers.Where(x => x.Image.Contains(gameServer.ContainerName)).ToList();

            if (gameContainers.Any() == false)
            {
                await icrt.Respond("No servers to stop found.");
                return;
            }
            
            if (gameContainers.Count() == 1)
            {
                StopAndRemove(gameContainers[0].ID);
                
                icrt.Respond($"Game server {gameServer.ServerName} removed.");
            }
            
            if (gameContainers.Count() > 1)
            {
                string runningContainerIds = "";

                foreach (var cont in gameContainers)
                {
                    runningContainerIds += $"{cont.ID} {cont.Image} {cont.Created} \n";
                }

                icrt.Respond($"Which container would you like me to stop? `-stop *containerid*`");
            }
        }

        public async Task StopById(string serverId, IChatResponseToken icrt, string containerId)
        {
            StopAndRemove(containerId);

            icrt.Respond($"Game server {containerId} removed.");
        }

        private async Task StopAndRemove(string containerId)
        {
            await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters()
            {
                WaitBeforeKillSeconds = 10
            });
            
            //Sleep so the daemon can stop the pod
            Thread.Sleep(WAIT_FOR_DOCKER_MILLISECONDS);
            
            await _client.Containers.RemoveContainerAsync(containerId, new ContainerRemoveParameters()
            {
                Force = false
            });
        }

        private async Task<IEnumerable<ContainerListResponse>> GetAllContainersRanByServer(string serverId)
        {
            var allContainers =
                await _client.Containers.ListContainersAsync(new ContainersListParameters() {Limit = 40});

            var serverContainers = new List<ContainerListResponse>();

            foreach (var container in allContainers)
            {
                bool isServersContainer = false;
                foreach (var name in container.Names)
                {
                    if (name.Contains(serverId))
                        isServersContainer = true;
                }

                if (isServersContainer)
                    serverContainers.Add(container);
            }

            //var serverContainers = allContainers.Where(x => x.Names.Any(y => y.Contains(serverId)));
            return serverContainers;
        }
    }
}