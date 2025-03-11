using System.ComponentModel.DataAnnotations;

namespace workstream.Model
{
    public class Permission
    {
        [Key]
        public int PermissionId { get; set; }
        [Required]
        public string Name { get; set; }

        // Navigation property for RolePermissions
        public ICollection<RolePermission> RolePermissions { get; set; }

    }

}
