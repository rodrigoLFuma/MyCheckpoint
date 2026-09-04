namespace MyCheckpoint.Data.Entities
{
    public class Notification : IEntity
    {
        public int Id { get; set; }

        public int RecipientUserId { get; set; }
        public User RecipientUser { get; set; }

        public int ActorUserId { get; set; }
        public User ActorUser { get; set; }

        // "Follow" o "Like".
        public string Type { get; set; }

        // Solo para notificaciones de tipo "Like": la reseña a la que enlaza.
        public int? UserGameId { get; set; }
        public UserGame? UserGame { get; set; }

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
