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
            Response.Cookies.Delete("token", CookieOptions());
            return NoContent();
        }

        private void SetTokenCookie(string token)
        {
            Response.Cookies.Append("token", token, CookieOptions());
        }

        private static CookieOptions CookieOptions() => new()
        {
            HttpOnly = true,
            Secure = true,
            // El frontend (http://localhost:4200) y esta API (https://localhost:7018)
            // tienen esquemas distintos, así que el navegador los trata como sitios
            // distintos ("schemeful same-site"). Strict/Lax bloquearían la cookie en
            // cualquier petición cross-origin; None es lo correcto aquí, no un parche
            // — la protección real la da la política de CORS (origen exacto + credenciales).
            SameSite = SameSiteMode.None
        };
    }
}
