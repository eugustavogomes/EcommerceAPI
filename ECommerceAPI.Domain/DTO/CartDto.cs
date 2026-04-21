namespace ECommerceAPI.Domain;

public record AddToCartDto(Guid ProductId, int Quantity);
public record RemoveFromCartDto(Guid ProductId);
public record UpdateCartItemDto(Guid ProductId, int Quantity);

public class CartDto
{
    public Guid Id { get; set; }
    public List<CartItemDto> Items { get; set; } = [];
    public decimal Total { get; set; }
}

public class CartItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
    public int Quantity { get; set; }
}
