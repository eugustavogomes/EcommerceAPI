using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Domain.Repositories;
using ECommerceAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Infrastructure.Repositories;

public class CartRepository(AppDbContext db) : ICartRepository
{
    public async Task<Cart?> GetWithItemsAsync(Guid userId) =>
        await db.Carts
            .Include(c => c.Items)
            .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

    public async Task AddAsync(Cart cart) =>
        await db.Carts.AddAsync(cart);

    public Task ClearItemsAsync(Cart cart)
    {
        db.CartItems.RemoveRange(cart.Items);
        cart.Items.Clear();
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await db.SaveChangesAsync();
}
