using BaseExchange.OrderFlow.OrderAccumulator.Application.DTOs;

namespace BaseExchange.OrderFlow.Api.Client
{
    
    public class ExposureApiClient
    {
        private readonly HttpClient _http;

        public ExposureApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ExposureDTO>> GetExposure()
        {
            return await _http.GetFromJsonAsync<List<ExposureDTO>>("/exposure") ?? new();
        }
    }

}
