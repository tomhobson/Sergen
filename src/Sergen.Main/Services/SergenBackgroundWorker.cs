using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sergen.Main.Services.Chat.ChatEventHandler;

namespace Sergen.Main.Services
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
