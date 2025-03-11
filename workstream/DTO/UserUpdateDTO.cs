namespace workstream.DTO
{
    public class UserUpdateDTO
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }  // Optional
        public int? RoleId { get; set; }  // Allow role changes, but be cautious with permission management
    }
}

