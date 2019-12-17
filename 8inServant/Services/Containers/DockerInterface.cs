using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace _8inServant.Services.Containers
{
    public class DockerInterface : IContainerInterface
    {
        DockerClient _client;

        public DockerInterface()
        {
            var os = Environment.OSVersion;

            if (os.Platform == PlatformID.Unix)
            {
                _client = new DockerClientConfiguration(
                        new Uri("unix:///var/run/docker.sock"))
                    .CreateClient();
            }
            else if (os.Platform == PlatformID.Win32NT)
            {
                _client = new DockerClientConfiguration(
                        new Uri("npipe://./pipe/docker_engine"))
                    .CreateClient();
            }
        }

        public async Task<IList<string>> GetRunningContainers()
        {
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters(){ Limit = 10 });
            return containers.Select(cnt => $"Image: {cnt.Image} Status: {cnt.Status}").ToList();
        }
    }
}