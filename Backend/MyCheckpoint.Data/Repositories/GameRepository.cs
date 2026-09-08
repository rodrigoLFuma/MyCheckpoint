using Microsoft.EntityFrameworkCore;
using MyCheckpoint.Data.Entities;
using MyCheckpoint.Data.Repositories.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Repositories
{
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

        // Nº mínimo de votos para que la nota de un juego cuente con
        // confianza casi plena en el ranking (ver fórmula más abajo).
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

            // Media pura, no: un juego con una sola reseña de 6 no debería
            // ganar a uno con cien reseñas de media 5.9. Se usa una media
            // bayesiana ponderada (la misma idea que el Top 250 de IMDB):
            // con pocos votos (v) la nota se atempera hacia la media global
            // (C); con muchos votos se acerca a la media real del juego (R).
            //   NotaPonderada = (v / (v + m)) * R + (m / (v + m)) * C
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

            // Where(Contains) no conserva el orden del ranking, así que se
            // reordena a mano según la lista ya ordenada por nota media.
            return ranked.Select(r => new GameRatingSummary
            {
                Game = gamesById[r.GameId],
                AverageRate = r.Avg,
                RatingsCount = r.Count
            });
        }
    }
}
