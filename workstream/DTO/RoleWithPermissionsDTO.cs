namespace workstream.DTO
{
    public class RoleWithPermissionsDTO
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public List<string> PermissionNames { get; set; } // List of permission names
    }

}
