using System.Collections.Generic;

namespace Sergen.Core.Data
{
    public class GameServer
    {
        public string ServerName { get; set; }

        public string ContainerName { get; set; }

        public string ContainerType { get; set; }

        public string ContainerTag { get; set; } = "latest";

        public Dictionary<string, string> Ports { get; set; }

        public Dictionary<string, string> EnvironmentalVariables { get; set; }
        
        public IList<string> Binds { get; set; }
        
        public IList<string> Commands { get; set; }
    }
}