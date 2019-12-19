using System.Collections.Generic;
using Sergen.Core.Data;

namespace Sergen.Core.Services.ServerStore
{
    public interface IServerStore
    {
         IList<GameServer> GetAllServers(string containerType);

         GameServer GetGameServerByName(string serverName, string containerType);
    }
}