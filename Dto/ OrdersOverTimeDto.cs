public class OrdersOverTimeDto
{
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }

    // New property for category info
    public int? CategoryId { get; set; }
    public string? CategoryName { get; set; }
}
