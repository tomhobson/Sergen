using System;
using _8inServant.Data;
using _8inServant.Services.Chat.ChatResponseToken;
using Docker.DotNet.Models;

namespace _8inServant.Services.Containers.Docker
{
    public class DockerContainer
    {
        public string ID { get; set; } = Guid.NewGuid ().ToString ();

        public GameServer GameServer { get; set; }

        private IChatResponseToken _icrt;

        private string _initialMessageID;

        private DateTime _lastUpdatedTime = DateTime.UtcNow;

        public DockerContainer (IChatResponseToken icrt, GameServer gs)
        {
            _icrt = icrt;
            GameServer = gs;

            var task = _icrt.Respond($"Current status is: Starting...");

            _initialMessageID = task.Result;
        }

        internal async void HandleUpdate (object sender, JSONMessage e)
        {
            var milliseconds = DateTime.UtcNow.Subtract(_lastUpdatedTime).TotalMilliseconds;
            if (milliseconds > 1500)
            {
                _lastUpdatedTime = DateTime.UtcNow;
                // Update the message. Don't hang the thread.
                _icrt.Update (_initialMessageID, $"Current status is: {e.Status}");
            }
        }
    }
}