namespace BaseExchange.OrderFlow.UI.Models
{
    public class OrderRequest
    {
        public string Symbol { get; set; } = "";
        public int Side { get; set; } = 0;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
