using System.Collections.Generic;

namespace Sergen.Core.Data
{
    public class GameServer
    {
        public string ServerName { get; set; }

        public string ContainerName { get; set; }

        public string ContainerType { get; set; }
        
        public string ContainerTag { get; set; }

        public IList<string> Ports { get; set; }

        public Dictionary<string, string> EnvironmentalVariables { get; set; }
        
        public IList<string> Binds { get; set; }
    }
}