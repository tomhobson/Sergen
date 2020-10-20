using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sergen.Core.Data;
using Sergen.Core.Services.Chat.StaticHelpers;

namespace Sergen.Core.Services.ServerStore
{
    public class JsonServerStore : IServerStore
    {
        private string _path = "";

        public JsonServerStore ()
        {
            var executingLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _path = Path.Combine (executingLocation, "GameServers");
        }

        public IList<GameServer> GetAllServers (string containerType)
        {
            List<GameServer> servers = new List<GameServer> ();

            var files = Directory.GetFiles (_path);

            foreach (var file in files)
            {
                var text = File.ReadAllText (file);
                var gameServer = JsonConvert.DeserializeObject<GameServer> (text);
                if (gameServer.ContainerType == containerType)
                {
                    servers.Add (gameServer);
                }
            }
            return servers;
        }

        public GameServer GetGameServerByName (string serverName)
        {
            var files = Directory.GetFiles (_path);

            foreach (var file in files)
            {
                var text = File.ReadAllText (file);
                var gameServer = JsonConvert.DeserializeObject<GameServer> (text);
                if (ChatHelper.PreParseInputString(gameServer.ServerName) == serverName)
                {
                    return gameServer;
                }
            }
            return null;
        }
        
        public GameServer GetGameServerByName (string serverName, string containerType)
        {
            var files = Directory.GetFiles (_path);

            foreach (var file in files)
            {
                var text = File.ReadAllText (file);
                var gameServer = JsonConvert.DeserializeObject<GameServer> (text);
                if (gameServer.ContainerType == containerType && ChatHelper.PreParseInputString(gameServer.ServerName) == serverName)
                {
                    return gameServer;
                }
            }
            return null;
        }

        public IList<GameServer> SearchGameServersByName(string serverName)
        {
            var files = Directory.GetFiles (_path);
            
            var returnServers = new List<GameServer>();
            
            foreach (var file in files)
            {
                var text = File.ReadAllText (file);
                var gameServer = JsonConvert.DeserializeObject<GameServer> (text);
                if (serverName.Contains(ChatHelper.PreParseInputString(gameServer.ServerName)))
                {
                    returnServers.Add(gameServer);
                }
            }

            if (returnServers.Any())
            {
                return returnServers;
            }
            return null;
        }
    }
}