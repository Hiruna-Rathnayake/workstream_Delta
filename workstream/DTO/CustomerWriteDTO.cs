using System.ComponentModel.DataAnnotations;

namespace workstream.DTO
{
    public class CustomerWriteDTO
    {
        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public int TenantId { get; set; }
    }
}
