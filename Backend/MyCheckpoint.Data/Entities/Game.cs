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

        public List<GameListItem> GameListItems { get; set; }

        public List<UserGame> UserGames { get; set; }

        public List<GameCategory> GameCategories { get; set; }


    }
}
