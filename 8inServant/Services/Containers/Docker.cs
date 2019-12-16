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
            DockerClient client = new DockerClientConfiguration(
            new Uri("unix:///var/run/docker.sock"))
            .CreateClient();

            _client = client;
        }

        public async Task<IList<string>> GetRunningContainers()
        {
            var containers = await _client.Containers.ListContainersAsync(new ContainersListParameters(){ Limit = 10 });
            return containers.Select(cnt => cnt.Image).ToList();
        }
    }
}