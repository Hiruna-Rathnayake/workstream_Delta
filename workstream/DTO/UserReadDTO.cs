namespace workstream.DTO
{
    public class UserReadDTO
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public RoleReadDTO Role { get; set; }  // Role with permissions
    }

}
