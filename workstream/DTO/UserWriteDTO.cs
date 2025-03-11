using System.ComponentModel.DataAnnotations;

namespace workstream.DTO
{
    public class UserWriteDTO
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public int? TenantId { get; set; }

        public int? RoleId { get; set; }  // Optional RoleId
    }


}
