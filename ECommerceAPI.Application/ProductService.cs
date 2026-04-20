using ECommerceAPI.Domain;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Application;

public class ProductService(AppDbContext db)
{
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        if (await db.Products.AnyAsync(p => p.Sku == dto.Sku))
            throw new InvalidOperationException($"Product with SKU '{dto.Sku}' already exists.");

        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Product name is required.");
        if (dto.Price <= 0) throw new ArgumentException("Price must be greater than 0.");
        if (dto.Stock < 0) throw new ArgumentException("Stock cannot be negative.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Price = dto.Price,
            Stock = dto.Stock,
            Sku = dto.Sku.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        db.Products.Add(product);
        await db.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product '{id}' not found.");

        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Product name is required.");
        if (dto.Price <= 0) throw new ArgumentException("Price must be greater than 0.");
        if (dto.Stock < 0) throw new ArgumentException("Stock cannot be negative.");

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return MapToDto(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product '{id}' not found.");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.IsActive)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product '{id}' not found.");

        return MapToDto(product);
    }

    public async Task<(List<ProductDto> Products, int TotalCount)> GetProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = db.Products.Where(p => p.IsActive).AsNoTracking();
        var totalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products.Select(MapToDto).ToList(), totalCount);
    }

    public async Task<(List<ProductDto> Products, int TotalCount)> SearchProductsAsync(
        string? name, decimal? minPrice, decimal? maxPrice, int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = db.Products.Where(p => p.IsActive).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = $"%{name.Trim()}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, term) || EF.Functions.ILike(p.Description, term));
        }

        if (minPrice > 0) query = query.Where(p => p.Price >= minPrice.Value);
        if (maxPrice > 0) query = query.Where(p => p.Price <= maxPrice.Value);

        var totalCount = await query.CountAsync();

        var products = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (products.Select(MapToDto).ToList(), totalCount);
    }

    public async Task ReduceStockAsync(Guid id, int quantity)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product '{id}' not found.");

        if (product.Stock < quantity)
            throw new InvalidOperationException($"Insufficient stock. Available: {product.Stock}, Requested: {quantity}.");

        product.Stock -= quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task IncreaseStockAsync(Guid id, int quantity)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"Product '{id}' not found.");

        product.Stock += quantity;
        product.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task<bool> HasStockAsync(Guid id, int quantity)
    {
        var product = await db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

        return product != null && product.Stock >= quantity;
    }

    private static ProductDto MapToDto(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Sku, p.IsActive, p.CreatedAt);
}
