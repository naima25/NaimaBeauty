public class RevenueOverTimeDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }

    // Optional, only if you want to send category info
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
