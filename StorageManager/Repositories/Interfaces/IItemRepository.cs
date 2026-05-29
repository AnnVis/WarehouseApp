using System.Collections.Generic;
using System.Threading.Tasks;
using StorageManager.Models;

namespace StorageManager.Repositories.Interfaces
{
    public interface IItemRepository
    {
        Task<List<Item>> GetAllAsync();

        Task<Item> GetByIdAsync(int id);

        Task<bool> CodeExistsAsync(
            string code,
            int excludeId = 0);

        Task AddAsync(Item item);

        void Remove(Item item);

        Task SaveChangesAsync();
    }
}