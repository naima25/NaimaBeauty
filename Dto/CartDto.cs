
public class CartDto
{
    public int Id { get; set; }             // Include Id if updating existing cart
    public string CustomerId { get; set; }
    public decimal Price { get; set; }
    public List<CartItemDto> CartItems { get; set; }
}