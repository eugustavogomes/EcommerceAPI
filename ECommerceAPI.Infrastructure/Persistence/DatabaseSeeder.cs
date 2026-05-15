using ECommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ECommerceAPI.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    public static async Task SeedAdminAsync(AppDbContext db, IConfiguration config, ILogger logger)
    {
        if (await db.Users.AnyAsync(u => u.Role == "Admin")) return;

        var section = config.GetSection("AdminSeed");
        var email = section["Email"] ?? "admin@ecommerce.com";
        var password = section["Password"] ?? "Admin@123456";
        var fullName = section["FullName"] ?? "Admin";

        db.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = "Admin"
        });

        await db.SaveChangesAsync();
        logger.LogInformation("Admin user seeded: {Email}", email);
    }
}
