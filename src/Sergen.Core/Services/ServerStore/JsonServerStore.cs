using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Sergen.Core.Data;

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

        public GameServer GetGameServerByName (string serverName, string containerType)
        {
            var files = Directory.GetFiles (_path);

            foreach (var file in files)
            {
                var text = File.ReadAllText (file);
                var gameServer = JsonConvert.DeserializeObject<GameServer> (text);
                if (gameServer.ContainerType == containerType && gameServer.ServerName == serverName)
                {
                    return gameServer;
                }
            }
            return null;
        }
    }
}