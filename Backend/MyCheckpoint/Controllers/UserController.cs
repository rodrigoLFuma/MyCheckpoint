using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserSummaryDto>> GetById(int id)
        {
            return Ok(await _userService.GetByIdAsync(id));
        }

        [HttpGet("{id}/stats")]
        public async Task<ActionResult<UserStatsDto>> GetStats(int id)
        {
            return Ok(await _userService.GetStatsAsync(id));
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> Search(
            [FromQuery] string? term, [FromQuery] int? categoryId)
        {
            return Ok(await _userService.SearchAsync(term, categoryId));
        }

        [HttpGet("top-by-favorites")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<CategoryTopUsersDto>>> GetTopByFavorites([FromQuery] int perCategory = 5)
        {
            return Ok(await _userService.GetTopByFavoriteGenresAsync(this.GetUserId(), perCategory));
        }

        [HttpGet("category/{categoryId}/top")]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetTopInCategory(
            int categoryId, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            // Ranking completo de la categoría: no se excluye a nadie, ni
            // siquiera a quien lo consulta (a diferencia de top-by-favorites,
            // que sí te excluye porque ahí no tiene sentido recomendarte a ti).
            return Ok(await _userService.GetTopInCategoryAsync(categoryId, excludeUserId: null, skip, take));
        }

        [HttpGet("similar")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserSummaryDto>>> GetSimilar(
            [FromQuery] int skip = 0, [FromQuery] int take = 6)
        {
            return Ok(await _userService.GetSimilarUsersAsync(this.GetUserId(), skip, take));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetMyProfile()
        {
            return Ok(await _userService.GetMyProfileAsync(this.GetUserId()));
        }

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateProfile(UserModifyDto dto)
        {
            return Ok(await _userService.UpdateProfileAsync(this.GetUserId(), dto));
        }

        [HttpPost("me/profile-picture")]
        [Authorize]
        [RequestSizeLimit(5 * 1024 * 1024)]
        public async Task<ActionResult<UserDto>> UploadProfilePicture(IFormFile file)
        {
            return Ok(await _userService.UpdateProfilePictureAsync(this.GetUserId(), file));
        }

        [HttpDelete("me/profile-picture")]
        [Authorize]
        public async Task<ActionResult<UserDto>> DeleteProfilePicture()
        {
            return Ok(await _userService.RemoveProfilePictureAsync(this.GetUserId()));
        }

        [HttpPut("me/password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            await _userService.ChangePasswordAsync(this.GetUserId(), dto);
            return NoContent();
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            await _userService.DeleteMyAccountAsync(this.GetUserId());
            return NoContent();
        }
    }
}
