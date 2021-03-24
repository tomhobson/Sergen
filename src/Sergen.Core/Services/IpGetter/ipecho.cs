using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Sergen.Core.Services.IpGetter
{
    public class ipecho : IIpGetter
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;

        public ipecho(ILogger<ipecho> logger)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://ipecho.net/")
            };
            _logger = logger;
        }

        public async Task<string> GetIp()
        {
            try
            {
                var response = await _httpClient.GetAsync("plain");
                if(response.IsSuccessStatusCode)
                {
                    var ip = await response.Content.ReadAsStringAsync();
                    return ip;
                }
                _logger.LogWarning($"Unable to obtain ip. Status code: {response.StatusCode.ToString()}");
                return "Unknown";
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unable to obtain ip.");
                return "Unknown";
            }
        }
    }
}