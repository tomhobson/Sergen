using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sergen.Core.Data;
using Sergen.Core.Services.Chat.ChatResponseToken;

namespace Sergen.Core.Services.Containers
{
    public interface IContainerInterface
    {
        Task<IList<string>> GetRunningContainers(string serverId);

        Task<string> Setup(IChatResponseToken icrt, GameServer gameServer);

        Task Run(string serverId, IChatResponseToken icrt, string id);
        
        Task Stop(string serverId, IChatResponseToken icrt, GameServer gameServer);
    }
}