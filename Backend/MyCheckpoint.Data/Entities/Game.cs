using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Entities
{
    public class Game : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
        public int? RawgId { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public int? Metacritic { get; set; }

        public List<GameListItem> GameListItems { get; set; }

        public List<UserGame> UserGames { get; set; }

        public List<GameCategory> GameCategories { get; set; }

    }
}
