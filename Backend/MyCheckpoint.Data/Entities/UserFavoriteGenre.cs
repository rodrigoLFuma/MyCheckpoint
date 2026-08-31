namespace MyCheckpoint.Data.Entities
{
    public class UserFavoriteGenre
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
