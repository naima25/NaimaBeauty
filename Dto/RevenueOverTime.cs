public class RevenueOverTimeDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
