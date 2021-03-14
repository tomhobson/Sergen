using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Sergen.Core.Services.ServerFileStore;
using Sergen.Main.Data;

namespace Sergen.Main.Services.Chat.ChatWhitelist
{
    public class DiscordAllowList : IChatAllowList
    {
        private readonly ILogger<DiscordAllowList> _logger;
        private readonly DiscordShardedClient _discord;
        private readonly IServerFileStore _fileStore;
        private const string ALLOW_LIST_NAME = "allowList.json";

        public DiscordAllowList(ILogger<DiscordAllowList> logger, DiscordShardedClient discord, IConfiguration config, IServerFileStore fileStore)
        {
            _logger = logger;
            string discordToken = config["Tokens:Discord"]; // Get the discord token from the config file

            discord.LoginAsync (TokenType.Bot, discordToken).Wait(); // Login to discord
            discord.StartAsync().Wait(); // Connect to the websocket
            _discord = discord;
            _fileStore = fileStore;
        }

        public async Task SetAllowListStatus(string serverId, bool enabled)
        {
            var allowList = await GetInternalAllowList(serverId);

            allowList.Enabled = enabled;
            
            await File.WriteAllTextAsync(await GetAllowListLocation(serverId), JsonConvert.SerializeObject(allowList));
            return;
        }

        public async Task<bool> IsUserAllowed(string serverId, string UserId)
        {
            var allowList = await GetInternalAllowList(serverId);
            
            if (!allowList.Enabled)
            {
                return true;
            }
            
            if (allowList?.AllowedIds?.Contains(UserId) == true || await IsUserAllowedToManage(serverId, UserId))
            {
                return true;    
            }

            return false;
        }
        
        public async Task<bool> IsUserAllowedToManage(string serverId, string UserId)
        {
            var ownerId = _discord.GetGuild(Convert.ToUInt64(serverId)).OwnerId; 
            if (ownerId == Convert.ToUInt64(UserId))
            {
                return true;
            }
            return false;
        }

        public async Task<bool> AddUser(string serverId, string UserId)
        {
            var allowList = await GetInternalAllowList(serverId);
            
            if (!allowList.Enabled)
            {
                return false;
            }
            
            UserId = UserId.Replace("<@!", "");
            UserId = UserId.Replace(">", "");

            if (allowList?.AllowedIds != null && allowList.AllowedIds.Contains(UserId) == false)
            {
                allowList.AllowedIds.Add(UserId);
            }
            else
            {
                allowList = new AllowList()
                {
                    AllowedIds = new List<string>()
                    {
                        UserId
                    }
                };
            }
            
            await File.WriteAllTextAsync(await GetAllowListLocation(serverId), JsonConvert.SerializeObject(allowList));

            return true;
        }

        public async Task<bool> RemoveUser(string serverId, string UserId)
        {
            var allowList = await GetInternalAllowList(serverId);
            
            if (!allowList.Enabled)
            {
                return false;
            }
            
            UserId = UserId.Replace("<@!", "");
            UserId = UserId.Replace(">", "");

            if (allowList?.AllowedIds != null)
            {
                allowList.AllowedIds.Remove(UserId);
            }
            else
            {
                return false;
            }
            
            await File.WriteAllTextAsync(await GetAllowListLocation(serverId), JsonConvert.SerializeObject(allowList));

            return true;
        }

        private async Task<AllowList> GetInternalAllowList(string serverId)
        {
            try
            {
                var allowListLocation = await GetAllowListLocation(serverId);
                if (!File.Exists(allowListLocation))
                {
                    return new AllowList()
                    {
                        AllowedIds = new List<string>()
                    };
                }

                var allowListJson = await File.ReadAllTextAsync(allowListLocation);
                return JsonConvert.DeserializeObject<AllowList>(allowListJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't get allowlist.");
                return null;
            }
        }

        public async Task<IList<string>> GetAllowList(string serverId)
        {
            try
            {
                var allowList = await GetInternalAllowList(serverId);
                var userList = new List<string>();
                foreach (var allowedUser in allowList.AllowedIds)
                {
                    var user = _discord.GetUser(Convert.ToUInt64(allowedUser));
                    if (user != null)
                    {
                        userList.Add(user.Username);   
                    }
                }

                return userList;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Couldn't get allowlist.");
                return null;
            }
        }

        private async Task<string> GetAllowListLocation(string serverId)
        {
            var serverPath = await _fileStore.GetServerPathOrCreateIt(serverId);
            return Path.Combine(serverPath, ALLOW_LIST_NAME);
        }
    }
}