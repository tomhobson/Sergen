using Discord;

namespace Sergen.Core.Services.Chat.StaticHelpers
{
    public static class DiscordHelper
    {
        public static Embed CreateEmbeddedMessage(string message)
        {
            EmbedBuilder builder = new EmbedBuilder();

            builder.WithTitle("Sergen");

            builder.WithDescription(message);

            builder.WithColor(Color.Green);
            
            return builder.Build();
        }
    }
}