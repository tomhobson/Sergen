using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sergen.Core.Services.Chat.ChatResponseToken;
using Sergen.Core.Services.Chat.StaticHelpers;
using Sergen.Core.Services.ServerStore;
using Sergen.Main.Services.Chat.ChatProcessor;

namespace  Sergen.Main.Services.Chat.ChatEventHandler
{
    public class DiscordEventHandler : IChatEventHandler
    {
        private readonly ILogger _logger;
        private readonly DiscordSocketClient _discord;
        private readonly IChatProcessor _chatProcessor;
        private readonly IConfiguration _config;
        private readonly IServerStore _serverStore;
        
        private Dictionary<ulong, DateTime> _lastMsgPerServer;

        // DiscordSocketClient, CommandService, and IConfigurationRoot are injected automatically from the IServiceProvider
        public DiscordEventHandler (
            ILogger<DiscordEventHandler> logger,
            DiscordSocketClient discord,
            IChatProcessor chatProcessor,
            IConfiguration config,
            IServerStore serverStore)
        {
            _logger = logger;
            _discord = discord;
            _config = config;
            _chatProcessor = chatProcessor;
            _serverStore = serverStore;
        }

        public async void Connect ()
        {
            string discordToken = _config["Tokens:Discord"]; // Get the discord token from the config file
            if (string.IsNullOrWhiteSpace (discordToken))
                throw new Exception ("Please enter your bot's token into the `appsettings.json` file found in the applications root directory.");

            await _discord.LoginAsync (TokenType.Bot, discordToken); // Login to discord
            await _discord.StartAsync (); // Connect to the websocket

            _discord.GuildMemberUpdated += DiscordUserUpdated;
            _discord.MessageReceived += DiscordMessageReceived;
            _lastMsgPerServer = new Dictionary<ulong, DateTime>();

            await _discord.SetGameAsync("Listening to -help");
        }

        /// <summary>
        /// This is fired anytime a user is updated
        /// </summary>
        /// <param name="preUser">The user before the update</param>
        /// <param name="postUser">The user after the update</param>
        private async Task DiscordUserUpdated(SocketUser preUser, SocketUser postUser)
        {
            // Knockout conditions
            if (postUser.IsBot || postUser.Activity == null ||
                preUser.Activity?.Name == postUser.Activity?.Name)
            {
                return;
            }

            foreach (var mutualGuild in postUser.MutualGuilds)
            {
                try
                {
                    await MessageGuildIfGameServerExists(mutualGuild, postUser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unable to post message to server {mutualGuild.Id}:{mutualGuild.Name}");
                }
            }
        }

        private async Task MessageGuildIfGameServerExists(SocketGuild mutualGuild,
            SocketUser postUser)
        {
            // Get all the users in all the servers we exist in playing this game.
            var usrList = mutualGuild.Users.Where(u => u.Activity?.Name == postUser.Activity.Name).ToList();
            var serverId = mutualGuild.Id;
            //Assume general channel for now.
            SocketGuildChannel sgc;
            if (mutualGuild.Channels.Any(c => c.Name == "general"))
            {
                sgc = mutualGuild.Channels.First(gc => gc.Name == "general");
            }
            else
            {
                // Catch all. eventually we want to use a value from the db that the guild has set.
                // Could probably do this with -configure
                sgc = mutualGuild.Channels.First();
            }

            var lastMsg = _lastMsgPerServer.GetValueOrDefault(serverId);
            
            //Make sure we don't send a loads of updates within a short period of time
            if (lastMsg != null && DateTime.Now.Subtract(lastMsg).Seconds <= 2)
            {
                return;
            }

            // Get the multiple word (people/person) playing a game.
            string multipleWord = "people";
            if (usrList.Count == 1)
            {
                multipleWord = "person";
            }

            //Decides if we have a person playing it and if we've got a server.
            if (sgc is IMessageChannel imc)
            {
                var parsedActivityName = ChatHelper.PreParseInputString(postUser.Activity.Name);
                var gameServer = _serverStore.GetGameServerByName(parsedActivityName);

                if (gameServer != null)
                {
                    //Definitley got one
                    var msg = DiscordHelper.CreateEmbeddedMessage(
                        $"{usrList.Count} {multipleWord} playing {postUser.Activity.Name}. I've got a server for it. Enter `-run {parsedActivityName}` to start one.");
                    await imc.SendMessageAsync(embed: msg);
                }
                else
                {
                    var possibleGameServers = _serverStore.SearchGameServersByName(parsedActivityName);

                    if (possibleGameServers == null)
                    {
                        return;
                    }

                    //Probably got one, the server name is contained within their activity
                    if (possibleGameServers.Count == 1)
                    {
                        var msg = DiscordHelper.CreateEmbeddedMessage(
                            $"{usrList.Count} {multipleWord} playing {postUser.Activity.Name}. I think I've got a server for it. Enter `-run {possibleGameServers[0].ServerName}` to start one.");
                        await imc.SendMessageAsync(embed: msg);
                    }
                    //Almost certainly got one, just don't know which one
                    else if (possibleGameServers.Count > 1)
                    {
                        var msg = DiscordHelper.CreateEmbeddedMessage(
                            $"{usrList.Count} {multipleWord} playing {postUser.Activity.Name}. I've got a few servers that match that title. Enter `-possible` to see if one matches what you're playing.");
                        await imc.SendMessageAsync(embed: msg);
                    }
                }

                await SetOrAddToLastMsgDictionary(serverId);
            }
        }

        private async Task DiscordMessageReceived (SocketMessage message)
        {
            // The bot should never respond to itself.
            if (message.Author.Id == _discord.CurrentUser.Id)
                return;

            IChatResponseToken crt = new DiscordResponseToken(message.Channel, message.Timestamp.DateTime);

            var channel = message.Channel as SocketGuildChannel;
            
            await _chatProcessor.ProcessMessage (channel.Guild.Id.ToString(), crt, message.Author.Id.ToString(), message.Content);
        }

        private async Task SetOrAddToLastMsgDictionary(ulong serverId)
        {
            if (_lastMsgPerServer.ContainsKey(serverId))
            {
                _lastMsgPerServer[serverId] = DateTime.Now;
                return;
            }
            _lastMsgPerServer.Add(serverId, DateTime.Now);
        }
    }
}