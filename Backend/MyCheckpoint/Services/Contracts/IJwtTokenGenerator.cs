using MyCheckpoint.Data.Entities;

namespace MyCheckpoint.Services.Contracts
{
    public interface IJwtTokenGenerator
    {
        Task<(string Token, DateTime ExpiresAt)> GenerateTokenAsync(User user);
    }
}
