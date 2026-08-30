using AutoMapper;
using Microsoft.AspNetCore.Identity;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class AuthService : IAuthService
    {
        private const string DefaultRole = "User";

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;
        private readonly IGameListService _gameListService;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IJwtTokenGenerator jwtTokenGenerator,
            IGameListService gameListService,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _gameListService = gameListService;
            _mapper = mapper;
        }

        public async Task<AuthResponseDto> RegisterAsync(UserAddDto dto)
        {
            var user = _mapper.Map<User>(dto);
            user.CreatedAt = DateTime.UtcNow;

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                throw new ConflictException(string.Join(" ", result.Errors.Select(e => e.Description)));

            if (!await _roleManager.RoleExistsAsync(DefaultRole))
                await _roleManager.CreateAsync(new Role { Name = DefaultRole });

            await _userManager.AddToRoleAsync(user, DefaultRole);
            await _gameListService.EnsurePendingListAsync(user.Id);

            return await BuildAuthResponseAsync(user);
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedException("Credenciales inválidas.");

            return await BuildAuthResponseAsync(user);
        }

        private async Task<AuthResponseDto> BuildAuthResponseAsync(User user)
        {
            var (token, expiresAt) = await _jwtTokenGenerator.GenerateTokenAsync(user);

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = userDto
            };
        }
    }
}
