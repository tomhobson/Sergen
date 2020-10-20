using System;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger _logger;

        private DateTime _startTime = DateTime.UtcNow;
        
        private DateTime _lastUpdatedTime = DateTime.UtcNow;

        public DockerContainer (ILogger logger, IChatResponseToken icrt, GameServer gs)
        {
            _icrt = icrt;
            _logger = logger;
            GameServer = gs;

            var task = _icrt.Respond($"Current status is: Initialising...");

            _initialMessageID = task.Result;
        }

        internal async void HandleUpdate (object sender, JSONMessage e)
        {
            var milliseconds = DateTime.UtcNow.Subtract(_lastUpdatedTime).TotalMilliseconds;
            _logger.LogDebug($"Status:{e.Status}, Message:{e.ProgressMessage}");
            if (milliseconds > 1500)
            {
                _lastUpdatedTime = DateTime.UtcNow;
                var timeTaken = _lastUpdatedTime.Subtract(_startTime);
                // Update the message. Don't hang the thread.
                await _icrt.Update (_initialMessageID, $"Current status is: {e.Status} \n Taken: {timeTaken.Seconds}s so far.");
            }
        }
    }
}