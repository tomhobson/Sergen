using System;
using System.Linq;
using System.Threading.Tasks;
using _8inServant.Services.Chat.ChatProcessor;
using _8inServant.Services.Chat.ChatResponseToken;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace  _8inServant.Services.Chat.ChatEventHandler
{
    public class DiscordEventHandler : IChatEventHandler
    {
        private readonly DiscordSocketClient _discord;
        private readonly IChatProcessor _chatProcessor;
        private readonly IConfiguration _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public DiscordEventHandler (
            DiscordSocketClient discord,
            IChatProcessor chatProcessor,
            IConfiguration config)
        {
            _discord = discord;
            _config = config;
            _chatProcessor = chatProcessor;
        }

        public async void Connect ()
        {
            string discordToken = _config["Tokens:Discord"]; // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace (discordToken))
                throw new Exception ("Please enter your bot's token into the `appsettings.json` file found in the applications root directory.");

            await _discord.LoginAsync (TokenType.Bot, discordToken); // Login to discord
            await _discord.StartAsync (); // Connect to the websocket

            _discord.GuildMemberUpdated += DiscordUserUpdated;

            _discord.MessageReceived += DiscordMessageRecieved;
        }

        private async Task DiscordUserUpdated (SocketUser preUser, SocketUser postUser)
        {
            if (postUser.IsBot == false && postUser.Activity != null && preUser.Activity?.Name != postUser.Activity.Name)
            {
                foreach (var mutualGuild in postUser.MutualGuilds)
                {
                    var usrList = mutualGuild.Users.Where (u => u.Activity?.Name == postUser.Activity.Name).ToList ();
                    //Assume general channel for now.
                    if (usrList.Count > 1)
                    {
                        SocketGuildChannel sgc;
                        if (mutualGuild.Channels.Any (c => c.Name == "general"))
                        {
                            sgc = mutualGuild.Channels.First (gc => gc.Name == "general");
                        }
                        else
                        {
                            // Catch all. eventually we want to use a value from the db that the guild has set.
                            // Could probably do this with -configure
                            sgc = mutualGuild.Channels.First ();
                        }

                        if (sgc is IMessageChannel imc)
                        {
                            await imc.SendMessageAsync ($"Are you lot really playing {postUser.Activity.Name}");
                        }
                    }
                }
            }
        }

        private async Task DiscordMessageRecieved (SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == _discord.CurrentUser.Id)
                return;

            IChatResponseToken crt = new DiscordResponseToken(message.Channel);

            await _chatProcessor.ProcessMessage (crt, message.Author.Id, message.Content);
        }
    }
}