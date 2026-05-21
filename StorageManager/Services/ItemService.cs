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
    public class ItemService : IItemService
    {
        private readonly ApplicationDbContext _db;

        public ItemService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ItemDto>> GetAllAsync()
        {
            return await _db.Items
                .Include(i => i.AddedBy)
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Code = i.Code,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    RoomNumber = i.RoomNumber,
                    Section = i.Section,
                    AddedById = i.AddedById,
                    AddedByName =
                        i.AddedByName ??
                        i.AddedBy.Username,
                    CreatedAt = i.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<ItemDto> GetByIdAsync(int id)
        {
            return await _db.Items
                .Include(i => i.AddedBy)
                .Where(i => i.Id == id)
                .Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Code = i.Code,
                    Description = i.Description,
                    Quantity = i.Quantity,
                    RoomNumber = i.RoomNumber,
                    Section = i.Section,
                    AddedById = i.AddedById,
                    AddedByName =
                        i.AddedByName ??
                        i.AddedBy.Username,
                    CreatedAt = i.CreatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ItemDto> CreateAsync(
            CreateItemRequest model,
            string currentUsername)
        {
            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var codeExists = await _db.Items
                    .AnyAsync(i => i.Code == model.Code);

                if (codeExists)
                    throw new Exception(
                        "Item code already in use.");
            }

            string addedByName = model.AddedByName;
            int? addedById = model.AddedById;

            // Resolve user by id
            if (addedById.HasValue)
            {
                var user = await _db.Users
                    .FindAsync(addedById.Value);

                if (user != null)
                {
                    addedByName = user.Username;
                }
            }

            // Use current user if empty
            if (string.IsNullOrWhiteSpace(addedByName))
            {
                addedByName = currentUsername;
            }

            // Fallback
            if (string.IsNullOrWhiteSpace(addedByName))
            {
                addedByName = "Unknown";
            }

            var item = new Item
            {
                Name = model.Name,
                Code = model.Code,
                Description = model.Description,
                Quantity = model.Quantity,
                RoomNumber = model.RoomNumber,
                Section = model.Section,
                AddedById = addedById,
                AddedByName = addedByName
            };

            _db.Items.Add(item);

            await _db.SaveChangesAsync();

            return new ItemDto
            {
                Id = item.Id,
                Name = item.Name,
                Code = item.Code,
                Description = item.Description,
                Quantity = item.Quantity,
                RoomNumber = item.RoomNumber,
                Section = item.Section,
                AddedById = item.AddedById,
                AddedByName = item.AddedByName,
                CreatedAt = item.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateItemRequest model)
        {
            var item = await _db.Items
                .FindAsync(id);

            if (item == null)
                return false;

            // Check unique code
            if (!string.Equals(
                item.Code,
                model.Code,
                StringComparison.OrdinalIgnoreCase)
                &&
                !string.IsNullOrWhiteSpace(model.Code))
            {
                var codeTaken = await _db.Items
                    .AnyAsync(i =>
                        i.Code == model.Code &&
                        i.Id != id);

                if (codeTaken)
                    throw new Exception(
                        "Item code already in use.");
            }

            item.Name = model.Name;
            item.Code = model.Code;
            item.Description = model.Description;
            item.Quantity = model.Quantity;
            item.RoomNumber = model.RoomNumber;
            item.Section = model.Section;

            // Resolve AddedBy
            if (model.AddedById.HasValue)
            {
                var user = await _db.Users
                    .FindAsync(model.AddedById.Value);

                if (user != null)
                {
                    item.AddedById = user.Id;
                    item.AddedByName = user.Username;
                }
                else
                {
                    item.AddedById = null;
                    item.AddedByName =
                        model.AddedByName;
                }
            }
            else if (!string.IsNullOrWhiteSpace(
                model.AddedByName))
            {
                item.AddedByName =
                    model.AddedByName;
            }

            await _db.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _db.Items
                .FindAsync(id);

            if (item == null)
                return false;

            _db.Items.Remove(item);

            await _db.SaveChangesAsync();

            return true;
        }
    }
}