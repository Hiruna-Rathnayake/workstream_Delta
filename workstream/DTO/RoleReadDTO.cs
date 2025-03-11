namespace workstream.DTO
{
    public class RoleReadDTO
    {
        public int RoleId { get; set; }
        public string Name { get; set; }
        public int TenantId { get; set; }
        public List<string> PermissionNames { get; set; } // List of permission names
    }

}
