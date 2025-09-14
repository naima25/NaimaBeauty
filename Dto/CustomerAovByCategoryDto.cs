public class CustomerAovByCategoryDto
{

    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public decimal AverageOrderValue { get; set; }
}
