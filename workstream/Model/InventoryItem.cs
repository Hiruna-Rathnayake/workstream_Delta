using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class InventoryItem
    {
        [Key]
        public int InventoryItemId { get; set; }

        [Required]
        public string Name { get; set; } // Name of the inventory item

        public string Description { get; set; } // Optional description of the item

        public decimal Price { get; set; } // Price of the item

        public ICollection<Stock> Stocks { get; set; } // Navigation property to Stocks

        public ICollection<OrderItem> OrderItems { get; set; } // Navigation property to OrderItems

        // Soft delete flag
        public bool IsDeleted { get; set; } // Mark as deleted, without actually deleting it from the database

        [Required]
        public int TenantId { get; set; } // Foreign key to Tenant, ensuring inventory is tenant-specific
    }
}
