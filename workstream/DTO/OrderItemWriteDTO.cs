using System.ComponentModel.DataAnnotations;

namespace workstream.DTO
{
    public class OrderItemWriteDTO
    {
        [Required]
        public int InventoryItemId { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

    }
}
