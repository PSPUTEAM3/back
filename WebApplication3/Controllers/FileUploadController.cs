using DocumentFormat.OpenXml.Packaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using System.Text.RegularExpressions;
using Microsoft.Office.Interop.Word;
using MimeKit;

namespace WebApplication3.Controllers
{
    [ApiController]
    [Route("api/v1/upload")]
    public class FileUploadController : ControllerBase
    {
        private readonly TokenHelper _tokenHelper;
        private readonly ApplicationDbContext _context;
        private const int MaxSize = 16777215; // 16,777,215 байтов

        public FileUploadController(ApplicationDbContext context, TokenHelper tokenHelper)
        {
            _tokenHelper = tokenHelper; // Помощник для работы с JWT.
            _context = context;
        }
        [HttpPost("file")]
        public async Task<IActionResult> Upload(IFormFile file)//, [FromForm] string currentToken)
        {
            // Извлечение имени пользователя из токена.
            //if (_tokenHelper.IsTokenExpired(currentToken))
            //    return Unauthorized(new { Message = "Expired token" });

            //var currentTokenId = _tokenHelper.GetCurrentTokenId(currentToken);

            //// Проверка действительности токена.
            //if (_tokenHelper.IsInvalidToken(currentTokenId))
            //    return Unauthorized(new { Message = "This token has been invalidated." });

            if (file == null || file.Length == 0)
                return BadRequest("Пожалуйста, предоставьте файл для загрузки.");

            // Проверка расширения файла
            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (fileExtension != ".doc" && fileExtension != ".docx" && fileExtension != ".txt")
                return BadRequest("Файл должен быть формата .doc, .docx или .txt");

            var path = Path.Combine(Directory.GetCurrentDirectory(), "temp", file.FileName);

            using (var stream = new FileStream(path, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            if (fileExtension == ".txt")
            {
                // Прочитать содержимое файла
                var fileContent = await System.IO.File.ReadAllTextAsync(path);

                // Удалить табуляции и переносы строк
                fileContent = fileContent.Replace("\t", "").Replace("\r", "").Replace("\n", "");
                if (!await ContentWriter(fileContent, path))
                    return BadRequest($"Текст превышает допустимый размер в {MaxSize} байтов.");
            }
            if (fileExtension == ".doc")
            {
                var fileContent = ConvertDocToText(path);
                if (!await ContentWriter(fileContent, path))
                    return BadRequest($"Текст превышает допустимый размер в {MaxSize} байтов.");
            }
            if (fileExtension == ".docx")
            {
                var fileContent = ConvertDocxToText(path);
                if (!await ContentWriter(fileContent, path))
                    return BadRequest($"Текст превышает допустимый размер в {MaxSize} байтов.");
            }
            System.IO.File.Delete(path);
            return Ok($"Файл {file.FileName} успешно загружен.");
        }
        private string ConvertDocxToText(string path)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Open(path, false))
            {
                string docText = null;
                using (StreamReader sr = new StreamReader(doc.MainDocumentPart.GetStream()))
                {
                    docText = sr.ReadToEnd();
                }

                // Удаление всех XML тегов, оставляя только текст
                string plainText = Regex.Replace(docText, @"<[^>]+>", string.Empty);

                // Замена всех последовательностей пробелов, табуляций и переносов строк на одиночный пробел
                plainText = Regex.Replace(plainText, @"\s+", " ");

                return plainText;
            }
        }
        private string ConvertDocToText(string path)
        {
            Application app = new Application();
            Document doc = app.Documents.Open(path);
            string plainText = doc.Content.Text;

            // Замена всех последовательностей пробелов, табуляций и переносов строк на одиночный пробел
            plainText = Regex.Replace(plainText, @"\s+", " ");

            doc.Close();
            app.Quit();

            return plainText;
        }
        private async Task<bool> ContentWriter(string fileContent, string path)
        {
            var byteCount = Encoding.UTF8.GetByteCount(fileContent);
            if (byteCount > MaxSize)
            {
                System.IO.File.Delete(path);  // Удаление файла
                return false;
            }

            // Сохраните текст в базе данных
            var newTextEntry = new GOSTEntry // предположим, у вас есть модель TextEntry в вашем DbContext
            {
                Content = fileContent
            };
            _context.GOSTEntry.Add(newTextEntry);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
