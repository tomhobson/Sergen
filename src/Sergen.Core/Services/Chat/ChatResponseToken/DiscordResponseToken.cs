using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;

namespace  Sergen.Core.Services.Chat.ChatResponseToken
{
    public class DiscordResponseToken : IChatResponseToken
    {
        ISocketMessageChannel _responseChannel;

        RestUserMessage _lastMessagedInteractedWith;

        public DiscordResponseToken (ISocketMessageChannel channel)
        {
            _responseChannel = channel;
        }

        public async Task<string> Respond(string response)
        {
            var mes = await _responseChannel.SendMessageAsync(response);
            _lastMessagedInteractedWith = mes;
            return mes.Id.ToString();
        }

        public async Task Update(string messageID, string update)
        {
            var message = await _responseChannel.GetMessageAsync(Convert.ToUInt64(messageID));
            if(message is RestUserMessage rumess)
            {
                await rumess.ModifyAsync(msg => msg.Content = update);
                _lastMessagedInteractedWith = rumess;
            }
        }

        public async Task UpdateLastInteractedWithMessage(string update)
        {
            if(_lastMessagedInteractedWith != null)
            {
                _lastMessagedInteractedWith.ModifyAsync(msg => msg.Content = update);
            }
        }
    }
}