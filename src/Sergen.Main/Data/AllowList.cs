using System.Collections.Generic;

namespace Sergen.Main.Data
{
    public class AllowList
    {
        public bool Enabled { get; set; } = false;
        public IList<string> AllowedIds { get; set; }
    }
}