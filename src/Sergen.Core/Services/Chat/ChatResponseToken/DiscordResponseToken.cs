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
            var embed = await CreateEmbed(response);
            
            var mes = await _responseChannel.SendMessageAsync("", false, embed);
            
            _lastMessagedInteractedWith = mes;
            return mes.Id.ToString();
        }

        public async Task Update(string messageID, string update)
        {
            var message = await _responseChannel.GetMessageAsync(Convert.ToUInt64(messageID));
            if(message is RestUserMessage rumess)
            {
                
                var embed = await CreateEmbed(update);
                
                await rumess.ModifyAsync(msg => msg.Embed = embed);
                _lastMessagedInteractedWith = rumess;
            }
        }

        public async Task UpdateLastInteractedWithMessage(string update)
        {
            if(_lastMessagedInteractedWith != null)
            {
                var embed = await CreateEmbed(update);
                
                _lastMessagedInteractedWith.ModifyAsync(msg => msg.Embed = embed);
            }
        }

        private async Task<Embed> CreateEmbed(string message)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Sergen");

            builder.WithDescription(message);

            builder.WithColor(Color.Green);
            
            return builder.Build();
        }
    }
}