using System.Collections.Generic;

namespace Sergen.Core.Services.Chat.StaticHelpers
{
    public static class ObjectToString
    {
        public static string Convert (IList<string> inputList)
        {
            string allText = "";
            foreach (var stri in inputList)
            {
                allText = allText + $"\n - {stri}";
            }
            return allText;
        }
        
        public static string Convert (Dictionary<string,string> inputList)
        {
            string allText = "";
            foreach (var pair in inputList)
            {
                allText = allText + $"\n {pair.Key} {pair.Value}";
            }
            return allText;
        }
    }
}