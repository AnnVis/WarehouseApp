using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StorageManager.Models
{
    public class Item
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(100), Index(IsUnique = true)]
        public string Code { get; set; } // SKU / article code

        [MaxLength(1000)]
        public string Description { get; set; }

        public int Quantity { get; set; }

        [MaxLength(50)]
        public string RoomNumber { get; set; } // номер кабинета

        [MaxLength(100)]
        public string Section { get; set; } // название секции в кабинете-складе

        [MaxLength(100)]
        public string AddedByName { get; set; } // имя человека, внесшего артикул

        public int? AddedById { get; set; } // опциональная ссылка на User

        [ForeignKey("AddedById")]
        public virtual User AddedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}