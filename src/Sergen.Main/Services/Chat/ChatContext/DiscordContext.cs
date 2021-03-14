using System;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace Sergen.Main.Services.Chat.ChatContext
{
    public class DiscordContext : IChatContext
    {
        private readonly DiscordShardedClient _discord;
        private readonly IConfiguration _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public DiscordContext (
            DiscordShardedClient discord,
            IConfiguration config)
        {
            _discord = discord;
            _config = config;
        }

        public async void Connect ()
        {
            string discordToken = _config["Tokens:Discord"]; // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace (discordToken))
                throw new Exception ("Please enter your bot's token into the `appsettings.json` file found in the applications root directory.");

            await _discord.LoginAsync (TokenType.Bot, discordToken); // Login to discord
            await _discord.StartAsync (); // Connect to the websocket
        }

        public string GetUsername (string userID)
        {
            return _discord.GetUser (Convert.ToUInt64(userID)).Username;
        }
    }
}