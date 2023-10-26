using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GetController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenHelper _tokenHelper;
        public GetController(ApplicationDbContext context, TokenHelper tokenHelper)
        {
            _context = context;
            _tokenHelper = tokenHelper;
        }
        [HttpPost("username-from-token")]
        public IActionResult GetUserNameFromToken()
        {
            try
            {
                string currentToken = Request.HttpContext.Request.Headers["Authorization"].ToString();
                if (_tokenHelper.IsTokenExpired(currentToken))
                    return Unauthorized(new { Message = "Expired token" });
                var currentTokenId = _tokenHelper.GetCurrentTokenId(currentToken);

                if (_tokenHelper.IsInvalidToken(currentTokenId))
                    return Unauthorized(new { Message = "This token has been invalidated." });
                var username = _tokenHelper.ExtractUsernameFromToken(Request.Headers["Authorization"].ToString());

                if (username != null)
                    return Ok(new { UserName = username });
                else
                    return Unauthorized(new { Message = "Invalid token" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while processing your request. Please try again later." });
            }
        }
    }
}
