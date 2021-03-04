using System.Threading.Tasks;
using Sergen.Core.Services.Chat.ChatResponseToken;

namespace  Sergen.Main.Services.Chat.ChatProcessor
{
    public interface IChatProcessor
    {
        Task ProcessMessage (string serverId, IChatResponseToken _responder, string senderID, string input);
    }
}