using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Domain.Repositories;

public interface IProductRepository
{
    Task<bool> SkuExistsAsync(string sku);
    Task AddAsync(Product product);
    Task<Product?> GetByIdAsync(Guid id);
    Task<Product?> GetActiveByIdAsync(Guid id);
    Task<(List<Product> Items, int TotalCount)> GetPagedActiveAsync(int pageNumber, int pageSize);
    Task<(List<Product> Items, int TotalCount)> SearchActiveAsync(
        string? name, decimal? minPrice, decimal? maxPrice, int pageNumber, int pageSize);
    Task<bool> HasStockAsync(Guid id, int quantity);
    Task SaveChangesAsync();
}
