using System.Security.Cryptography;
using System;
using System.Threading.Tasks;
using Discord;
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
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Sergen");

            builder.WithDescription(response);

            builder.WithColor(Color.Red);
            var mes = await _responseChannel.SendMessageAsync("", false, builder.Build());
            
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
                EmbedBuilder builder = new EmbedBuilder();

                builder.WithTitle("Sergen");

                builder.WithDescription(update);

                builder.WithColor(Color.Red);
                
                _lastMessagedInteractedWith.ModifyAsync(msg => msg.Embed = builder.Build());
            }
        }
    }
}