using System;
using Discord;

namespace Sergen.Main.Services.Chat.ChatEventHandler.Discord
{
    public class ListeningActivity : IActivity
    {
        public string Name { get; }
        public ActivityType Type { get; }
        public ActivityProperties Flags { get; }
        public string Details { get; }

        public ListeningActivity(string name)
        {
            Name = name;
            Details = String.Empty;
            Type = ActivityType.CustomStatus;
            Flags = ActivityProperties.None;
        }
    }
}