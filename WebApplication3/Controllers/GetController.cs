using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace WebApplication3.Controllers
{
    // Определение маршрута и атрибута контроллера для данного класса контроллера.
    [Route("api/v1/[controller]")]
    [ApiController]
    public class GetController : Controller
    {
        // Объявляются частные поля для контекста базы данных и вспомогательного класса для работы с токенами.
        private readonly ApplicationDbContext _context;
        private readonly TokenHelper _tokenHelper;

        // Конструктор контроллера с внедрением зависимостей для контекста базы данных и вспомогательного класса токенов.
        public GetController(ApplicationDbContext context, TokenHelper tokenHelper)
        {
            _context = context;
            _tokenHelper = tokenHelper;
        }

        // Метод действия контроллера для получения имени пользователя из токена.
        [HttpPost("username-from-token")]
        public IActionResult GetUserNameFromToken([FromForm] string currentToken)
        {
            try
            {
                // Извлечение имени пользователя из токена.
                if (_tokenHelper.IsTokenExpired(currentToken))
                    return Unauthorized(new { Message = "Expired token" });

                // Получение идентификатора текущего токена.
                var currentTokenId = _tokenHelper.GetCurrentTokenId(currentToken);

                // Проверка действительности токена.
                if (_tokenHelper.IsInvalidToken(currentTokenId))
                    return Unauthorized(new { Message = "This token has been invalidated." });

                // Извлечение имени пользователя из токена.
                var username = _tokenHelper.ExtractUsernameFromToken(Request.Headers["Authorization"].ToString());

                // Если имя пользователя найдено, возвращается ответ с именем пользователя.
                if (username != null)
                    return Ok(new { UserName = username });
                // В противном случае возвращается ответ об ошибке.
                else
                    return Unauthorized(new { Message = "Invalid token" });
            }
            // Обработка исключений и возврат ошибки сервера.
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return StatusCode(500, new { Error = "An error occurred while processing your request. Please try again later." });
            }
        }
        [HttpPost("report")]
        public async Task<IActionResult> GetReportForProduct([FromForm] string product)
        {
            string question = "Какое оборудование для тестирования и испытаний?";
            var report = "Начало";
            using (var client = new HttpClient())
            {
                foreach (var entry in _context.GOSTEntry)
                {
                    // Задайте адрес вашего Flask-API
                    var apiUrl = "http://62.113.116.57:5000/api";

                    var requestData = new
                    {
                        entry = entry.Content,
                        question = question,
                        product = product
                    };

                    var jsonContent = JsonConvert.SerializeObject(requestData);
                    var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync(apiUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        // Обработайте ответ
                        report += await response.Content.ReadAsStringAsync();
                        // Далее обработайте responseBody, если это необходимо
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
                return Ok(new { Report = report });
            }
        }
    }
}
