using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using StorageManager.Models;
using StorageManager.Repositories.Interfaces;
using StorageManager.Services.Interfaces;

namespace StorageManager.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _itemRepository;
        private readonly IUserRepository _userRepository;

        public ItemService(
            IItemRepository itemRepository,
            IUserRepository userRepository)
        {
            _itemRepository = itemRepository;
            _userRepository = userRepository;
        }

        public async Task<List<ItemDto>> GetAllAsync()
        {
            var items =
                await _itemRepository.GetAllAsync();

            return items.Select(MapToDto).ToList();
        }

        public async Task<ItemDto> GetByIdAsync(int id)
        {
            var item =
                await _itemRepository.GetByIdAsync(id);

            return item == null
                ? null
                : MapToDto(item);
        }

        public async Task<ItemDto> CreateAsync(
            CreateItemRequest model,
            string currentUsername)
        {
            if (!string.IsNullOrWhiteSpace(model.Code))
            {
                var exists =
                    await _itemRepository
                        .CodeExistsAsync(model.Code);

                if (exists)
                {
                    throw new Exception(
                        "Item code already exists.");
                }
            }

            string addedByName = currentUsername;

            if (model.AddedById.HasValue)
            {
                var user =
                    await _userRepository
                        .FindAsync(
                            model.AddedById.Value);

                if (user != null)
                {
                    addedByName = user.Username;
                }
            }

            if (string.IsNullOrWhiteSpace(
                addedByName))
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
                AddedById = model.AddedById,
                AddedByName = addedByName
            };

            await _itemRepository.AddAsync(item);

            await _itemRepository.SaveChangesAsync();

            return MapToDto(item);
        }

        public async Task<bool> UpdateAsync(
            int id,
            UpdateItemRequest model)
        {
            var item =
                await _itemRepository.GetByIdAsync(id);

            if (item == null)
                return false;

            if (!string.Equals(
                item.Code,
                model.Code,
                StringComparison.OrdinalIgnoreCase))
            {
                var exists =
                    await _itemRepository
                        .CodeExistsAsync(
                            model.Code,
                            id);

                if (exists)
                {
                    throw new Exception(
                        "Item code already exists.");
                }
            }

            item.Name = model.Name;
            item.Code = model.Code;
            item.Description = model.Description;
            item.Quantity = model.Quantity;
            item.RoomNumber = model.RoomNumber;
            item.Section = model.Section;

            if (model.AddedById.HasValue)
            {
                var user =
                    await _userRepository
                        .FindAsync(
                            model.AddedById.Value);

                if (user != null)
                {
                    item.AddedById = user.Id;
                    item.AddedByName = user.Username;
                }
            }

            await _itemRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item =
                await _itemRepository.GetByIdAsync(id);

            if (item == null)
                return false;

            _itemRepository.Remove(item);

            await _itemRepository.SaveChangesAsync();

            return true;
        }

        private ItemDto MapToDto(Item item)
        {
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
    }
}