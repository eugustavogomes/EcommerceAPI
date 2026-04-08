using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // DbSet<Entidade> Entidades { get; set; } vai aqui conforme as entidades forem criadas
}
