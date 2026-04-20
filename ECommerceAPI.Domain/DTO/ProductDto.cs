namespace ECommerceAPI.Domain;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int Stock,
    string Sku,
    bool IsActive,
    DateTime CreatedAt
);

public record CreateProductDto(string Name, string? Description, decimal Price, int Stock, string Sku);

public record UpdateProductDto(string Name, string? Description, decimal Price, int Stock);
