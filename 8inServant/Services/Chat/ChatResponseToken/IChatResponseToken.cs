using System.Threading.Tasks;

namespace  _8inServant.Services.Chat.ChatResponseToken
{
    public interface IChatResponseToken
    {
        Task<string> Respond (string response);

        Task Update(string messageID, string update);
    }
}