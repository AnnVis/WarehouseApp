using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using StorageManager.Data;
using StorageManager.Models;
using StorageManager.Services.Interfaces;

namespace StorageManager.Services
{
    public class AccountService : IAccountService
    {
        private readonly ApplicationDbContext _db;
        private readonly TokenService _tokenService;

        public AccountService(ApplicationDbContext db)
        {
            _db = db;
            _tokenService = new TokenService();
        }

        public async Task RegisterAsync(RegisterRequest model)
        {
            var exists = await _db.Users.AnyAsync(u =>
                u.Username == model.Username ||
                u.Email == model.Email);

            if (exists)
                throw new Exception(
                    "Username or email already in use.");

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

            _db.Users.Add(user);

            // Default role
            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.Name == "User");

            if (role == null)
            {
                role = new Role
                {
                    Name = "User"
                };

                _db.Roles.Add(role);

                await _db.SaveChangesAsync();
            }

            _db.UserRoles.Add(new UserRole
            {
                User = user,
                Role = role
            });

            await _db.SaveChangesAsync();
        }

        public async Task<LoginResponse> LoginAsync(
            LoginRequest model)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .FirstOrDefaultAsync(u =>
                    u.Username == model.UsernameOrEmail ||
                    u.Email == model.UsernameOrEmail);

            if (user == null)
                throw new Exception("Invalid credentials.");

            var valid = PasswordHasher.VerifyPassword(
                model.Password,
                user.PasswordHash,
                user.PasswordSalt);

            if (!valid)
                throw new Exception("Invalid credentials.");

            var token = _tokenService.GenerateJwt(
                user,
                TimeSpan.FromHours(8));

            return new LoginResponse
            {
                Token = token,
                ExpiresIn = 8 * 3600
            };
        }

        public async Task<UserDto> GetCurrentUserAsync(
            int userId)
        {
            return await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .Where(u => u.Id == userId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    CreatedAt = u.CreatedAt,
                    Roles = u.UserRoles
                        .Select(ur => ur.Role.Name)
                        .ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task AssignRoleAsync(
            int userId,
            string roleName)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("User not found.");

            var role = await _db.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName);

            if (role == null)
            {
                role = new Role
                {
                    Name = roleName
                };

                _db.Roles.Add(role);

                await _db.SaveChangesAsync();
            }

            var alreadyAssigned = user.UserRoles
                .Any(ur => ur.RoleId == role.Id);

            if (!alreadyAssigned)
            {
                _db.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });

                await _db.SaveChangesAsync();
            }
        }
    }
}