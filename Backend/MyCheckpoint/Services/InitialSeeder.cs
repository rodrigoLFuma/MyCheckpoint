using Microsoft.AspNetCore.Identity;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class InitialSeeder
    {
        private const string UserRole = "User";
        private const string AdminRole = "Admin";

        private const string AdminUserName = "Admin";
        private const string AdminEmail = "Admin@Admin.es";
        private const string AdminPassword = "Admin1234";

        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IGameListService _gameListService;
        private readonly ILogger<InitialSeeder> _logger;

        public InitialSeeder(
            UserManager<User> userManager,
            RoleManager<Role> roleManager,
            IGameListService gameListService,
            ILogger<InitialSeeder> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _gameListService = gameListService;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            await EnsureRoleAsync(UserRole);
            await EnsureRoleAsync(AdminRole);
            await EnsureAdminAsync();
        }

        private async Task EnsureRoleAsync(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                await _roleManager.CreateAsync(new Role { Name = roleName });
        }

        private async Task EnsureAdminAsync()
        {
            if (await _userManager.FindByEmailAsync(AdminEmail) is not null
                || await _userManager.FindByNameAsync(AdminUserName) is not null)
                return;

            var admin = new User { UserName = AdminUserName, Email = AdminEmail, CreatedAt = DateTime.UtcNow };

            var result = await _userManager.CreateAsync(admin, AdminPassword);
            if (!result.Succeeded)
            {
                _logger.LogError(
                    "No se ha podido crear el administrador inicial: {Errors}",
                    string.Join(" ", result.Errors.Select(e => e.Description)));
                return;
            }

            await _userManager.AddToRolesAsync(admin, new[] { UserRole, AdminRole });
            await _gameListService.EnsurePendingListAsync(admin.Id);

            _logger.LogInformation("Administrador inicial creado: {UserName}", AdminUserName);
        }
    }
}
