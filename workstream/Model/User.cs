using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class User
    {
        [Key] // Marks UserId as the primary key
        public int UserId { get; set; }

        [Required] // Username is required for identification
        public string Username { get; set; }

        [Required] // PasswordHash is required for login
        public string PasswordHash { get; set; }

        [Required]
        public int TenantId { get; set; } // Foreign key to Tenant
        //public Tenant Tenant { get; set; } // Navigation property to Tenant (commented for now due to circular reference that causes performance issues and stackoverflow)

        public int? RoleId { get; set; } // Foreign key to Role (make sure to implement if roleid = null, then it is a guest)
        public Role Role { get; set; } // Navigation property to Role
    }


}
