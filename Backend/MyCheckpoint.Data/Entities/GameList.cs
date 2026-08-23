using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Entities
{
    public class GameList : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsPublic { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }

        public List<GameListItem> GameListItems { get; set; }
    }
}
