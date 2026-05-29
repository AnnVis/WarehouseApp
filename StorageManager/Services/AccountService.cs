using System;
using System.Linq;
using System.Threading.Tasks;
using StorageManager.Models;
using StorageManager.Repositories.Interfaces;
using StorageManager.Services.Interfaces;

namespace StorageManager.Services
{
    public class AccountService : IAccountService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly TokenService _tokenService;

        public AccountService(
            IUserRepository userRepository,
            IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _tokenService = new TokenService();
        }

        public async Task RegisterAsync(
            RegisterRequest model)
        {
            var exists =
                await _userRepository.ExistsAsync(
                    model.Username,
                    model.Email);

            if (exists)
            {
                throw new Exception(
                    "Username or email already exists.");
            }

            PasswordHasher.CreatePasswordHash(
                model.Password,
                out var hash,
                out var salt);

            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Phone = model.Phone,
                Address = model.Address,
                PasswordHash = hash,
                PasswordSalt = salt
            };

            await _userRepository.AddAsync(user);

            await _userRepository.SaveChangesAsync();

            var role =
                await _roleRepository
                    .GetByNameAsync("User");

            if (role == null)
            {
                role = new Role
                {
                    Name = "User"
                };

                await _roleRepository
                    .AddAsync(role);

                await _roleRepository
                    .SaveChangesAsync();
            }

            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id
            });

            await _userRepository.SaveChangesAsync();
        }

        public async Task<LoginResponse> LoginAsync(
            LoginRequest model)
        {
            var user =
                await _userRepository
                    .GetByUsernameOrEmailAsync(
                        model.UsernameOrEmail);

            if (user == null)
            {
                throw new Exception(
                    "Invalid credentials.");
            }

            var valid =
                PasswordHasher.VerifyPassword(
                    model.Password,
                    user.PasswordHash,
                    user.PasswordSalt);

            if (!valid)
            {
                throw new Exception(
                    "Invalid credentials.");
            }

            var token =
                _tokenService.GenerateJwt(
                    user,
                    TimeSpan.FromHours(8));

            return new LoginResponse
            {
                Token = token,
                ExpiresIn = 8 * 3600
            };
        }

        public async Task<UserDto>
            GetCurrentUserAsync(int userId)
        {
            var user =
                await _userRepository
                    .GetByIdAsync(userId);

            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                CreatedAt = user.CreatedAt,
                Roles = user.UserRoles
                    .Select(ur => ur.Role.Name)
                    .ToList()
            };
        }

        public async Task AssignRoleAsync(
            int userId,
            string roleName)
        {
            var user =
                await _userRepository
                    .GetByIdAsync(userId);

            if (user == null)
            {
                throw new Exception(
                    "User not found.");
            }

            var role =
                await _roleRepository
                    .GetByNameAsync(roleName);

            if (role == null)
            {
                role = new Role
                {
                    Name = roleName
                };

                await _roleRepository
                    .AddAsync(role);

                await _roleRepository
                    .SaveChangesAsync();
            }

            var exists = user.UserRoles
                .Any(ur => ur.RoleId == role.Id);

            if (!exists)
            {
                user.UserRoles.Add(
                    new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });

                await _userRepository
                    .SaveChangesAsync();
            }
        }
    }
}