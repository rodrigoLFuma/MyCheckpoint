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
            return Ok(await _userGameService.GetByIdAsync(id));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserGameDto>>> GetByUser(int userId)
        {
            return Ok(await _userGameService.GetByUserIdAsync(userId));
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
