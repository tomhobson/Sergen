namespace Sergen.Core.Services.Chat.StaticHelpers
{
    public class ChatHelper
    {
        public static string PreParseInputString(string message)
        {
            return message.ToLower().Replace(' ', '-');
        }
    }
}