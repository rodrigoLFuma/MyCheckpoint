using Microsoft.AspNetCore.Identity;
using Moq;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Models.Dtos;
using MyCheckpoint.Services;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Tests
{

    public class AuthServiceTests
    {
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<RoleManager<Role>> _roleManager;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
        private readonly Mock<IGameListService> _gameListService = new();
        private readonly AuthService _sut;

        public AuthServiceTests()
        {
            _userManager = TestHelpers.CreateUserManagerMock();
            _roleManager = TestHelpers.CreateRoleManagerMock();
            _jwtTokenGenerator
                .Setup(j => j.GenerateTokenAsync(It.IsAny<User>()))
                .ReturnsAsync(("fake-jwt", DateTime.UtcNow.AddHours(2)));

            _sut = new AuthService(
                _userManager.Object,
                _roleManager.Object,
                _jwtTokenGenerator.Object,
                _gameListService.Object,
                TestHelpers.CreateMapper());
        }

        private static User MakeUser(int id, string userName, string email) => new()
        {
            Id = id,
            UserName = userName,
            Email = email,
            FavoriteGenres = new List<UserFavoriteGenre>(),
        };

        [Fact]
        public async Task RegisterAsync_IdentityRejectsPassword_ThrowsConflictWithIdentityMessage()
        {
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<User>(), "1234"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Las contraseñas deben tener al menos 8 caracteres." }));
            var dto = new UserAddDto { UserName = "nuevo", Email = "nuevo@test.es", Password = "1234" };

            var ex = await Assert.ThrowsAsync<ConflictException>(() => _sut.RegisterAsync(dto));

            Assert.Contains("8 caracteres", ex.Message);
            _roleManager.Verify(r => r.RoleExistsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_RoleDoesNotExistYet_CreatesItOnce()
        {
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<User>(), "Passw0rd"))
                .ReturnsAsync(IdentityResult.Success);
            _roleManager.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(false);
            _userManager.Setup(m => m.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "User" });
            var dto = new UserAddDto { UserName = "primero", Email = "primero@test.es", Password = "Passw0rd" };

            await _sut.RegisterAsync(dto);

            _roleManager.Verify(r => r.CreateAsync(It.Is<Role>(role => role.Name == "User")), Times.Once);
            _userManager.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), "User"), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_RoleAlreadyExists_DoesNotRecreateIt()
        {
            _userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), "Passw0rd")).ReturnsAsync(IdentityResult.Success);
            _roleManager.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);
            _userManager.Setup(m => m.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "User" });
            var dto = new UserAddDto { UserName = "otro", Email = "otro@test.es", Password = "Passw0rd" };

            await _sut.RegisterAsync(dto);

            _roleManager.Verify(r => r.CreateAsync(It.IsAny<Role>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_Success_EnsuresPendingListAndReturnsToken()
        {
            User? created = null;
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<User>(), "Passw0rd"))
                .Callback<User, string>((u, _) => { u.Id = 5; created = u; })
                .ReturnsAsync(IdentityResult.Success);
            _roleManager.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);
            _userManager.Setup(m => m.GetRolesAsync(It.IsAny<User>())).ReturnsAsync(new List<string> { "User" });
            var dto = new UserAddDto { UserName = "gamer", Email = "gamer@test.es", Password = "Passw0rd" };

            var result = await _sut.RegisterAsync(dto);

            Assert.Equal("fake-jwt", result.Token);
            Assert.Equal("gamer", result.User.UserName);
            Assert.Contains("User", result.User.Roles);
            _gameListService.Verify(s => s.EnsurePendingListAsync(5), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_UnknownEmailOrUserName_ThrowsUnauthorized()
        {
            _userManager.Setup(m => m.FindByEmailAsync("ghost@test.es")).ReturnsAsync((User?)null);
            _userManager.Setup(m => m.FindByNameAsync("ghost@test.es")).ReturnsAsync((User?)null);
            var dto = new LoginDto { EmailOrUserName = "ghost@test.es", Password = "Passw0rd" };

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsUnauthorized()
        {
            var user = MakeUser(1, "gamer", "gamer@test.es");
            _userManager.Setup(m => m.FindByEmailAsync("gamer@test.es")).ReturnsAsync(user);
            _userManager.Setup(m => m.CheckPasswordAsync(user, "malacontraseña")).ReturnsAsync(false);
            var dto = new LoginDto { EmailOrUserName = "gamer@test.es", Password = "malacontraseña" };

            await Assert.ThrowsAsync<UnauthorizedException>(() => _sut.LoginAsync(dto));
        }

        [Fact]
        public async Task LoginAsync_ValidCredentialsByUserName_ReturnsToken()
        {
            var user = MakeUser(1, "gamer", "gamer@test.es");

            _userManager.Setup(m => m.FindByEmailAsync("gamer")).ReturnsAsync((User?)null);
            _userManager.Setup(m => m.FindByNameAsync("gamer")).ReturnsAsync(user);
            _userManager.Setup(m => m.CheckPasswordAsync(user, "Passw0rd")).ReturnsAsync(true);
            _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { "User" });
            var dto = new LoginDto { EmailOrUserName = "gamer", Password = "Passw0rd" };

            var result = await _sut.LoginAsync(dto);

            Assert.Equal("fake-jwt", result.Token);
        }
    }
}
