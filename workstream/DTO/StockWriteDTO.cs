using System;
using System.ComponentModel.DataAnnotations;

namespace workstream.DTO
{
    public class StockWriteDTO
    {
        [Required]
        public int InventoryItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public DateTime ManufacturingDate { get; set; }

        [Required]
        public DateTime ExpirationDate { get; set; }

        [Required]
        [MaxLength(50)]
        public string BatchNumber { get; set; }
    }
}

