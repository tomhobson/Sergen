using _8inServant.Services;
using _8inServant.Services.Containers;
using _8inServant.Services.ServerStore;
using Discord;
using Discord.WebSocket;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using _8inServant.Services.Chat.ChatEventHandler;
using _8inServant.Services.Chat.ChatProcessor;
using _8inServant.Services.Chat.ChatContext;

namespace _8inServant
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                // Add discord to the collection
                LogLevel = LogSeverity.Verbose, // Tell the logger to give Verbose amount of info
                MessageCacheSize = 1000 // Cache 1,000 messages per channel
            }));

            services.AddSingleton<IChatEventHandler, DiscordEventHandler>();
            services.AddSingleton<IChatProcessor, ChatProcessor>();
            services.AddSingleton<IChatContext, DiscordContext>();
            services.AddSingleton<IContainerInterface, DockerInterface>();
            services.AddSingleton<IServerStore, JsonServerStore>();
            services.AddHostedService<_8inBackgroundWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

        }
    }
}
