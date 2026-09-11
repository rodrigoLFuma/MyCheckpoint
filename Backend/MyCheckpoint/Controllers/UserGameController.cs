using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserGameController : ControllerBase
    {
        private readonly IUserGameService _userGameService;

        public UserGameController(IUserGameService userGameService)
        {
            _userGameService = userGameService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserGameDto>> GetById(int id)
        {
            return Ok(await _userGameService.GetByIdAsync(id, this.GetUserIdOrNull()));
        }

        [HttpGet("top-liked")]
        public async Task<ActionResult<IEnumerable<UserGameDto>>> GetTopLiked(
            [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            return Ok(await _userGameService.GetTopLikedAsync(skip, take, this.GetUserIdOrNull()));
        }

        [HttpGet("following-activity")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<UserGameDto>>> FollowingActivity(
            [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            return Ok(await _userGameService.GetFollowingActivityAsync(this.GetUserId(), skip, take));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserGameDto>>> GetByUser(int userId)
        {
            return Ok(await _userGameService.GetByUserIdAsync(userId, this.GetUserIdOrNull()));
        }

        [HttpPost("{id}/like")]
        [Authorize]
        public async Task<ActionResult<UserGameDto>> Like(int id)
        {
            return Ok(await _userGameService.LikeAsync(id, this.GetUserId()));
        }

        [HttpDelete("{id}/like")]
        [Authorize]
        public async Task<ActionResult<UserGameDto>> Unlike(int id)
        {
            return Ok(await _userGameService.UnlikeAsync(id, this.GetUserId()));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<UserGameDto>> Create(UserGameAddDto dto)
        {
            var created = await _userGameService.CreateAsync(dto, this.GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<UserGameDto>> Update(int id, UserGameModifyDto dto)
        {
            return Ok(await _userGameService.UpdateAsync(id, dto, this.GetUserId()));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _userGameService.DeleteAsync(id, this.GetUserId());
            return NoContent();
        }
    }
}
