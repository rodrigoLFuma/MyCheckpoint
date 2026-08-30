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
        private const string PendingListName = "Juegos pendientes";

        private readonly IGameListRepository _gameListRepository;
        private readonly IGameListItemRepository _gameListItemRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GameListService(
            IGameListRepository gameListRepository,
            IGameListItemRepository gameListItemRepository,
            IGameRepository gameRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _gameListRepository = gameListRepository;
            _gameListItemRepository = gameListItemRepository;
            _gameRepository = gameRepository;
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

            var created = await _gameListRepository.GetByIdAsync(gameList.Id);
            return _mapper.Map<GameListDto>(created);
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

        public async Task AddItemAsync(int gameListId, GameListItemAddDto dto, int userId)
        {
            var gameList = await _gameListRepository.GetByIdAsync(gameListId)
                ?? throw new NotFoundException("La lista no existe.");

            if (gameList.UserId != userId)
                throw new ForbiddenException("No tienes permiso para modificar esta lista.");

            if (gameList.GameListItems.Any(gli => gli.GameId == dto.GameId))
                throw new ConflictException("Ese juego ya está en la lista.");

            if (await _gameRepository.GetByIdAsync(dto.GameId) is null)
                throw new NotFoundException("El juego no existe.");

            var item = _mapper.Map<GameListItem>(dto);
            item.GameListId = gameListId;
            item.AddedAt = DateTime.UtcNow;

            await _gameListItemRepository.CreateAsync(item);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(int gameListId, int gameId, int userId)
        {
            var gameList = await _gameListRepository.GetByIdAsync(gameListId)
                ?? throw new NotFoundException("La lista no existe.");

            if (gameList.UserId != userId)
                throw new ForbiddenException("No tienes permiso para modificar esta lista.");

            var item = gameList.GameListItems.FirstOrDefault(gli => gli.GameId == gameId)
                ?? throw new NotFoundException("Ese juego no está en la lista.");

            _gameListItemRepository.Delete(item);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AddToPendingListAsync(int gameId, int userId)
        {
            var pendingList = await GetOrCreatePendingListAsync(userId);
            await AddItemAsync(pendingList.Id, new GameListItemAddDto { GameId = gameId }, userId);
        }

        public async Task EnsurePendingListAsync(int userId)
        {
            await GetOrCreatePendingListAsync(userId);
        }

        public async Task RemoveFromPendingListIfPresentAsync(int gameId, int userId)
        {
            var lists = await _gameListRepository.GetByUserIdAsync(userId, userId);
            var pendingList = lists.FirstOrDefault(gl => gl.Name == PendingListName);
            if (pendingList is null)
                return;

            var alreadyThere = await _gameListItemRepository.ExistsAsync(pendingList.Id, gameId);
            if (!alreadyThere)
                return;

            await RemoveItemAsync(pendingList.Id, gameId, userId);
        }

        private async Task<GameList> GetOrCreatePendingListAsync(int userId)
        {
            var lists = await _gameListRepository.GetByUserIdAsync(userId, userId);
            var existing = lists.FirstOrDefault(gl => gl.Name == PendingListName);
            if (existing is not null)
                return existing;

            var pendingList = new GameList
            {
                Name = PendingListName,
                IsPublic = false,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _gameListRepository.CreateAsync(pendingList);
            await _unitOfWork.SaveChangesAsync();

            return pendingList;
        }
    }
}
