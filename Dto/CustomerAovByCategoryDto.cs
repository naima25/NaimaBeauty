public class CustomerAovByCategoryDto
{
    // You can remove CustomerId if we're ignoring customers for this visualization
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    // Add the Date property for the X-axis
    public DateTime Date { get; set; }

    public decimal AverageOrderValue { get; set; }
}
