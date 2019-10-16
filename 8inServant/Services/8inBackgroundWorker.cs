using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace _8inServant.Services
{
    public class _8inBackgroundWorker : BackgroundService
    {
        private IChat _chat;
        private ILogger _logger;
        public _8inBackgroundWorker(IChat chat, ILogger<_8inBackgroundWorker> logger)
        {
            _chat = chat;
            _logger = logger;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
