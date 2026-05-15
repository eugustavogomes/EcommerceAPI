using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Domain.Repositories;
using ECommerceAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Infrastructure.Repositories;

public class ProductRepository(AppDbContext db) : IProductRepository
{
    public async Task<bool> SkuExistsAsync(string sku) =>
        await db.Products.AnyAsync(p => p.Sku == sku);

    public async Task AddAsync(Product product) =>
        await db.Products.AddAsync(product);

    public async Task<Product?> GetByIdAsync(Guid id) =>
        await db.Products.FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product?> GetActiveByIdAsync(Guid id) =>
        await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

    public async Task<(List<Product> Items, int TotalCount)> GetPagedActiveAsync(int pageNumber, int pageSize)
    {
        var query = db.Products.Where(p => p.IsActive).AsNoTracking();
        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<(List<Product> Items, int TotalCount)> SearchActiveAsync(
        string? name, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize)
    {
        var query = db.Products.Where(p => p.IsActive).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = $"%{name.Trim()}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, term) ||
                EF.Functions.ILike(p.Description, term));
        }

        if (minPrice > 0) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice > 0) query = query.Where(p => p.Price <= maxPrice.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<bool> HasStockAsync(Guid id, int quantity)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        return product != null && product.Stock >= quantity;
    }

    public async Task SaveChangesAsync() => await db.SaveChangesAsync();
}
