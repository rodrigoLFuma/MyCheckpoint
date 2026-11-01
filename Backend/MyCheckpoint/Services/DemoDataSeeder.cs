using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data;
using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Services
{
    public class DemoDataSeeder
    {
        private const string ResourceSuffix = "demo-data.json";
        private const string PasswordSuffix = "1234";

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<DemoDataSeeder> _logger;

        public DemoDataSeeder(
            ApplicationDbContext context,
            UserManager<User> userManager,
            ILogger<DemoDataSeeder> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            if (await _context.Users.CountAsync() > 1 || await _context.Games.AnyAsync())
                return;

            var data = await LoadDataAsync();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var categories = await ImportCategoriesAsync(data);
                var games = await ImportGamesAsync(data, categories);
                var users = await ImportUsersAsync(data, categories);
                var userGames = await ImportUserGamesAsync(data, users, games);
                var lists = await ImportListsAsync(data, users, games);
                await ImportInteractionsAsync(data, users, userGames, lists);

                await transaction.CommitAsync();
                _logger.LogInformation(
                    "Datos de demostración cargados: {Users} usuarios, {Games} juegos y {Reviews} entradas de biblioteca.",
                    data.Users.Count, data.Games.Count, data.UserGames.Count);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task<SeedData> LoadDataAsync()
        {
            var assembly = typeof(DemoDataSeeder).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .Single(name => name.EndsWith(ResourceSuffix, StringComparison.Ordinal));

            await using var stream = assembly.GetManifestResourceStream(resourceName)!;
            return await JsonSerializer.DeserializeAsync<SeedData>(stream, JsonOptions)
                ?? throw new InvalidOperationException("El archivo de datos de demostración está vacío.");
        }

        private async Task<Dictionary<int, Category>> ImportCategoriesAsync(SeedData data)
        {
            var categories = data.Categories.ToDictionary(
                c => c.Id,
                c => new Category { Name = c.Name, RawgSlug = c.RawgSlug });

            _context.Categories.AddRange(categories.Values);
            await _context.SaveChangesAsync();
            return categories;
        }

        private async Task<Dictionary<int, Game>> ImportGamesAsync(SeedData data, Dictionary<int, Category> categories)
        {
            var games = data.Games.ToDictionary(
                g => g.Id,
                g => new Game
                {
                    Name = g.Name,
                    ReleaseDate = g.ReleaseDate.UtcDateTime,
                    RawgId = g.RawgId,
                    Developer = g.Developer,
                    Publisher = g.Publisher,
                    CoverImageUrl = g.CoverImageUrl,
                    Metacritic = g.Metacritic,
                    GameCategories = g.CategoryIds
                        .Select(id => new GameCategory { Category = categories[id] })
                        .ToList()
                });

            _context.Games.AddRange(games.Values);
            await _context.SaveChangesAsync();
            return games;
        }

        private async Task<Dictionary<int, User>> ImportUsersAsync(SeedData data, Dictionary<int, Category> categories)
        {
            var users = new Dictionary<int, User>();

            foreach (var seed in data.Users)
            {
                var user = await _userManager.FindByNameAsync(seed.UserName);

                if (user is null)
                {
                    user = new User
                    {
                        UserName = seed.UserName,
                        Email = seed.Email,
                        Bibliography = seed.Bibliography,
                        CreatedAt = seed.CreatedAt.UtcDateTime
                    };

                    var result = await _userManager.CreateAsync(user, seed.UserName + PasswordSuffix);
                    if (!result.Succeeded)
                        throw new InvalidOperationException(
                            $"No se ha podido crear el usuario {seed.UserName}: {string.Join(" ", result.Errors.Select(e => e.Description))}");

                    await _userManager.AddToRolesAsync(user, seed.Roles);
                }

                users[seed.Id] = user;

                var currentGenreIds = await _context.UserFavoriteGenres
                    .Where(f => f.UserId == user.Id)
                    .Select(f => f.CategoryId)
                    .ToListAsync();

                foreach (var genreId in seed.FavoriteGenreIds)
                {
                    var categoryId = categories[genreId].Id;
                    if (!currentGenreIds.Contains(categoryId))
                        _context.UserFavoriteGenres.Add(new UserFavoriteGenre { UserId = user.Id, CategoryId = categoryId });
                }
            }

            await _context.SaveChangesAsync();
            return users;
        }

        private async Task<Dictionary<int, UserGame>> ImportUserGamesAsync(
            SeedData data, Dictionary<int, User> users, Dictionary<int, Game> games)
        {
            var userGames = data.UserGames.ToDictionary(
                ug => ug.Id,
                ug => new UserGame
                {
                    Status = ug.Status,
                    Rate = ug.Rate,
                    Title = ug.Title,
                    Review = ug.Review,
                    CreatedAt = ug.CreatedAt.UtcDateTime,
                    PlayedDate = ug.PlayedDate?.UtcDateTime,
                    UpdatedAt = ug.UpdatedAt.UtcDateTime,
                    UserId = users[ug.UserId].Id,
                    GameId = games[ug.GameId].Id
                });

            _context.UserGames.AddRange(userGames.Values);
            await _context.SaveChangesAsync();
            return userGames;
        }

        private async Task<Dictionary<int, GameList>> ImportListsAsync(
            SeedData data, Dictionary<int, User> users, Dictionary<int, Game> games)
        {
            var lists = new Dictionary<int, GameList>();

            foreach (var seed in data.Lists)
            {
                var ownerId = users[seed.UserId].Id;

                var list = await _context.GameLists
                    .Include(l => l.GameListItems)
                    .FirstOrDefaultAsync(l => l.UserId == ownerId && l.Name == seed.Name);

                if (list is null)
                {
                    list = new GameList
                    {
                        Name = seed.Name,
                        Description = seed.Description,
                        CreatedAt = seed.CreatedAt.UtcDateTime,
                        IsPublic = seed.IsPublic,
                        UserId = ownerId,
                        GameListItems = new List<GameListItem>()
                    };
                    _context.GameLists.Add(list);
                }

                foreach (var item in seed.Items)
                {
                    var gameId = games[item.GameId].Id;
                    if (list.GameListItems.All(i => i.GameId != gameId))
                        list.GameListItems.Add(new GameListItem { GameId = gameId, AddedAt = item.AddedAt.UtcDateTime });
                }

                lists[seed.Id] = list;
            }

            await _context.SaveChangesAsync();
            return lists;
        }

        private async Task ImportInteractionsAsync(
            SeedData data,
            Dictionary<int, User> users,
            Dictionary<int, UserGame> userGames,
            Dictionary<int, GameList> lists)
        {
            _context.GameListFollows.AddRange(data.ListFollows.Select(f => new GameListFollow
            {
                UserId = users[f.UserId].Id,
                GameListId = lists[f.ListId].Id,
                CreatedAt = f.CreatedAt.UtcDateTime
            }));

            _context.Follows.AddRange(data.Follows.Select(f => new Follow
            {
                FollowerId = users[f.FollowerId].Id,
                FollowedId = users[f.FollowedId].Id,
                CreatedAt = f.CreatedAt.UtcDateTime
            }));

            _context.ReviewLikes.AddRange(data.Likes.Select(l => new ReviewLike
            {
                UserId = users[l.UserId].Id,
                UserGameId = userGames[l.UserGameId].Id
            }));

            _context.Notifications.AddRange(data.Notifications.Select(n => new Notification
            {
                RecipientUserId = users[n.RecipientId].Id,
                ActorUserId = users[n.ActorId].Id,
                Type = n.Type,
                UserGameId = n.UserGameId is null ? null : userGames[n.UserGameId.Value].Id,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt.UtcDateTime
            }));

            await _context.SaveChangesAsync();
        }

        private sealed class SeedData
        {
            public List<CategorySeed> Categories { get; set; } = new();
            public List<GameSeed> Games { get; set; } = new();
            public List<UserSeed> Users { get; set; } = new();
            public List<UserGameSeed> UserGames { get; set; } = new();
            public List<ListSeed> Lists { get; set; } = new();
            public List<ListFollowSeed> ListFollows { get; set; } = new();
            public List<FollowSeed> Follows { get; set; } = new();
            public List<LikeSeed> Likes { get; set; } = new();
            public List<NotificationSeed> Notifications { get; set; } = new();
        }

        private sealed class CategorySeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string? RawgSlug { get; set; }
        }

        private sealed class GameSeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public DateTimeOffset ReleaseDate { get; set; }
            public int? RawgId { get; set; }
            public string? Developer { get; set; }
            public string? Publisher { get; set; }
            public string? CoverImageUrl { get; set; }
            public int? Metacritic { get; set; }
            public List<int> CategoryIds { get; set; } = new();
        }

        private sealed class UserSeed
        {
            public int Id { get; set; }
            public string UserName { get; set; } = "";
            public string Email { get; set; } = "";
            public string? Bibliography { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public List<string> Roles { get; set; } = new();
            public List<int> FavoriteGenreIds { get; set; } = new();
        }

        private sealed class UserGameSeed
        {
            public int Id { get; set; }
            public string Status { get; set; } = "";
            public int? Rate { get; set; }
            public string? Title { get; set; }
            public string? Review { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public DateTimeOffset? PlayedDate { get; set; }
            public DateTimeOffset UpdatedAt { get; set; }
            public int UserId { get; set; }
            public int GameId { get; set; }
        }

        private sealed class ListSeed
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
            public bool IsPublic { get; set; }
            public int UserId { get; set; }
            public List<ListItemSeed> Items { get; set; } = new();
        }

        private sealed class ListItemSeed
        {
            public int GameId { get; set; }
            public DateTimeOffset AddedAt { get; set; }
        }

        private sealed class ListFollowSeed
        {
            public int UserId { get; set; }
            public int ListId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
        }

        private sealed class FollowSeed
        {
            public int FollowerId { get; set; }
            public int FollowedId { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
        }

        private sealed class LikeSeed
        {
            public int UserId { get; set; }
            public int UserGameId { get; set; }
        }

        private sealed class NotificationSeed
        {
            public int RecipientId { get; set; }
            public int ActorId { get; set; }
            public string Type { get; set; } = "";
            public int? UserGameId { get; set; }
            public bool IsRead { get; set; }
            public DateTimeOffset CreatedAt { get; set; }
        }
    }
}
