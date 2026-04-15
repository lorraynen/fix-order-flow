using BaseExchange.OrderFlow.UI.Models;
namespace BaseExchange.OrderFlow.UI.Services
{   
    public class ExposureState
    {
        public List<ExposureDto> Data { get; private set; } = new();

        public event Action? OnChange;

        public void Set(List<ExposureDto> data)
        {
            Data = data;
            OnChange?.Invoke();
        }
    }
}
