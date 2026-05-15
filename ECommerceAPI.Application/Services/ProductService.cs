using ECommerceAPI.Domain;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Domain.Repositories;

namespace ECommerceAPI.Application;

public class ProductService(IProductRepository productRepository)
{
    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto)
    {
        if (await productRepository.SkuExistsAsync(dto.Sku))
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

        await productRepository.AddAsync(product);
        await productRepository.SaveChangesAsync();

        return MapToDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        var product = await productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product '{id}' not found.");

        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Product name is required.");
        if (dto.Price <= 0) throw new ArgumentException("Price must be greater than 0.");
        if (dto.Stock < 0) throw new ArgumentException("Stock cannot be negative.");

        product.Name = dto.Name.Trim();
        product.Description = dto.Description?.Trim() ?? string.Empty;
        product.Price = dto.Price;
        product.Stock = dto.Stock;
        product.UpdatedAt = DateTime.UtcNow;

        await productRepository.SaveChangesAsync();
        return MapToDto(product);
    }

    public async Task DeleteProductAsync(Guid id)
    {
        var product = await productRepository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product '{id}' not found.");

        product.IsActive = false;
        product.UpdatedAt = DateTime.UtcNow;
        await productRepository.SaveChangesAsync();
    }

    public async Task<ProductDto> GetByIdAsync(Guid id)
    {
        var product = await productRepository.GetActiveByIdAsync(id)
            ?? throw new KeyNotFoundException($"Product '{id}' not found.");

        return MapToDto(product);
    }

    public async Task<(List<ProductDto> Products, int TotalCount)> GetProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var (products, total) = await productRepository.GetPagedActiveAsync(pageNumber, pageSize);
        return (products.Select(MapToDto).ToList(), total);
    }

    public async Task<(List<ProductDto> Products, int TotalCount)> SearchProductsAsync(
        string? name, decimal? minPrice, decimal? maxPrice, int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var (products, total) = await productRepository.SearchActiveAsync(name, minPrice, maxPrice, pageNumber, pageSize);
        return (products.Select(MapToDto).ToList(), total);
    }

    public Task<bool> HasStockAsync(Guid id, int quantity) =>
        productRepository.HasStockAsync(id, quantity);

    private static ProductDto MapToDto(Product p) =>
        new(p.Id, p.Name, p.Description, p.Price, p.Stock, p.Sku, p.IsActive, p.CreatedAt);
}
