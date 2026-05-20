using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace StorageManager.Models
{
    public class ItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public string RoomNumber { get; set; }
        public string Section { get; set; }
        public string AddedByName { get; set; }
        public int? AddedById { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateItemRequest
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Code { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; } = 1;

        [MaxLength(50)]
        public string RoomNumber { get; set; }

        [MaxLength(100)]
        public string Section { get; set; }

        // Either provide AddedById (existing user) or AddedByName; if both missing,
        // controller will use current authenticated user's name when available.
        public int? AddedById { get; set; }

        [MaxLength(100)]
        public string AddedByName { get; set; }
    }

    public class UpdateItemRequest
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string Code { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [MaxLength(50)]
        public string RoomNumber { get; set; }

        [MaxLength(100)]
        public string Section { get; set; }

        public int? AddedById { get; set; }

        [MaxLength(100)]
        public string AddedByName { get; set; }
    }
}