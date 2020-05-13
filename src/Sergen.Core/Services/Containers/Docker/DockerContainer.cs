using System;
using Docker.DotNet.Models;
using Sergen.Core.Data;
using Sergen.Core.Services.Chat.ChatResponseToken;

namespace Sergen.Core.Services.Containers.Docker
{
    public class DockerContainer
    {
        public string ID { get; set; } = Guid.NewGuid ().ToString ();

        public GameServer GameServer { get; set; }

        private readonly IChatResponseToken _icrt;

        private readonly string _initialMessageID;

        private DateTime _startTime = DateTime.UtcNow;
        
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
                var timeTaken = _lastUpdatedTime.Subtract(_startTime);
                // Update the message. Don't hang the thread.
                _icrt.Update (_initialMessageID, $"Current status is: {e.Status} \n Taken: {timeTaken.Seconds}s so far.");
            }
        }
    }
}