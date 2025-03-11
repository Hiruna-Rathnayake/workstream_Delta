using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class Stock
    {
        [Key]
        public int StockId { get; set; }

        [Required]
        public int InventoryItemId { get; set; } // Foreign key to InventoryItem
        //public InventoryItem InventoryItem { get; set; } // Navigation property

        [Required]
        public int Quantity { get; set; } // Quantity of stock item

        [Required]
        public DateTime ManufacturingDate { get; set; } // Manufacturing date of stock

        [Required]
        public DateTime ExpirationDate { get; set; } // Expiration date of stock

        [Required]
        [MaxLength(50)]
        public string BatchNumber { get; set; } // Unique batch number
    }
}
