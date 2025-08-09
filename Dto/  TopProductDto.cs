namespace NaimaBeauty.Dtos
{
    public class TopSellingProductDto
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
