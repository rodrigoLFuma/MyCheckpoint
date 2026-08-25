using Microsoft.AspNetCore.Identity;

namespace MyCheckpoint.Data.Entities
{
    public class User : IdentityUser<int>, IEntity
    {
        public string? ProfilePictureURL { get; set; }
        public string? Bibliography { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<Follow> Followers { get; set; }
        public List<Follow> Followeds { get; set; }
        

        public List<GameList> GameLists { get; set; }

        public List<UserGame> UserGames { get; set; }


    }
}
