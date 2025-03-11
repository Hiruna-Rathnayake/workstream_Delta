using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class Order
    {
        // Primary key for the Order table
        public int OrderId { get; set; }

        // Foreign key to the Customer who placed the order
        [Required]
        public int CustomerId { get; set; }

        // Navigation property for the related Customer entity
        public Customer Customer { get; set; }

        // Foreign key for multi-tenancy
        [Required]
        public int TenantId { get; set; }

        // Date when the order was placed
        [Required]
        public DateTime OrderDate { get; set; }

        // Status of the order (e.g., Pending, Completed, Shipped, etc.)
        [Required]
        [MaxLength(50)]
        public string Status { get; set; }

        // Navigation property for the related OrderItems
        // An Order can have multiple OrderItems
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
