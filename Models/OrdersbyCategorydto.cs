namespace NaimaBeauty.Dtos
{
    public class OrdersByCategoryDto
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
