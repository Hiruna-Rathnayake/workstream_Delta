using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string Name { get; set; } // Role name (e.g., Admin, Manager)
        [Required]
        public int TenantId { get; set; }

        public Tenant Tenant { get; set; } // Navigation property for Tenant
        public ICollection<User> Users { get; set; } // Navigation property to Users
        public ICollection<RolePermission> RolePermissions { get; set; } // Navigation property to RolePermissions
    }


}
