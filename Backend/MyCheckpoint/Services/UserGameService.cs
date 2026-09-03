using AutoMapper;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class UserGameService : IUserGameService
    {
        private readonly IUserGameRepository _userGameRepository;
        private readonly IGameRepository _gameRepository;
        private readonly IGameListService _gameListService;
        private readonly IFollowRepository _followRepository;
        private readonly IReviewLikeRepository _reviewLikeRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserGameService(
            IUserGameRepository userGameRepository,
            IGameRepository gameRepository,
            IGameListService gameListService,
            IFollowRepository followRepository,
            IReviewLikeRepository reviewLikeRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userGameRepository = userGameRepository;
            _gameRepository = gameRepository;
            _gameListService = gameListService;
            _followRepository = followRepository;
            _reviewLikeRepository = reviewLikeRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserGameDto> GetByIdAsync(int id, int? viewerUserId)
        {
            var userGame = await _userGameRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Esta entrada no existe.");

            var dto = _mapper.Map<UserGameDto>(userGame);
            return (await EnrichWithLikesAsync(new[] { dto }, viewerUserId)).Single();
        }

        public async Task<IEnumerable<UserGameDto>> GetByUserIdAsync(int userId, int? viewerUserId)
        {
            var userGames = await _userGameRepository.GetByUserIdAsync(userId);
            var dtos = _mapper.Map<IEnumerable<UserGameDto>>(userGames);
            return await EnrichWithLikesAsync(dtos, viewerUserId);
        }

        public async Task<IEnumerable<UserGameDto>> GetDiscoverReviewsAsync(int userId, int skip, int take)
        {
            var myGames = await _userGameRepository.GetByUserIdAsync(userId);
            var myGameIds = myGames.Select(ug => ug.GameId).ToList();

            var candidates = (await _userGameRepository.GetOtherReviewsForGamesAsync(myGameIds, userId)).ToList();

            // Orden aleatorio: es un feed de descubrimiento, no un ranking
            // donde la consistencia entre páginas importe.
            var random = new Random();
            var shuffled = candidates.OrderBy(_ => random.Next()).Skip(skip).Take(take);

            var dtos = _mapper.Map<IEnumerable<UserGameDto>>(shuffled);
            return await EnrichWithLikesAsync(dtos, userId);
        }

        public async Task<IEnumerable<UserGameDto>> GetFollowingActivityAsync(int userId, int skip, int take)
        {
            var following = await _followRepository.GetFollowingAsync(userId);
            var followingIds = following.Select(f => f.FollowedId).ToList();

            if (followingIds.Count == 0)
                return Enumerable.Empty<UserGameDto>();

            var activity = await _userGameRepository.GetByUserIdsAsync(followingIds, skip, take);
            var dtos = _mapper.Map<IEnumerable<UserGameDto>>(activity);
            return await EnrichWithLikesAsync(dtos, userId);
        }

        public async Task<UserGameDto> CreateAsync(UserGameAddDto dto, int userId)
        {
            if (await _gameRepository.GetByIdAsync(dto.GameId) is null)
                throw new NotFoundException("El juego no existe.");

            if (await _userGameRepository.GetByUserAndGameAsync(userId, dto.GameId) is not null)
                throw new ConflictException("Ya tienes este juego en tu biblioteca.");

            var userGame = _mapper.Map<UserGame>(dto);
            userGame.UserId = userId;
            userGame.CreatedAt = DateTime.UtcNow;
            userGame.UpdatedAt = DateTime.UtcNow;

            if (userGame.Status == "Pendiente")
                userGame.Rate = null;

            await _userGameRepository.CreateAsync(userGame);
            await _unitOfWork.SaveChangesAsync();

            // Si el juego estaba en el backlog de "Juegos pendientes", ya no
            // pinta ahí: acaba de recibir una reseña de verdad.
            await _gameListService.RemoveFromPendingListIfPresentAsync(dto.GameId, userId);

            var created = await _userGameRepository.GetByIdAsync(userGame.Id);
            return _mapper.Map<UserGameDto>(created);
        }

        public async Task<UserGameDto> UpdateAsync(int id, UserGameModifyDto dto, int userId)
        {
            var userGame = await _userGameRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Esta entrada no existe.");

            if (userGame.UserId != userId)
                throw new ForbiddenException("No tienes permiso para editar esta entrada.");

            _mapper.Map(dto, userGame);
            userGame.UpdatedAt = DateTime.UtcNow;

            if (userGame.Status == "Pendiente")
                userGame.Rate = null;

            await _unitOfWork.SaveChangesAsync();

            var updatedDto = _mapper.Map<UserGameDto>(userGame);
            return (await EnrichWithLikesAsync(new[] { updatedDto }, userId)).Single();
        }

        public async Task DeleteAsync(int id, int userId)
        {
            var userGame = await _userGameRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Esta entrada no existe.");

            if (userGame.UserId != userId)
                throw new ForbiddenException("No tienes permiso para borrar esta entrada.");

            _userGameRepository.Delete(userGame);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<UserGameDto> LikeAsync(int userGameId, int userId)
        {
            var userGame = await _userGameRepository.GetByIdAsync(userGameId)
                ?? throw new NotFoundException("Esta reseña no existe.");

            if (await _reviewLikeRepository.GetAsync(userId, userGameId) is null)
            {
                await _reviewLikeRepository.CreateAsync(new ReviewLike { UserId = userId, UserGameId = userGameId });
                await _unitOfWork.SaveChangesAsync();
            }

            var dto = _mapper.Map<UserGameDto>(userGame);
            return (await EnrichWithLikesAsync(new[] { dto }, userId)).Single();
        }

        public async Task<UserGameDto> UnlikeAsync(int userGameId, int userId)
        {
            var userGame = await _userGameRepository.GetByIdAsync(userGameId)
                ?? throw new NotFoundException("Esta reseña no existe.");

            var existing = await _reviewLikeRepository.GetAsync(userId, userGameId);
            if (existing is not null)
            {
                _reviewLikeRepository.Delete(existing);
                await _unitOfWork.SaveChangesAsync();
            }

            var dto = _mapper.Map<UserGameDto>(userGame);
            return (await EnrichWithLikesAsync(new[] { dto }, userId)).Single();
        }

        /// <summary>
        /// Rellena LikeCount y LikedByMe en lote (nada de una consulta por
        /// cada reseña). viewerUserId nulo cuando nadie ha iniciado sesión:
        /// se rellena el conteo igualmente, LikedByMe se queda en false.
        /// </summary>
        private async Task<IEnumerable<UserGameDto>> EnrichWithLikesAsync(IEnumerable<UserGameDto> dtos, int? viewerUserId)
        {
            var list = dtos.ToList();
            var ids = list.Select(d => d.Id).ToList();
            if (ids.Count == 0)
                return list;

            var counts = await _reviewLikeRepository.GetCountsAsync(ids);
            var liked = viewerUserId is not null
                ? await _reviewLikeRepository.GetLikedUserGameIdsAsync(viewerUserId.Value, ids)
                : new HashSet<int>();

            foreach (var dto in list)
            {
                dto.LikeCount = counts.GetValueOrDefault(dto.Id);
                dto.LikedByMe = liked.Contains(dto.Id);
            }

            return list;
        }
    }
}
