using ECommerceAPI.Domain;
using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Domain.Repositories;

namespace ECommerceAPI.Application.Services;

public class OrderService(
    IOrderRepository orderRepository,
    ICartRepository cartRepository,
    IProductRepository productRepository)
{
    public async Task<OrderDto> CheckoutAsync(Guid userId)
    {
        var cart = await cartRepository.GetWithItemsAsync(userId)
            ?? throw new KeyNotFoundException("Cart not found.");

        if (!cart.Items.Any())
            throw new InvalidOperationException("Cart is empty.");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Items = []
        };

        foreach (var cartItem in cart.Items)
        {
            var product = await productRepository.GetActiveByIdAsync(cartItem.ProductId)
                ?? throw new InvalidOperationException($"Product '{cartItem.ProductId}' is no longer available.");

            if (product.Stock < cartItem.Quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for '{product.Name}'. Available: {product.Stock}.");

            order.Items.Add(new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity
            });

            product.Stock -= cartItem.Quantity;
            product.UpdatedAt = DateTime.UtcNow;
        }

        order.Total = order.Items.Sum(i => i.UnitPrice * i.Quantity);

        await orderRepository.AddAsync(order);
        await cartRepository.ClearItemsAsync(cart);
        cart.UpdatedAt = DateTime.UtcNow;

        // all repositories share the same DbContext (scoped), one save persists everything
        await orderRepository.SaveChangesAsync();

        return MapToDto(order);
    }

    public async Task<(List<OrderDto> Orders, int TotalCount)> GetUserOrdersAsync(
        Guid userId, int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var (orders, total) = await orderRepository.GetByUserIdAsync(userId, pageNumber, pageSize);
        return (orders.Select(MapToDto).ToList(), total);
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid orderId, Guid userId, bool isAdmin)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(orderId)
            ?? throw new KeyNotFoundException($"Order '{orderId}' not found.");

        if (!isAdmin && order.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        return MapToDto(order);
    }

    public async Task<OrderDto> CancelOrderAsync(Guid orderId, Guid userId, bool isAdmin)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(orderId)
            ?? throw new KeyNotFoundException($"Order '{orderId}' not found.");

        if (!isAdmin && order.UserId != userId)
            throw new UnauthorizedAccessException("Access denied.");

        if (order.Status is OrderStatus.Shipped or OrderStatus.Delivered)
            throw new InvalidOperationException($"Cannot cancel an order with status '{order.Status}'.");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Order is already cancelled.");

        foreach (var item in order.Items)
        {
            var product = await productRepository.GetByIdAsync(item.ProductId);
            if (product is not null)
            {
                product.Stock += item.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
            }
        }

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepository.SaveChangesAsync();
        return MapToDto(order);
    }

    public async Task<(List<OrderDto> Orders, int TotalCount)> GetAllOrdersAsync(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var (orders, total) = await orderRepository.GetAllAsync(pageNumber, pageSize);
        return (orders.Select(MapToDto).ToList(), total);
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, OrderStatus newStatus)
    {
        var order = await orderRepository.GetByIdWithItemsAsync(orderId)
            ?? throw new KeyNotFoundException($"Order '{orderId}' not found.");

        if (order.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot change status of a cancelled order.");

        order.Status = newStatus;
        order.UpdatedAt = DateTime.UtcNow;

        await orderRepository.SaveChangesAsync();
        return MapToDto(order);
    }

    private static OrderDto MapToDto(Order order) => new(
        order.Id,
        order.Status,
        order.Status.ToString(),
        order.Items.Select(i => new OrderItemDto(
            i.ProductId,
            i.ProductName,
            i.UnitPrice,
            i.Quantity,
            i.UnitPrice * i.Quantity
        )).ToList(),
        order.Total,
        order.CreatedAt,
        order.UpdatedAt
    );
}
