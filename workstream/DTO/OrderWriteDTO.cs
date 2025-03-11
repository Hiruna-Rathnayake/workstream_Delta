using System.ComponentModel.DataAnnotations;

namespace workstream.DTO
{
    public class OrderWriteDTO
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int TenantId { get; set; } // Multi-tenancy support

        [Required]
        public DateTime OrderDate { get; set; }

        public string Status { get; set; } // Status of the order (optional to update)

        public List<OrderItemWriteDTO> ItemsToAdd { get; set; } = new List<OrderItemWriteDTO>(); // Items being added to the order

        public List<int> ItemsToRemove { get; set; } = new List<int>(); // IDs of items being removed from the order
    }
}
