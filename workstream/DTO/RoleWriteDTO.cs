using System.ComponentModel.DataAnnotations;

namespace workstream.DTO
{
    public class RoleWriteDTO
    {
        public string Name { get; set; }
        public int TenantId { get; set; }
        public List<string> PermissionNames { get; set; } // List of permission names to assign to the role
    }

}
