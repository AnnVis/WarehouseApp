using System.Data.Entity;
using System.Threading.Tasks;
using StorageManager.Data;
using StorageManager.Models;
using StorageManager.Repositories.Interfaces;

namespace StorageManager.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _db;

        public RoleRepository(
            ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Role> GetByNameAsync(
            string roleName)
        {
            return await _db.Roles
                .FirstOrDefaultAsync(r =>
                    r.Name == roleName);
        }

        public async Task AddAsync(Role role)
        {
            _db.Roles.Add(role);

            await Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}