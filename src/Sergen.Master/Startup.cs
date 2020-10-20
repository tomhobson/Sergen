using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sergen.Core.Services.Containers;
using Sergen.Core.Services.Containers.Docker;
using Sergen.Core.Services.IpGetter;
using Sergen.Core.Services.ServerStore;
using Sergen.Master.Services;
using Sergen.Master.Services.Chat.ChatContext;
using Sergen.Master.Services.Chat.ChatEventHandler;
using Sergen.Master.Services.Chat.ChatProcessor;

namespace Sergen.Master
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                // Add discord to the collection
                LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000 // Cache 1,000 messages per channel
            }));

            services.AddHostedService<SergenBackgroundWorker>();
            services.AddTransient<IChatProcessor, ChatProcessor>();
            services.AddTransient<IChatEventHandler, DiscordEventHandler>();
            services.AddTransient<IChatContext, DiscordContext>();
            services.AddTransient<IContainerInterface, DockerInterface>();
            services.AddTransient<IServerStore, JsonServerStore>();
            services.AddTransient<IIpGetter, ipecho>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }
}
