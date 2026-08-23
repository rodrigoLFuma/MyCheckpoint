using System;
using System.Collections.Generic;
using System.Text;

namespace MyCheckpoint.Data.Entities
{
    public class UserGame : IEntity
    {
        public int Id { get; set; }
        public string Status { get; set; }
        public int Rate { get; set; }
        public string Review { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PlayedDate { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int UserId { get; set; }
        public User User { get; set; }
        
        public int GameId { get; set; }
        public Game Game { get; set; }
    }
}
