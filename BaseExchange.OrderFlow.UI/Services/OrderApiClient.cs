using BaseExchange.OrderFlow.UI.Models;
using System.Net.Http.Json;

namespace BaseExchange.OrderFlow.UI.Services
{
    
    public class OrderApiClient
    {
        private readonly HttpClient _http;

        public OrderApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task SendOrder(OrderRequest order)
        {
            var response = await _http.PostAsJsonAsync("/api/orders", order);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }

        public async Task<List<ExposureDto>> GetExposure()
        {
            return await _http.GetFromJsonAsync<List<ExposureDto>>("/api/exposure") ?? new();
        }
    }
}

