using System.Collections.Generic;
using System.Threading.Tasks;
using StorageManager.Models;

namespace StorageManager.Services.Interfaces
{
    public interface IItemService
    {
        Task<List<ItemDto>> GetAllAsync();

        Task<ItemDto> GetByIdAsync(int id);

        Task<ItemDto> CreateAsync(
            CreateItemRequest model,
            string currentUsername);

        Task<bool> UpdateAsync(
            int id,
            UpdateItemRequest model);

        Task<bool> DeleteAsync(int id);
    }
}