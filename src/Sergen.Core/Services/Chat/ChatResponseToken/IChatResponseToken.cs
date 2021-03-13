using System;
using System.Threading.Tasks;

namespace  Sergen.Core.Services.Chat.ChatResponseToken
{
    public interface IChatResponseToken
    {
        DateTime SendTime { get; }
        
        Task<string> Respond (string response);

        Task Update(string messageID, string update);

        Task UpdateLastInteractedWithMessage(string update);
    }
}