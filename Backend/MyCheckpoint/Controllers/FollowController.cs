using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FollowController : ControllerBase
    {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService)
        {
            _followService = followService;
        }

        [HttpGet("followers/{userId}")]
        public async Task<ActionResult<IEnumerable<FollowDto>>> GetFollowers(int userId)
        {
            return Ok(await _followService.GetFollowersAsync(userId));
        }

        [HttpGet("following/{userId}")]
        public async Task<ActionResult<IEnumerable<FollowDto>>> GetFollowing(int userId)
        {
            return Ok(await _followService.GetFollowingAsync(userId));
        }

        [HttpGet("is-following/{followedId}")]
        [Authorize]
        public async Task<ActionResult<bool>> IsFollowing(int followedId)
        {
            return Ok(await _followService.IsFollowingAsync(this.GetUserId(), followedId));
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Follow(FollowAddDto dto)
        {
            await _followService.FollowAsync(dto, this.GetUserId());
            return NoContent();
        }

        [HttpDelete("{followedId}")]
        [Authorize]
        public async Task<IActionResult> Unfollow(int followedId)
        {
            await _followService.UnfollowAsync(followedId, this.GetUserId());
            return NoContent();
        }
    }
}
