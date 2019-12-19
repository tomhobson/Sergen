using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Sergen.Core.Services.IpGetter
{
    public class ipecho : IIPGetter
    {
        private readonly HttpClient _httpClient;

        public ipecho()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://ipecho.net/")
            };
        }

        public async Task<string> GetIP()
        {
            try
            {
                var response = await _httpClient.GetAsync("plain");
                if(response.IsSuccessStatusCode)
                {
                    var ip = await response.Content.ReadAsStringAsync();
                    return ip;
                }
                return "Unknown";
            }
            catch(Exception ex)
            {
                return "Unknown";
            }
        }
    }
}