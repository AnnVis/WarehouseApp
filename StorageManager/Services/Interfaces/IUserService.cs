using System.Collections.Generic;
using System.Threading.Tasks;
using StorageManager.Models;

namespace StorageManager.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();

        Task<UserDto> GetByIdAsync(int id);

        Task<UserDto> CreateAsync(CreateUserRequest model);

        Task<bool> UpdateAsync(int id, UpdateUserRequest model);

        Task<bool> DeleteAsync(int id);
    }
}