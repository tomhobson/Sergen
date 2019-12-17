using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using _8inServant.Services.Chat.ChatEventHandler;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace _8inServant.Services
{
    public class _8inBackgroundWorker : IHostedService
    {
        private IChatEventHandler _chat;
        private ILogger _logger;
        public _8inBackgroundWorker(IChatEventHandler chat, ILogger<_8inBackgroundWorker> logger)
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
