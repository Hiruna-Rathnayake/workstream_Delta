//MIDDLE TABLE FOR MANY TO MANY RELATIONSHIP BETWEEN ROLE AND PERMISSION
using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class RolePermission
    {
        [Key] // Composite primary key
        public int RoleId { get; set; }
        public Role Role { get; set; }

        [Required] // Ensures TenantId is always provided
        public int TenantId { get; set; }

        [Key] // Composite primary key
        public int PermissionId { get; set; }
        public Permission Permission { get; set; }
    }

}
