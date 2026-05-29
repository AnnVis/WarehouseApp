using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorageManager.Models;
using StorageManager.Repositories.Interfaces;
using StorageManager.Services.Interfaces;

namespace StorageManager.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(
            IUserRepository userRepository,
            IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users =
                await _userRepository.GetAllAsync();

            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto> GetByIdAsync(int id)
        {
            var user =
                await _userRepository.GetByIdAsync(id);

            return user == null
                ? null
                : MapToDto(user);
        }

        public async Task<UserDto> CreateAsync(
            CreateUserRequest model)
        {
            var exists =
                await _userRepository.ExistsAsync(
                    model.Username,
                    model.Email);

            if (exists)
            {
                throw new Exception(
                    "Username or email already in use.");
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

            if (model.Roles != null)
            {
                foreach (var roleName in model.Roles.Distinct())
                {
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

                    user.UserRoles.Add(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                }

                await _userRepository.SaveChangesAsync();
            }

            return MapToDto(user);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateUserRequest model)
        {
            var user =
                await _userRepository.GetByIdAsync(id);

            if (user == null)
                return false;

            var usernameTaken =
                await _userRepository
                    .UsernameExistsAsync(
                        model.Username,
                        id);

            if (usernameTaken)
            {
                throw new Exception(
                    "Username already in use.");
            }

            var emailTaken =
                await _userRepository
                    .EmailExistsAsync(
                        model.Email,
                        id);

            if (emailTaken)
            {
                throw new Exception(
                    "Email already in use.");
            }

            user.Username = model.Username;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Address = model.Address;

            if (!string.IsNullOrWhiteSpace(
                model.Password))
            {
                PasswordHasher.CreatePasswordHash(
                    model.Password,
                    out var hash,
                    out var salt);

                user.PasswordHash = hash;
                user.PasswordSalt = salt;
            }

            user.UserRoles.Clear();

            if (model.Roles != null)
            {
                foreach (var roleName in model.Roles)
                {
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

                    user.UserRoles.Add(
                        new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        });
                }
            }

            await _userRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user =
                await _userRepository.FindAsync(id);

            if (user == null)
                return false;

            _userRepository.Remove(user);

            await _userRepository.SaveChangesAsync();

            return true;
        }

        private UserDto MapToDto(User user)
        {
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
    }
}