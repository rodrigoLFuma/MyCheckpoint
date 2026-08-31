using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;
using MyCheckpoint.Services.External;

namespace MyCheckpoint.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GameController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GameController(IGameService gameService)
        {
            _gameService = gameService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetAll()
        {
            return Ok(await _gameService.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GameDto>> GetById(int id)
        {
            return Ok(await _gameService.GetByIdAsync(id));
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<GameDto>>> Search([FromQuery] string term)
        {
            return Ok(await _gameService.SearchByNameAsync(term));
        }

        [HttpGet("category")]
        public async Task<ActionResult<IEnumerable<GameDto>>> GetByCategory([FromQuery] IEnumerable<int> categoryIds)
        {
            return Ok(await _gameService.GetByCategoryAsync(categoryIds));
        }

        [HttpGet("search-external")]
        public async Task<ActionResult<GameSearchResultDto>> SearchExternal(
            [FromQuery] string? term, [FromQuery] string? genres, [FromQuery] int page = 1)
        {
            return Ok(await _gameService.SearchExternalAsync(term, genres, page));
        }

        [HttpPost("import/{rawgId}")]
        [Authorize]
        public async Task<ActionResult<GameDto>> Import(int rawgId)
        {
            var created = await _gameService.ImportFromRawgAsync(rawgId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GameDto>> Create(GameAddDto dto)
        {
            var created = await _gameService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GameDto>> Update(int id, GameModifyDto dto)
        {
            return Ok(await _gameService.UpdateAsync(id, dto));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _gameService.DeleteAsync(id);
            return NoContent();
        }
    }
}
