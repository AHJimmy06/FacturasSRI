using System.ComponentModel.DataAnnotations;

namespace FacturasSRI.Application.Dtos
{
    public class ClienteLoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
