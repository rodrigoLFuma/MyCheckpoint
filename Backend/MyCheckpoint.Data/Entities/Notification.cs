namespace MyCheckpoint.Data.Entities
{
    public class Notification : IEntity
    {
        public int Id { get; set; }

        public int RecipientUserId { get; set; }
        public User RecipientUser { get; set; }

        public int ActorUserId { get; set; }
        public User ActorUser { get; set; }

        public string Type { get; set; }

        public int? UserGameId { get; set; }
        public UserGame? UserGame { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
