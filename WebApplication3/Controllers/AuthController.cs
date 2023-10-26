using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly TokenHelper _tokenHelper;
        public AuthController(ApplicationDbContext context, TokenHelper tokenHelper)
        {
            _context = context;
            _tokenHelper = tokenHelper;
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var user = _context.users.SingleOrDefault(u => u.Username == request.Username);

                if (
                user == null || !VerifyPassword(request.Password, user.PasswordHash!))
                {
                    return Unauthorized();
                }

                if (!user.EmailConfirmed)
                {
                    return BadRequest(new { Error = "Please confirm your email address first" });
                }

                var token = _tokenHelper.GenerateToken(user.Username);
                var refreshToken = Guid.NewGuid().ToString();

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(60);
                _context.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Username = user.Username
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while processing your request. Please try again later." });
            }
        }
        
        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            try
            {
                string currentToken = Request.HttpContext.Request.Headers["ToInvalidToken"].ToString();
                if (!string.IsNullOrEmpty(currentToken) || !_tokenHelper.IsTokenExpired(currentToken))
                {
                    _tokenHelper.ToInvalidToken(_tokenHelper.GetCurrentTokenId(currentToken), _tokenHelper.GetTokenExpiryDate(currentToken));
                }
                var user = await _context.users.FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

                if (user == null || user.RefreshTokenExpiryDate < DateTime.UtcNow)
                {
                    return Unauthorized(new { Message = "Invalid or expired refresh token" });
                }
                
                var newToken = _tokenHelper.GenerateToken(user.Username);

                // Генерация нового рефреш-токена
                var newRefreshToken = Guid.NewGuid().ToString();
                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiryDate = DateTime.UtcNow.AddDays(60);
                _context.Update(user);
                _context.SaveChanges();

                return Ok(new
                {
                    Token = newToken,
                    RefreshToken = newRefreshToken
                });
            }
            catch(Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while processing your request. Please try again later." });
            }
        }
        [HttpPost("invalidate-token")]
        public IActionResult InvalidateCurrentToken()
        {
            try
            {
                string currentToken = Request.HttpContext.Request.Headers["Authorization"].ToString();
                if (_tokenHelper.IsTokenExpired(currentToken))
                    return Unauthorized(new { Message = "Expired token" });
                var currentTokenId = _tokenHelper.GetCurrentTokenId(currentToken);
                if (currentTokenId == null)
                    return BadRequest(new { Error = "Token ID not found" });
                var currentTokenExpiryDate = DateTime.UtcNow.AddMinutes(5); // Задайте срок действия вашего токена
                _tokenHelper.ToInvalidToken(currentTokenId, _tokenHelper.GetTokenExpiryDate(currentToken));
                return Ok(new { Message = "Token has been invalidated" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = "An error occurred while processing your request. Please try again later." });
            }
        }
    }
}
