using System.Linq;
using System.Collections.Generic;
using System.IO;
using _8inServant.Data;
using Newtonsoft.Json;

namespace _8inServant.Services.ServerStore
{
    public class JsonServerStore : IServerStore
    {
        private string _path = "";

        public JsonServerStore(){
            _path = Path.Combine("GameServers");
        }

        public IList<GameServer> GetAllServers(string containerType)
        {
            List<GameServer> servers = new List<GameServer>();

            var files = Directory.GetFiles(_path);

            foreach(var file in files)
            {
                var text = File.ReadAllText(file);
                var gameServer = JsonConvert.DeserializeObject<GameServer>(text);
                if(gameServer.ContainerType == containerType)
                {
                    servers.Add(gameServer);
                }
            }
            return servers;
        }

        public GameServer GetGameServerByName(string serverName, string containerType)
        {
            var files = Directory.GetFiles(_path);

            foreach(var file in files)
            {
                var text = File.ReadAllText(file);
                var gameServer = JsonConvert.DeserializeObject<GameServer>(text);
                if(gameServer.ContainerType == containerType && gameServer.ServerName == serverName)
                {
                    return gameServer;
                }
            }
            return null;
        }
    }
}