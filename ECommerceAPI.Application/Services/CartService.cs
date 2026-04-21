using ECommerceAPI.Domain;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Application.Services;

public class CartService(AppDbContext db, ProductService productService)
{
    public async Task<CartDto> GetCartAsync(Guid userId)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException($"Cart for user '{userId}' not found.");

        return MapToDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(Guid userId, Guid productId, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0.");

        if (!await productService.HasStockAsync(productId, quantity))
            throw new InvalidOperationException("Product not available or insufficient stock.");

        var cart = await db.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException($"Cart for user '{userId}' not found.");

        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId && p.IsActive)
            ?? throw new KeyNotFoundException("Product not found.");

        cart.AddItem(product, quantity);
        cart.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToDto(cart);
    }

    public async Task<CartDto> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException($"Cart for user '{userId}' not found.");

        if (!cart.Items.Any(i => i.ProductId == productId))
            throw new KeyNotFoundException($"Product '{productId}' not in cart.");

        cart.RemoveItem(productId);
        cart.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToDto(cart);
    }

    public async Task<CartDto> UpdateItemQuantityAsync(Guid userId, Guid productId, int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than 0.");

        var cart = await db.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException($"Cart for user '{userId}' not found.");

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId)
            ?? throw new KeyNotFoundException($"Product '{productId}' not in cart.");

        if (!await productService.HasStockAsync(productId, newQuantity))
            throw new InvalidOperationException("Insufficient stock for the requested quantity.");

        item.Quantity = newQuantity;
        cart.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToDto(cart);
    }

    public async Task<CartDto> ClearCartAsync(Guid userId)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new KeyNotFoundException($"Cart for user '{userId}' not found.");

        db.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToDto(cart);
    }

    public async Task<(bool IsValid, string? ErrorMessage)> ValidateCartForCheckoutAsync(Guid userId)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart is null) return (false, "Cart not found.");
        if (!cart.Items.Any()) return (false, "Cart is empty.");

        foreach (var item in cart.Items)
        {
            if (item.Product is null || !item.Product.IsActive)
                return (false, $"Product '{item.ProductId}' is no longer available.");

            if (item.Product.Stock < item.Quantity)
                return (false, $"Insufficient stock for '{item.Product.Name}'. Available: {item.Product.Stock}.");
        }

        return (true, null);
    }

    private static CartDto MapToDto(Cart cart) => new()
    {
        Id = cart.Id,
        Items = cart.Items.Select(ci => new CartItemDto
        {
            ProductId = ci.ProductId,
            ProductName = ci.Product?.Name ?? "Unknown",
            ProductPrice = ci.Product?.Price ?? 0,
            Quantity = ci.Quantity
        }).ToList(),
        Total = cart.Items.Sum(ci => (ci.Product?.Price ?? 0) * ci.Quantity)
    };
}
