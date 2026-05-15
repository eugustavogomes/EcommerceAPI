using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Domain.Repositories;
using ECommerceAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Infrastructure.Repositories;

public class OrderRepository(AppDbContext db) : IOrderRepository
{
    public async Task AddAsync(Order order) =>
        await db.Orders.AddAsync(order);

    public async Task<Order?> GetByIdWithItemsAsync(Guid id) =>
        await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<(List<Order> Items, int TotalCount)> GetByUserIdAsync(Guid userId, int pageNumber, int pageSize)
    {
        var query = db.Orders
            .Where(o => o.UserId == userId)
            .Include(o => o.Items)
            .AsNoTracking();

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = db.Orders
            .Include(o => o.Items)
            .AsNoTracking();

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task SaveChangesAsync() => await db.SaveChangesAsync();
}
