using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly IFollowRepository _followRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(
            UserManager<User> userManager,
            IFollowRepository followRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper)
        {
            _userManager = userManager;
            _followRepository = followRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserSummaryDto> GetByIdAsync(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            return _mapper.Map<UserSummaryDto>(user);
        }

        public async Task<UserDto> GetMyProfileAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return userDto;
        }

        public async Task<UserDto> UpdateProfileAsync(int userId, UserModifyDto dto)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            _mapper.Map(dto, user);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
            return userDto;
        }

        public async Task DeleteMyAccountAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString())
                ?? throw new NotFoundException("El usuario no existe.");

            // Follow tiene OnDelete(Restrict) en sus dos relaciones con User
            // (evita el problema de doble cascada sobre la misma tabla), así
            // que hay que borrar a mano las filas de follows antes de poder
            // borrar al usuario.
            var followers = await _followRepository.GetFollowersAsync(userId);
            var following = await _followRepository.GetFollowingAsync(userId);
            foreach (var follow in followers.Concat(following))
                _followRepository.Delete(follow);
            await _unitOfWork.SaveChangesAsync();

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));
        }
    }
}
