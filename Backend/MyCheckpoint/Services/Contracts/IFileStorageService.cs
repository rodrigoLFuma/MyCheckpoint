using Microsoft.AspNetCore.Http;

namespace MyCheckpoint.Services.Contracts
{
    public interface IFileStorageService
    {

        Task<string> SaveProfilePictureAsync(int userId, IFormFile file);

        void DeleteManagedFile(string? relativeUrl);
    }
}
