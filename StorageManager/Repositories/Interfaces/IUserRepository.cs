using System.Collections.Generic;
using System.Threading.Tasks;
using StorageManager.Models;

namespace StorageManager.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();

        Task<User> GetByIdAsync(int id);

        Task<User> GetByUsernameOrEmailAsync(
            string value);

        Task<User> FindAsync(int id);

        Task<bool> ExistsAsync(
            string username,
            string email);

        Task<bool> UsernameExistsAsync(
            string username,
            int excludeId);

        Task<bool> EmailExistsAsync(
            string email,
            int excludeId);

        Task AddAsync(User user);

        void Remove(User user);

        Task SaveChangesAsync();
    }
}