using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3
{
    public class RegistrationRequest
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        [EmailAddress] // Если вы хотите использовать email как имя пользователя
        public string? Email { get; set; } // Опционально

        [Required]
        public string? Password { get; set; }
    }

}
