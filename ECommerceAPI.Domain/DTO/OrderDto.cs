using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Domain;

public record OrderItemDto(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal Subtotal
);

public record OrderDto(
    Guid Id,
    OrderStatus Status,
    string StatusLabel,
    List<OrderItemDto> Items,
    decimal Total,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record UpdateOrderStatusDto(OrderStatus Status);

public record UpdateUserRoleDto(string Role);
