using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Exceptions;
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            return Ok(await _userService.GetAllAsync());
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

            return Ok(await _userService.GetTopInCategoryAsync(categoryId, excludeUserId: null, skip, take));
        }

        [HttpGet("top-followed")]
        public async Task<ActionResult<IEnumerable<RankedUserDto>>> GetTopByFollowers(
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            return Ok(await _userService.GetTopByFollowersAsync(skip, take));
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (id == this.GetUserId())
                throw new BadRequestException("No puedes eliminar tu propia cuenta desde aquí; usa tu perfil.");

            await _userService.DeleteUserAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/admin-role")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> SetAdminRole(int id, SetAdminRoleDto dto)
        {
            if (id == this.GetUserId() && !dto.IsAdmin)
                throw new BadRequestException("No puedes quitarte el rol de admin a ti mismo.");

            return Ok(await _userService.SetAdminRoleAsync(id, dto.IsAdmin));
        }
    }
}
