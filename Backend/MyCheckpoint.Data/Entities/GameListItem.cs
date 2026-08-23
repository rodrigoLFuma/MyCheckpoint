using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Entities
{
    public class GameListItem
    {
        public int GameListId { get; set; }
        public GameList GameList { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        public DateTime AddedAt { get; set; }
    }
}
