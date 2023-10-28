using System.ComponentModel.DataAnnotations;

namespace WebApplication3
{
    public class AuthRequest
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
