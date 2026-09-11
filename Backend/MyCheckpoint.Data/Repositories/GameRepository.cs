using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories
{
    public class GameRatingSummary
    {
        public Game Game { get; set; } = null!;
        public double AverageRate { get; set; }
        public int RatingsCount { get; set; }
    }
    public class GameRepository : RepositoryBaseWithId<Game>, IGameRepository
    {
        public GameRepository(ApplicationDbContext context) : base(context)
        {

        }

        public override async Task<Game?> GetByIdAsync(int id)
        {
            return await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .FirstOrDefaultAsync(g => g.Id == id);
        }

        public override async Task<IEnumerable<Game>> GetAllAsync()
        {
            return await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .ToListAsync();
        }

        public async Task<IEnumerable<Game>> GetByCategoryAsync(IEnumerable<int> categoryIds)
        {
            return await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .Where(g => g.GameCategories.Any(gc => categoryIds.Contains(gc.CategoryId)))
                    .ToListAsync();
        }

        public async Task<IEnumerable<Game>> SearchByNameAsync(string term)
        {
            return await _context.Set<Game>()
                .Where(g => EF.Functions.ILike(g.Name, $"%{term}%"))
                .ToListAsync();
        }

        public async Task<Game?> GetByRawgIdAsync(int rawgId)
        {
            return await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .FirstOrDefaultAsync(g => g.RawgId == rawgId);
        }

        private const double MinVotesForFullConfidence = 5;

        public async Task<IEnumerable<GameRatingSummary>> GetTopRatedAsync(int skip, int take)
        {
            var ratings = await _context.Set<UserGame>()
                    .Where(ug => ug.Rate != null)
                    .GroupBy(ug => ug.GameId)
                    .Select(g => new { GameId = g.Key, Avg = g.Average(ug => ug.Rate!.Value), Count = g.Count() })
                    .ToListAsync();

            if (ratings.Count == 0)
                return Enumerable.Empty<GameRatingSummary>();

            var globalAverage = await _context.Set<UserGame>()
                    .Where(ug => ug.Rate != null)
                    .AverageAsync(ug => ug.Rate!.Value);

            var ranked = ratings
                    .OrderByDescending(r => (r.Count / (r.Count + MinVotesForFullConfidence)) * r.Avg
                        + (MinVotesForFullConfidence / (r.Count + MinVotesForFullConfidence)) * globalAverage)
                    .Skip(skip)
                    .Take(take)
                    .ToList();

            var gameIds = ranked.Select(r => r.GameId).ToList();
            var games = await _context.Set<Game>()
                    .Include(g => g.GameCategories)
                    .ThenInclude(gc => gc.Category)
                    .Where(g => gameIds.Contains(g.Id))
                    .ToListAsync();
            var gamesById = games.ToDictionary(g => g.Id);

            return ranked.Select(r => new GameRatingSummary
            {
                Game = gamesById[r.GameId],
                AverageRate = r.Avg,
                RatingsCount = r.Count
            });
        }
    }
}
