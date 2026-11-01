using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Services;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Tests
{

    public class InitialSeederTests
    {
        private readonly Mock<UserManager<User>> _userManager;
        private readonly Mock<RoleManager<Role>> _roleManager;
        private readonly Mock<IGameListService> _gameListService = new();
        private readonly InitialSeeder _sut;

        public InitialSeederTests()
        {
            _userManager = TestHelpers.CreateUserManagerMock();
            _roleManager = TestHelpers.CreateRoleManagerMock();
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManager
                .Setup(m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(IdentityResult.Success);

            _sut = new InitialSeeder(
                _userManager.Object,
                _roleManager.Object,
                _gameListService.Object,
                NullLogger<InitialSeeder>.Instance);
        }

        [Fact]
        public async Task SeedAsync_EmptyDatabase_CreatesBothRoles()
        {
            await _sut.SeedAsync();

            _roleManager.Verify(r => r.CreateAsync(It.Is<Role>(role => role.Name == "User")), Times.Once);
            _roleManager.Verify(r => r.CreateAsync(It.Is<Role>(role => role.Name == "Admin")), Times.Once);
        }

        [Fact]
        public async Task SeedAsync_RolesAlreadyExist_DoesNotRecreateThem()
        {
            _roleManager.Setup(r => r.RoleExistsAsync("User")).ReturnsAsync(true);
            _roleManager.Setup(r => r.RoleExistsAsync("Admin")).ReturnsAsync(true);

            await _sut.SeedAsync();

            _roleManager.Verify(r => r.CreateAsync(It.IsAny<Role>()), Times.Never);
        }

        [Fact]
        public async Task SeedAsync_EmptyDatabase_CreatesAdminWithBothRolesAndPendingList()
        {
            await _sut.SeedAsync();

            _userManager.Verify(m => m.CreateAsync(
                It.Is<User>(u => u.UserName == "Admin" && u.Email == "Admin@Admin.es"),
                It.Is<string>(password => !string.IsNullOrEmpty(password))), Times.Once);
            _userManager.Verify(m => m.AddToRolesAsync(
                It.IsAny<User>(),
                It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(new[] { "User", "Admin" }))), Times.Once);
            _gameListService.Verify(g => g.EnsurePendingListAsync(It.IsAny<int>()), Times.Once);
        }

        [Fact]
        public async Task SeedAsync_AdminEmailAlreadyExists_DoesNotCreateNorModifyIt()
        {
            _userManager
                .Setup(m => m.FindByEmailAsync("Admin@Admin.es"))
                .ReturnsAsync(new User { Id = 1, UserName = "otro", Email = "Admin@Admin.es" });

            await _sut.SeedAsync();

            _userManager.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
            _userManager.Verify(m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        }

        [Fact]
        public async Task SeedAsync_AdminUserNameAlreadyExists_DoesNotCreateIt()
        {
            _userManager
                .Setup(m => m.FindByNameAsync("Admin"))
                .ReturnsAsync(new User { Id = 1, UserName = "Admin", Email = "otro@test.es" });

            await _sut.SeedAsync();

            _userManager.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SeedAsync_IdentityRejectsCreation_DoesNotAssignRolesNorCreateList()
        {
            _userManager
                .Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Contraseña demasiado corta." }));

            await _sut.SeedAsync();

            _userManager.Verify(m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
            _gameListService.Verify(g => g.EnsurePendingListAsync(It.IsAny<int>()), Times.Never);
        }
    }
}
