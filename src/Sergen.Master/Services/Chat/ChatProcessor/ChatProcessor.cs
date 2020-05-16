using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sergen.Core.Services.Chat.ChatResponseToken;
using Sergen.Core.Services.Chat.StaticHelpers;
using Sergen.Core.Services.Containers;
using Sergen.Core.Services.IpGetter;
using Sergen.Core.Services.ServerStore;
using Sergen.Master.Services.Chat.ChatContext;

namespace Sergen.Master.Services.Chat.ChatProcessor
{
    public class ChatProcessor : IChatProcessor
    {
        private readonly IChatContext _context;
        private readonly IContainerInterface _containerInterface;
        private readonly IServerStore _serverStore;
        private readonly IIpGetter _ipGetter;

        public ChatProcessor (
            IChatContext context,
            IContainerInterface containerInt,
            IServerStore serverStore,
            IIpGetter ipGetter)
        {
            _context = context;
            _containerInterface = containerInt;
            _serverStore = serverStore;
            _ipGetter = ipGetter;
        }

        public async Task ProcessMessage (string serverID, IChatResponseToken icrt, ulong senderID, string input)
        {
            // Make the initial command case insensitive
            var firstCommand = input.Split(" ")[0].ToLower();
            
            // Switch statement for all commands that are constant
            switch (firstCommand)
            {
                case "-help":
                    icrt.Respond(@"Command list:
                    `-ping` Will return if Sergen is alive.
                    `-ip` Will respond the ip of the master.
                    `-version` Will return 1.0.0 because I'm too lazy to fix the version.
                    `-running` Will return all the running game servers for this discord server.
                    `-possible` Will return all possible game servers.
                    `-run {Game Server}` Will start a game server of that type.
                    `-stop {Game Server}` Will stop a game server of that type.
                     ");
                    break;
                case "-ping":
                    icrt.Respond("pong!");
                    break;
                case "-ip":
                    icrt.Respond($"My IP Address is: {await _ipGetter.GetIp()}");
                    break;
                case "-whoami":
                    icrt.Respond($"You are: {_context.GetUsername(senderID)}");
                    break;
                case "-version":
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly ();
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo (assembly.Location);
                    string version = fvi.FileVersion;
                    icrt.Respond($"My version is: {version}");
                    break;
                case "-running":
                    var allcontainers = ObjectToString.Convert(await _containerInterface.GetRunningContainers (serverID));
                    icrt.Respond($"Running containers are: {allcontainers}");
                    break;
                case "-possible":
                    var servers = _serverStore.GetAllServers (GetContainerInterfaceType ());
                    var serverNames = servers.Select (s => s.ServerName).ToList ();
                    var serverStringList = ObjectToString.Convert(serverNames);
                    icrt.Respond($"Possible game servers are: {serverStringList}");
                    break;
            }

            if (input.StartsWith ("-run "))
            {
                // Time to do some work
                var serverName = ChatHelper.PreParseInputString(input.Replace ("-run ", ""));
                var gameServer = _serverStore.GetGameServerByName (serverName, GetContainerInterfaceType ());

                if (gameServer == null)
                {
                    icrt.Respond("Image could not be found. Find all possible game servers with -possible");
                    return;
                }
                var contId = await _containerInterface.Setup(icrt, gameServer);

                await _containerInterface.Run(serverID, icrt, contId);
            }
            
            if (input.StartsWith ("-stop "))
            {
                // Time to do some work
                var serverName = ChatHelper.PreParseInputString(input.Replace ("-stop ", ""));
                var gameServer = _serverStore.GetGameServerByName (serverName, GetContainerInterfaceType ());

                if (gameServer == null)
                {
                    await _containerInterface.StopById(serverID, icrt, serverName);
                }

                await _containerInterface.Stop(serverID, icrt, gameServer);
            }
        }

        public string GetContainerInterfaceType ()
        {
            return _containerInterface.GetType ().ToString ();
        }
    }
}