using ECommerceAPI.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerceAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<ProductService>();
        services.AddScoped<CartService>();
        services.AddScoped<OrderService>();

        return services;
    }
}
