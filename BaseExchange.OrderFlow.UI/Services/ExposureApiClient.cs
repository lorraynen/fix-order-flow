using BaseExchange.OrderFlow.UI.Models;
using System.Net.Http.Json;

namespace BaseExchange.OrderFlow.UI.Services
{
    
    public class ExposureApiClient
    {
        private readonly HttpClient _http;

        public ExposureApiClient(HttpClient http)
        {
            _http = http;
        }

       

        public async Task<List<object>> GetExposure()
        {
            return await _http.GetFromJsonAsync<List<object>>("/exposure") ?? new();
        }
    }
}

