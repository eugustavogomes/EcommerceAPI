using ECommerceAPI.Domain.Entities;

namespace ECommerceAPI.Domain.Repositories;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task SaveChangesAsync();
}
