using System.Threading.Tasks;
using Sergen.Core.Data;

namespace Sergen.Core.Services
{
    public interface IServerFileStore
    {
        Task<string> GetServerPathOrCreateIt(string serverId);

        Task<string> GetGameServerDirectoryOrCreateIt(string serverId, GameServer gameServer);

        Task CreateDirectoriesIfNotExist(string path);
    }
}