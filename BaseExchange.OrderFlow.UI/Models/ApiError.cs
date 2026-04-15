namespace BaseExchange.OrderFlow.UI.Models
{
    public class ApiError
    {
        public bool success { get; set; }
        public string message { get; set; } = "";
        public List<string> errors { get; set; } = new();
    }
}
