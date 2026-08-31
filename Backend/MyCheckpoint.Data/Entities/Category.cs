using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Entities
{
    public class Category : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        // Slug del género en RAWG (p. ej. "role-playing-games-rpg"). Nulo para
        // categorías creadas a mano por un admin, sin equivalente en RAWG.
        public string? RawgSlug { get; set; }

        public List<GameCategory> GameCategories { get; set; }
        public List<UserFavoriteGenre> UserFavoriteGenres { get; set; }
    }
}
