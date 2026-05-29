using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using StorageManager.Data;
using StorageManager.Models;
using StorageManager.Repositories.Interfaces;

namespace StorageManager.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _db;

        public UserRepository(
            ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .ToListAsync();
        }

        public async Task<User> GetByIdAsync(int id)
        {
            return await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User> GetByUsernameOrEmailAsync(
            string value)
        {
            return await _db.Users
                .Include(u => u.UserRoles.Select(ur => ur.Role))
                .FirstOrDefaultAsync(u =>
                    u.Username == value ||
                    u.Email == value);
        }

        public async Task<User> FindAsync(int id)
        {
            return await _db.Users.FindAsync(id);
        }

        public async Task<bool> ExistsAsync(
            string username,
            string email)
        {
            return await _db.Users.AnyAsync(u =>
                u.Username == username ||
                u.Email == email);
        }

        public async Task<bool> UsernameExistsAsync(
            string username,
            int excludeId)
        {
            return await _db.Users.AnyAsync(u =>
                u.Username == username &&
                u.Id != excludeId);
        }

        public async Task<bool> EmailExistsAsync(
            string email,
            int excludeId)
        {
            return await _db.Users.AnyAsync(u =>
                u.Email == email &&
                u.Id != excludeId);
        }

        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);

            await Task.CompletedTask;
        }

        public void Remove(User user)
        {
            _db.Users.Remove(user);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}