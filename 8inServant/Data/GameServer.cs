using System.Collections.Generic;

namespace _8inServant.Data
{
    public class GameServer
    {
        public string ServerName { get; set; }

        public string ContainerName { get; set; }

        public string ContainerType { get; set; }

        public IList<string> Ports { get; set; }

        public Dictionary<string, string> EnvironmentalVariables { get; set; }
    }
}