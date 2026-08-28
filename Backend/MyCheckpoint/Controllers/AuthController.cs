using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthUserResponseDto>> Register(UserAddDto dto)
        {
            var response = await _authService.RegisterAsync(dto);
            SetTokenCookie(response.Token);
            return Ok(new AuthUserResponseDto { User = response.User, ExpiresAt = response.ExpiresAt });
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthUserResponseDto>> Login(LoginDto dto)
        {
            var response = await _authService.LoginAsync(dto);
            SetTokenCookie(response.Token);
            return Ok(new AuthUserResponseDto { User = response.User, ExpiresAt = response.ExpiresAt });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete("token");
            return NoContent();
        }

        private void SetTokenCookie(string token)
        {
            Response.Cookies.Append("token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
        }
    }
}
