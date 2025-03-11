using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class Customer
    {
        [Key]
        public int CustomerId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        // TenantId to ensure customer belongs to a specific tenant
        [Required]
        public int TenantId { get; set; }

        // Flag to mark if the customer is deleted (soft delete)
        public bool IsDeleted { get; set; }

        // Navigation property for Orders
        public ICollection<Order> Orders { get; set; }
    }
}
