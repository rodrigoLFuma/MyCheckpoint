namespace MyCheckpoint.Data.Entities
{
    public class ReviewLike
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int UserGameId { get; set; }
        public UserGame UserGame { get; set; }
    }
}
