using System.Collections.Generic;

namespace Sergen.Core.Services.Chat.StaticHelpers
{
    public static class ListToStringList
    {
        public static string Convert (IList<string> inputList)
        {
            string allText = "";
            foreach (var stri in inputList)
            {
                allText = allText + $"\n {stri}";
            }
            return allText;
        }
    }
}