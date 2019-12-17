using System.Collections.Generic;
using _8inServant.Data;

namespace _8inServant.Services.ServerStore
{
    public interface IServerStore
    {
         public IList<GameServer> GetAllServers(string containerType);

         public GameServer GetGameServerByName(string serverName, string containerType);
    }
}