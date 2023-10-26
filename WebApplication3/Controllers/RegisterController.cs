using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailSender;

        public RegisterController(ApplicationDbContext context, IEmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest request)
        {
            // Проверка, существует ли уже пользователь с таким именем или почтой
            var existingUser = await _context.users
                .SingleOrDefaultAsync(u => u.Username == request.Username || u.Email == request.Email);

            if (existingUser != null)
            {
                if (existingUser.Email == request.Email)
                    return BadRequest(new { Error = "Email is already taken" });
                return BadRequest(new { Error = "Username is already taken" });
            }

            // Хеширование пароля
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            string emailConfirmationCode = Guid.NewGuid().ToString();

            // Создание нового пользователя
            var newUser = new ApplicationUser
            {
                Username = request.Username,
                Email = request.Email, // Если вы используете email
                PasswordHash = hashedPassword,
                EmailConfirmed = false,
                EmailConfirmationCode = emailConfirmationCode
            };
            _context.users.Add(newUser);
            await _context.SaveChangesAsync();

            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/Register/confirm-email?code={emailConfirmationCode}";
            if (request.Email != null && confirmationLink != null)
                await _emailSender.EmailConfirmationMessage(request.Email!, confirmationLink!);

            return Ok(new { Message = "Registration successful" });
        }
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string code)
        {
            var user = await _context.users.FirstOrDefaultAsync(u => u.EmailConfirmationCode == code);
            if (user == null)
            {
                return NotFound("Confirmation code is invalid.");
            }

            user.EmailConfirmed = true;
            user.EmailConfirmationCode = null; // очистите код, он больше не нужен

            _context.users.Update(user);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Email confirmed successfully" });
        }
    }
}
