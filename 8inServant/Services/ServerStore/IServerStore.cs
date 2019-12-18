using System.Collections.Generic;
using _8inServant.Data;

namespace _8inServant.Services.ServerStore
{
    public interface IServerStore
    {
         IList<GameServer> GetAllServers(string containerType);

         GameServer GetGameServerByName(string serverName, string containerType);
    }
}