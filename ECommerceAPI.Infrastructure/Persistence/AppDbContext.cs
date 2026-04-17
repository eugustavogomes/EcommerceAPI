using ECommerceAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}
