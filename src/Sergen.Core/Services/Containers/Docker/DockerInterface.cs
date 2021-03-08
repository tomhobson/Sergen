using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sergen.Core.Data;
using Sergen.Core.Services.Chat.ChatResponseToken;
using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sergen.Core.Options;
using Sergen.Core.Services.IpGetter;
using Sergen.Core.Services.Chat.StaticHelpers;
using Sergen.Core.Services.ServerFileStore;

namespace Sergen.Core.Services.Containers.Docker 
{
    public class DockerInterface : IContainerInterface 
    {
        ConcurrentDictionary<string, DockerContainer> _containerStore = new ConcurrentDictionary<string, DockerContainer>();

        private DockerClient _client;

        private readonly ILogger _logger;
        private readonly SteamLoginOptions _steamLoginOptions;
        private readonly IIpGetter _ipGetter;
        private readonly IServerFileStore _fileStore;

        private const int WAIT_FOR_DOCKER_MILLISECONDS = 2500;

        public DockerInterface (ILogger<DockerInterface> logger, IOptions<SteamLoginOptions> steamLoginOptions, IIpGetter ipGetter, IServerFileStore fileStore) 
        {
            _logger = logger;
            _steamLoginOptions = steamLoginOptions.Value;
            _ipGetter = ipGetter;
            _fileStore = fileStore;
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
            return containers.Select (cnt => $"Image: {cnt.Image} State: {cnt.State}").ToList ();
        }

        public async Task<string> Setup(IChatResponseToken icrt, GameServer gameServer)
        {
            try
            {
                _logger.LogInformation($"Setting up {gameServer.ServerName}...");
                
                var progression = new Progress<JSONMessage>();

                DockerContainer  dcsu = new DockerContainer(_logger, icrt, gameServer);
            
                progression.ProgressChanged += dcsu.HandleUpdate;

                await _client.Images.CreateImageAsync (new ImagesCreateParameters 
                    {
                        FromImage = gameServer.ContainerName,
                        Tag = gameServer.ContainerTag
                    },
                    new AuthConfig (),
                    progression);

                _containerStore.TryAdd(dcsu.ID, dcsu);
                _logger.LogInformation($"Completed setup for {gameServer.ServerName}...");
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
                _logger.LogInformation($"{gameServer.ServerName} for ServerId:{serverId} starting run.");

                var portsToExpose = new Dictionary<string, EmptyStruct>();
                var portBindings = new Dictionary<string, IList<PortBinding>>();
                var mounts = new List<Mount>();
                
                // Now assign the ports.
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
                
                // Assign the environment variables to the container
                var env = new List<string>();

                if (gameServer.EnvironmentalVariables != null)
                {
                    foreach (var variable in gameServer.EnvironmentalVariables)
                    {
                        if (variable.Value == "{$STEAM_USER}")
                        {
                            env.Add($"{variable.Key}={_steamLoginOptions.Username}");    
                            continue;
                        }

                        if (variable.Value == "{$STEAM_PASS}")
                        {
                            env.Add($"{variable.Key}={_steamLoginOptions.Password}");
                            continue;
                        }
                        
                        env.Add($"{variable.Key}={variable.Value}");
                    }
                }

                // Create basic server dirs if they don't exist already
                var gameFilesPath = await _fileStore.GetGameServerDirectoryOrCreateIt(serverId, gameServer);
                
                // Create all the mounts for the containers
                foreach (var bindData in gameServer?.Binds ?? Enumerable.Empty<string>())
                {
                    mounts.Add(await CreateContainerMount(gameFilesPath, bindData));
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

                _logger.LogInformation($"{gameServer.ServerName} for ServerId:{serverId} now running.");
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
                var messageId = await icrt.Respond($"Stopping game server {gameServer.ServerName}.");
                
                await StopAndRemove(gameContainers[0].ID);
                
                await icrt.Update(messageId, $"Game server {gameServer.ServerName} removed.");
                _logger.LogInformation($"{gameServer.ServerName} for ServerId:{serverId} stopped and removed.");
            }
            
            if (gameContainers.Count() > 1)
            {
                string runningContainerIds = "";

                foreach (var cont in gameContainers)
                {
                    runningContainerIds += $"{cont.ID} {cont.Image} {cont.Created} \n";
                }

                await icrt.Respond($"Which container would you like me to stop? `-stop *containerid*`");
            }
        }

        public async Task StopById(string serverId, IChatResponseToken icrt, string containerId)
        {
            var messageId = await icrt.Respond($"Stopping game server {containerId}.");
            
            var success = await StopAndRemove(containerId);
            if (success)
            {
                await icrt.Update(messageId, $"Game server {containerId} removed.");
            }
            else
            {
                await icrt.Update(messageId, "No servers to stop found.");
            }
        }

        private async Task<Mount> CreateContainerMount(string gameFilesPath, string bindData)
        {
            string actualBind = bindData;
            if (bindData.StartsWith("/"))
            {
                actualBind = bindData.Remove(0, 1);
            }


            actualBind = Path.Combine(gameFilesPath, actualBind);
            CreateDirectoriesIfNotExist(actualBind);

            return new Mount()
            {
                Target = bindData,
                Source = actualBind,
                Type = "bind"
            };
        }

        private async Task<bool> StopAndRemove(string containerId)
        {
            var allContainers = await _client.Containers.ListContainersAsync(new ContainersListParameters() {All = true});
            var exists = allContainers.Any(x => x.ID == containerId);

            if (exists == false)
            {
                return exists;
            }
            
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
            return true;
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