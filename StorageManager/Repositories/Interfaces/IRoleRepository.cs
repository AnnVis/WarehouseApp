using System.Threading.Tasks;
using StorageManager.Models;

namespace StorageManager.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> GetByNameAsync(string roleName);

        Task AddAsync(Role role);

        Task SaveChangesAsync();
    }
}