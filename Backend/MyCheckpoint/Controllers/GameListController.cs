using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameListController : ControllerBase
    {
        private readonly IGameListService _gameListService;

        public GameListController(IGameListService gameListService)
        {
            _gameListService = gameListService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GameListDto>> GetById(int id)
        {
            return Ok(await _gameListService.GetByIdAsync(id, this.GetUserIdOrNull()));
        }

        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<GameListDto>>> GetByUser(int userId)
        {
            return Ok(await _gameListService.GetByUserIdAsync(userId, this.GetUserIdOrNull()));
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<GameListDto>>> Search([FromQuery] string term)
        {
            return Ok(await _gameListService.SearchByNameAsync(term, this.GetUserIdOrNull()));
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<GameListDto>> Create(GameListAddDto dto)
        {
            var created = await _gameListService.CreateAsync(dto, this.GetUserId());
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<GameListDto>> Update(int id, GameListModifyDto dto)
        {
            return Ok(await _gameListService.UpdateAsync(id, dto, this.GetUserId()));
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _gameListService.DeleteAsync(id, this.GetUserId());
            return NoContent();
        }

        [HttpPost("{id}/items")]
        [Authorize]
        public async Task<IActionResult> AddItem(int id, GameListItemAddDto dto)
        {
            await _gameListService.AddItemAsync(id, dto, this.GetUserId());
            return NoContent();
        }

        [HttpDelete("{id}/items/{gameId}")]
        [Authorize]
        public async Task<IActionResult> RemoveItem(int id, int gameId)
        {
            await _gameListService.RemoveItemAsync(id, gameId, this.GetUserId());
            return NoContent();
        }

        [HttpPost("pending-items")]
        [Authorize]
        public async Task<IActionResult> AddPendingItem(GameListItemAddDto dto)
        {
            await _gameListService.AddToPendingListAsync(dto.GameId, this.GetUserId());
            return NoContent();
        }
    }
}
