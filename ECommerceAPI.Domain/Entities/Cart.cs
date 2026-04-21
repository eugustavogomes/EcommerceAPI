namespace ECommerceAPI.Domain.Entities;

public class Cart
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItem> Items { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void AddItem(Product product, int quantity)
    {
        var existing = Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing is not null)
            existing.Quantity += quantity;
        else
            Items.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                Product = product,
                Quantity = quantity
            });
    }

    public void RemoveItem(Guid productId) =>
        Items.RemoveAll(i => i.ProductId == productId);
}
