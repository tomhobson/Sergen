using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Sergen.Services.Chat.ChatEventHandler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sergen.Services
{
    public class SergenBackgroundWorker : IHostedService
    {
        private IChatEventHandler _chat;
        private ILogger _logger;
        public SergenBackgroundWorker(IChatEventHandler chat, ILogger<SergenBackgroundWorker> logger)
        {
            _chat = chat;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _chat.Connect();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
        
        }
    }
}
