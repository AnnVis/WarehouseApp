using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using StorageManager.Data;
using StorageManager.Models;
using StorageManager.Services.Interfaces;

namespace StorageManager.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;

        public UserService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            return await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    CreatedAt = u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .ToListAsync();
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            return await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .Where(u => u.Id == id)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Phone = u.Phone,
                    Address = u.Address,
                    CreatedAt = u.CreatedAt,
                    Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<UserDto> CreateAsync(CreateUserRequest model)
        {
            var exists = await _db.Users.AnyAsync(u =>
                u.Username == model.Username ||
                u.Email == model.Email);

            if (exists)
                throw new Exception("Username or email already in use.");

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

            await _db.SaveChangesAsync();

            // Assign roles
            if (model.Roles != null && model.Roles.Any())
            {
                foreach (var roleName in model.Roles.Distinct())
                {
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

                    _db.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                }

                await _db.SaveChangesAsync();
            }

            return await GetByIdAsync(user.Id);
        }

        public async Task<bool> UpdateAsync(int id, UpdateUserRequest model)
        {
            var user = await _db.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return false;

            // Username uniqueness
            if (!string.Equals(
                user.Username,
                model.Username,
                StringComparison.OrdinalIgnoreCase))
            {
                var usernameTaken = await _db.Users.AnyAsync(u =>
                    u.Username == model.Username &&
                    u.Id != id);

                if (usernameTaken)
                    throw new Exception("Username already in use.");

                user.Username = model.Username;
            }

            // Email uniqueness
            if (!string.Equals(
                user.Email,
                model.Email,
                StringComparison.OrdinalIgnoreCase))
            {
                var emailTaken = await _db.Users.AnyAsync(u =>
                    u.Email == model.Email &&
                    u.Id != id);

                if (emailTaken)
                    throw new Exception("Email already in use.");

                user.Email = model.Email;
            }

            user.Phone = model.Phone;
            user.Address = model.Address;

            // Update password
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                PasswordHasher.CreatePasswordHash(
                    model.Password,
                    out var hash,
                    out var salt);

                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            // Replace roles
            if (model.Roles != null)
            {
                var existingRoles = _db.UserRoles
                    .Where(ur => ur.UserId == user.Id);

                _db.UserRoles.RemoveRange(existingRoles);

                foreach (var roleName in model.Roles.Distinct())
                {
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

                    _db.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                }
            }

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _db.Users.FindAsync(id);

            if (user == null)
                return false;

            var userRoles = _db.UserRoles
                .Where(ur => ur.UserId == id);

            _db.UserRoles.RemoveRange(userRoles);

            _db.Users.Remove(user);

            await _db.SaveChangesAsync();

            return true;
        }
    }
}