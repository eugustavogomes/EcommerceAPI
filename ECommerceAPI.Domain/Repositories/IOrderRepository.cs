using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Domain.Repositories;

public interface IOrderRepository
{
    Task AddAsync(Order order);
    Task<Order?> GetByIdWithItemsAsync(Guid id);
    Task<(List<Order> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize);
    Task<(List<Order> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize);
    Task SaveChangesAsync();
}
