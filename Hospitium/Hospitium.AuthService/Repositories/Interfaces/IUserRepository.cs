using Hospitium.AuthService.Models;

namespace Hospitium.AuthService.Repositories.Interfaces
{
    /// <summary>
    /// Defines the contract for User data access operations.
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetAllByRoleAsync(string role);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
