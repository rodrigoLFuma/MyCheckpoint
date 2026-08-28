namespace MyCheckpoint.Models.Dtos
{
    public class AuthUserResponseDto
    {
        public UserDto User { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
