using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using _8inServant.Data;
using _8inServant.Services.Chat.ChatResponseToken;

namespace _8inServant.Services.Containers
{
    public interface IContainerInterface
    {
        Task<IList<string>> GetRunningContainers();

        Task<string> Setup(IChatResponseToken icrt, GameServer gameServer);

        Task<string> Run(string id);
    }
}