using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sergen.Core.Services.Chat.ChatResponseToken;
using Sergen.Core.Services.Chat.StaticHelpers;
using Sergen.Core.Services.Containers;
using Sergen.Core.Services.IpGetter;
using Sergen.Core.Services.ServerStore;
using Sergen.Main.Services.Chat.ChatContext;
using Sergen.Main.Services.Chat.ChatWhitelist;

namespace Sergen.Main.Services.Chat.ChatProcessor
{
    public class ChatProcessor : IChatProcessor
    {
        private readonly IChatContext _context;
        private readonly IContainerInterface _containerInterface;
        private readonly IServerStore _serverStore;
        private readonly IIpGetter _ipGetter;
        private readonly IChatAllowList _allowList;

        public ChatProcessor (
            IChatContext context,
            IContainerInterface containerInt,
            IServerStore serverStore,
            IIpGetter ipGetter,
            IChatAllowList allowList)
        {
            _context = context;
            _containerInterface = containerInt;
            _serverStore = serverStore;
            _ipGetter = ipGetter;
            _allowList = allowList;
        }

        public async Task ProcessMessage (string serverID, IChatResponseToken icrt, string senderID, string input)
        {
            // Make the initial command case insensitive
            var firstCommand = input.Split(" ")[0].ToLower();
            
            // Switch statement for all commands that are constant
            switch (firstCommand)
            {
                case "-help":
                    await icrt.Respond(@"Command list:
                    `-ping` Will return if Sergen is alive.
                    `-ip` Will respond the ip of the main.
                    `-version` Will return 1.0.0 because I'm too lazy to fix the version.
                    `-running` Will return all the running game servers for this discord server.
                    `-possible` Will return all possible game servers.
                    `-run {Game Server}` Will start a game server of that type.
                    `-stop {Game Server}` Will stop a game server of that type.
                    `-allowlist` Will show you your current allowlist.
                    `-allowlist enable/disable` Will enable/disable the allowlist.
                    `-allowlist add @user` Will enable that user to control servers.
                    `-allowlist remove @user` Will stop that user from controlling servers.
                     ");
                    break;
                case "-ping":
                    var processingTime = DateTime.Now.Subtract(icrt.SendTime);
                    await icrt.Respond(processingTime.Milliseconds + "ms");
                    break;
                case "-ip":
                    await icrt.Respond($"My IP address is: {await _ipGetter.GetIp()}");
                    break;
                case "-whoami":
                    await icrt.Respond($"You are: {_context.GetUsername(senderID)}");
                    break;
                case "-version":
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assembly.Location);
                    string version = fvi.FileVersion;
                    await icrt.Respond($"My version is: {version}");
                    break;
                case "-running":
                    var allcontainers = ObjectToString.Convert(await _containerInterface.GetRunningContainers(serverID));
                    await icrt.Respond($"Running containers are: {allcontainers}");
                    break;
                case "-possible":
                    var servers = _serverStore.GetAllServers(GetContainerInterfaceType());
                    var serverNames = servers.Select(s => s.ServerName).ToList();
                    var serverStringList = ObjectToString.Convert(serverNames);
                    await icrt.Respond($"Possible game servers are: {serverStringList}");
                    break;
            }

            if (input.StartsWith ("-run ") || input.StartsWith ("-start "))
            {
                if (await VerifyUser(serverID, senderID, icrt))
                {
                    await AttemptRun(serverID, input, icrt);
                }
                return;
            }
            
            if (input.StartsWith ("-stop "))
            {
                if (await VerifyUser(serverID, senderID, icrt))
                {
                    await AttemptStop(serverID, input, icrt);
                }
                return;
            }

            if (input.StartsWith("-allowlist"))
            {
                if (await _allowList.IsUserAllowedToManage(serverID, senderID))
                {
                    if (input.StartsWith("-allowlist add "))
                    {
                        await _allowList.AddUser(serverID, input.Replace("-allowlist add ", ""));
                        await icrt.Respond("User added.");
                        return;
                    }

                    if (input.StartsWith("-allowlist remove "))
                    {
                        await _allowList.RemoveUser(serverID, input.Replace("-allowlist remove ", ""));
                        await icrt.Respond("User removed.");
                        return;
                    }

                    if (input.StartsWith("-allowlist enable"))
                    {
                        await _allowList.SetAllowListStatus(serverID, true);
                        await icrt.Respond("Allow list enabled.");
                        return;
                    }

                    if (input.StartsWith("-allowlist disable"))
                    {
                        await _allowList.SetAllowListStatus(serverID, false);
                        await icrt.Respond("Allow list disabled.");
                        return;
                    }

                    var allowList = await _allowList.GetAllowList(serverID);
                    var rawAllowList = string.Join("\n", allowList.Select(x => x.ToString()).ToArray());
                    await icrt.Respond("Allow list:\n" + rawAllowList);
                    return;
                }

                await icrt.Respond("I'm not allowed to talk to you, ask your server admin to allowlist you!");
            }
        }

        private async Task<bool> VerifyUser(string serverId, string userId, IChatResponseToken icrt)
        {
            if (await _allowList.IsUserAllowed(serverId, userId))
            {
                return true;
            }

            await icrt.Respond("I'm not allowed to talk to you, ask your server admin to allowlist you!");
            return false;
        }

        private async Task AttemptRun(string serverId, string input, IChatResponseToken responseToken)
        {
            // Time to do some work
            var serverName = ChatHelper.PreParseInputString(input.Replace ("-run ", "").Replace("-start ", ""));
            var gameServer = _serverStore.GetGameServerByName (serverName, GetContainerInterfaceType ());

            if (gameServer == null)
            {
                await responseToken.Respond("Image could not be found. Find all possible game servers with -possible");
                return;
            }
            var contId = await _containerInterface.Setup(responseToken, gameServer);

            await _containerInterface.Run(serverId, responseToken, contId);
        }

        private async Task AttemptStop(string serverId, string input, IChatResponseToken responseToken)
        {
            // Time to do some work
            var serverName = ChatHelper.PreParseInputString(input.Replace ("-stop ", ""));
            var gameServer = _serverStore.GetGameServerByName (serverName, GetContainerInterfaceType ());

            if (gameServer == null)
            {
                await _containerInterface.StopById(serverId, responseToken, serverName);
            }
            else
            {
                await _containerInterface.Stop(serverId, responseToken, gameServer);   
            }
        }

        public string GetContainerInterfaceType ()
        {
            return _containerInterface.GetType ().ToString ();
        }
    }
}