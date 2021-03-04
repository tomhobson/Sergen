using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Sergen.Core.Data;

namespace Sergen.Core.Services.ServerFileStore
{
    public class LocalServerFileStore : IServerFileStore
    {
        public async Task<string> GetServerPathOrCreateIt(string serverId)
        {
            // Create basic server dirs if they don't exist already
            var basePath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var serverFiles = Path.Combine(basePath, "server-files");
            var serverPath = Path.Combine(serverFiles, serverId);
            await CreateDirectoriesIfNotExist(serverFiles);
            await CreateDirectoriesIfNotExist(serverPath);
            return serverPath;
        }

        public async Task<string> GetGameServerDirectoryOrCreateIt(string serverId, GameServer gameServer)
        {
            var serverPath = await GetServerPathOrCreateIt(serverId);
            var gameFilesPath = Path.Combine(serverPath, gameServer.ServerName);
            await CreateDirectoriesIfNotExist(gameFilesPath);
            return gameFilesPath;
        }
        
        public async Task CreateDirectoriesIfNotExist(string path)
        {
            if(Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}