using System.Collections.Generic;
using Sergen.Data;

namespace Sergen.Services.ServerStore
{
    public interface IServerStore
    {
         IList<GameServer> GetAllServers(string containerType);

         GameServer GetGameServerByName(string serverName, string containerType);
    }
}