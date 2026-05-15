using ECommerceAPI.Domain.Entities;
using ECommerceAPI.Domain.Repositories;
using ECommerceAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ECommerceAPI.Infrastructure.Repositories;

public class UserRepository(AppDbContext db) : IUserRepository
{
    public async Task<bool> EmailExistsAsync(string email) =>
        await db.Users.AnyAsync(u => u.Email == email);

    public async Task AddAsync(User user) =>
        await db.Users.AddAsync(user);

    public async Task<User?> GetByEmailAsync(string email) =>
        await db.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByIdAsync(Guid id) =>
        await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task SaveChangesAsync() => await db.SaveChangesAsync();
}
