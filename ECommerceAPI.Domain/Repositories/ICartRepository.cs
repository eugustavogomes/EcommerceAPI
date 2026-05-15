using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Domain.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetWithItemsAsync(Guid userId);
    Task AddAsync(Cart cart);
    Task ClearItemsAsync(Cart cart);
    Task SaveChangesAsync();
}
