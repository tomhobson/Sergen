using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Sergen.Services.Chat.ChatContext;
using Sergen.Services.Chat.ChatResponseToken;
using Sergen.Services.Containers;
using Sergen.Services.ServerStore;

namespace  Sergen.Services.Chat.ChatProcessor
{
    public class ChatProcessor : IChatProcessor
    {
        private readonly IChatContext _context;
        private readonly IContainerInterface _containerInterface;

        private readonly IServerStore _serverStore;

        public ChatProcessor (
            IChatContext context,
            IContainerInterface containerInt,
            IServerStore serverStore)
        {
            _context = context;
            _containerInterface = containerInt;
            _serverStore = serverStore;
        }

        public async Task ProcessMessage (IChatResponseToken icrt, ulong senderID, string input)
        {
            // Switch statement for all commands that are constant
            switch (input)
            {
                case "-ping":
                    icrt.Respond("pong!");
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
                    var allcontainers = ListToStringList (await _containerInterface.GetRunningContainers ());
                    icrt.Respond($"Running containers are: {allcontainers}");
                    break;
                case "-possible":
                    var servers = _serverStore.GetAllServers (GetContainerInterfaceType ());
                    var serverNames = servers.Select (s => s.ServerName).ToList ();
                    var serverStringList = ListToStringList (serverNames);
                    icrt.Respond($"Possible game servers are: {serverStringList}");
                    break;
            }

            if (input.StartsWith ("-run "))
            {
                // Time to do some work
                var serverName = input.Replace ("-run ", "");
                var gameServer = _serverStore.GetGameServerByName (serverName, GetContainerInterfaceType ());
                var contId = await _containerInterface.Setup(icrt,gameServer);

                _containerInterface.Run(contId);
            }
        }

        public string ListToStringList (IList<string> inputList)
        {
            string allText = "";
            foreach (var stri in inputList)
            {
                allText = allText + $"\n {stri}";
            }
            return allText;
        }

        public string GetContainerInterfaceType ()
        {
            return _containerInterface.GetType ().ToString ();
        }
    }
}