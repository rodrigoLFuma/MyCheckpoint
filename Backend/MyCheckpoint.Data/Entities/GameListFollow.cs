namespace MyCheckpoint.Data.Entities
{
    public class GameListFollow
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int GameListId { get; set; }
        public GameList GameList { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
