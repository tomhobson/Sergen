using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sergen.Data;
using Sergen.Services.Chat.ChatResponseToken;

namespace Sergen.Services.Containers
{
    public interface IContainerInterface
    {
        Task<IList<string>> GetRunningContainers();

        Task<string> Setup(IChatResponseToken icrt, GameServer gameServer);

        Task<string> Run(string id);
    }
}