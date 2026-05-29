using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using StorageManager.Data;
using StorageManager.Models;
using StorageManager.Repositories.Interfaces;

namespace StorageManager.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly ApplicationDbContext _db;

        public ItemRepository(
            ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<Item>> GetAllAsync()
        {
            return await _db.Items
                .Include(i => i.AddedBy)
                .ToListAsync();
        }

        public async Task<Item> GetByIdAsync(int id)
        {
            return await _db.Items
                .Include(i => i.AddedBy)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<bool> CodeExistsAsync(
            string code,
            int excludeId = 0)
        {
            return await _db.Items.AnyAsync(i =>
                i.Code == code &&
                i.Id != excludeId);
        }

        public async Task AddAsync(Item item)
        {
            _db.Items.Add(item);

            await Task.CompletedTask;
        }

        public void Remove(Item item)
        {
            _db.Items.Remove(item);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}