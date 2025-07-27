using NaimaBeauty.Dtos;

public class OrderDto
{
    public string CustomerId { get; set; }
    public decimal Price { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
}