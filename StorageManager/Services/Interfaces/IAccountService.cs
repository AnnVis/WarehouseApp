using System.Threading.Tasks;
using StorageManager.Models;

namespace StorageManager.Services.Interfaces
{
    public interface IAccountService
    {
        Task RegisterAsync(RegisterRequest model);

        Task<LoginResponse> LoginAsync(LoginRequest model);

        Task<UserDto> GetCurrentUserAsync(int userId);

        Task AssignRoleAsync(int userId, string roleName);
    }
}