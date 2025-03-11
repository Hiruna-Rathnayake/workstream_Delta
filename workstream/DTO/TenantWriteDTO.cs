using System.ComponentModel.DataAnnotations;

namespace workstream.DTO
{
    public class TenantWriteDTO
    {
        [Required]
        public string CompanyName { get; set; }

        [Required]
        [EmailAddress]
        public string ContactEmail { get; set; }
    }

}
