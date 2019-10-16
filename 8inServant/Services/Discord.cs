using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace _8inServant.Services
{
    public class Discord : IChat
    {
        private readonly DiscordSocketClient _discord;
        private readonly IConfiguration _config;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public Discord(
            DiscordSocketClient discord,
            IConfiguration config)
        {
            _config = config;
            _discord = discord;


            string discordToken = _config["Tokens:Discord"];     // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace(discordToken))
                throw new Exception("Please enter your bot's token into the `appsettings.json` file found in the applications root directory.");

            _discord.LoginAsync(TokenType.Bot, discordToken);     // Login to discord
            _discord.StartAsync();                                // Connect to the websocket
        }
    }
}
