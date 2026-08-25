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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserGameService(
            IUserGameRepository userGameRepository,
            IGameRepository gameRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userGameRepository = userGameRepository;
            _gameRepository = gameRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserGameDto> GetByIdAsync(int id)
        {
            var userGame = await _userGameRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Esta entrada no existe.");

            return _mapper.Map<UserGameDto>(userGame);
        }

        public async Task<IEnumerable<UserGameDto>> GetByUserIdAsync(int userId)
        {
            var userGames = await _userGameRepository.GetByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<UserGameDto>>(userGames);
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

            await _userGameRepository.CreateAsync(userGame);
            await _unitOfWork.SaveChangesAsync();

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

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<UserGameDto>(userGame);
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
    }
}
