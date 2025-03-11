using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class Tenant
    {
        [Key] // Marks TenantId as the primary key
        public int TenantId { get; set; }

        [Required] // Ensures the CompanyName is not null
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress] // Validates that the contact email follows the proper format
        public string ContactEmail { get; set; }

        public ICollection<User> Users { get; set; } // Navigation property to Users
        public ICollection<Role> Roles { get; set; }

    }


}
