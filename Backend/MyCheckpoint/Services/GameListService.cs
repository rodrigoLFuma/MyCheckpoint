using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class GameListService : IGameListService
    {
        private readonly IGameListRepository _gameListRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GameListService(IGameListRepository gameListRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _gameListRepository = gameListRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<GameListDto> GetByIdAsync(int id, int? viewerUserId)
        {
            var gameList = await _gameListRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("La lista no existe.");

            if (!gameList.IsPublic && gameList.UserId != viewerUserId)
                throw new ForbiddenException("No tienes permiso para ver esta lista.");

            return _mapper.Map<GameListDto>(gameList);
        }

        public async Task<IEnumerable<GameListDto>> GetByUserIdAsync(int userId, int? viewerUserId)
        {
            var gameLists = await _gameListRepository.GetByUserIdAsync(userId, viewerUserId);
            return _mapper.Map<IEnumerable<GameListDto>>(gameLists);
        }

        public async Task<IEnumerable<GameListDto>> SearchByNameAsync(string term, int? viewerUserId)
        {
            var gameLists = await _gameListRepository.SearchByNameAsync(term, viewerUserId);
            return _mapper.Map<IEnumerable<GameListDto>>(gameLists);
        }

        public async Task<GameListDto> CreateAsync(GameListAddDto dto, int userId)
        {
            if (await _gameListRepository.ExistsByNameForUserAsync(userId, dto.Name))
                throw new ConflictException("Ya tienes una lista con ese nombre.");

            var gameList = _mapper.Map<GameList>(dto);
            gameList.UserId = userId;
            gameList.CreatedAt = DateTime.UtcNow;

            await _gameListRepository.CreateAsync(gameList);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GameListDto>(gameList);
        }

        public async Task<GameListDto> UpdateAsync(int id, GameListModifyDto dto, int userId)
        {
            var gameList = await _gameListRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("La lista no existe.");

            if (gameList.UserId != userId)
                throw new ForbiddenException("No tienes permiso para editar esta lista.");

            if (await _gameListRepository.ExistsByNameForUserAsync(userId, dto.Name, excludeListId: id))
                throw new ConflictException("Ya tienes una lista con ese nombre.");

            _mapper.Map(dto, gameList);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<GameListDto>(gameList);
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var gameList = await _gameListRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("La lista no existe.");

            if (gameList.UserId != userId)
                throw new ForbiddenException("No tienes permiso para borrar esta lista.");

            _gameListRepository.Delete(gameList);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
