using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace _8inServant.Services
{
    public class _8inBackgroundWorker : IHostedService
    {
        private IChat _chat;
        private ILogger _logger;
        public _8inBackgroundWorker(IChat chat, ILogger<_8inBackgroundWorker> logger)
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
