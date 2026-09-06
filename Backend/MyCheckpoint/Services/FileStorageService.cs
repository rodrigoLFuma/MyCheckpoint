using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MyCheckpoint.Exceptions;
using MyCheckpoint.Services.Contracts;

namespace MyCheckpoint.Services
{
    public class FileStorageService : IFileStorageService
    {
        private const long MaxSizeBytes = 5 * 1024 * 1024;
        private const string ProfilePicturesFolder = "uploads/profile-pictures";

        // Se valida por content-type, no por la extensión del nombre de
        // archivo que manda el cliente (esa no es de fiar).
        private static readonly Dictionary<string, string> AllowedContentTypes = new()
        {
            ["image/jpeg"] = ".jpg",
            ["image/png"] = ".png",
            ["image/webp"] = ".webp",
            ["image/gif"] = ".gif",
        };

        private readonly IWebHostEnvironment _environment;

        public FileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveProfilePictureAsync(int userId, IFormFile file)
        {
            if (file is null || file.Length == 0)
                throw new BadRequestException("No se ha recibido ninguna imagen.");

            if (file.Length > MaxSizeBytes)
                throw new BadRequestException("La imagen no puede superar los 5 MB.");

            if (!AllowedContentTypes.TryGetValue(file.ContentType, out var extension))
                throw new BadRequestException("Formato de imagen no admitido. Usa JPG, PNG, WEBP o GIF.");

            var folderPath = Path.Combine(WebRootPath, ProfilePicturesFolder);
            Directory.CreateDirectory(folderPath);

            var fileName = $"{userId}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            await using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{ProfilePicturesFolder}/{fileName}";
        }

        public void DeleteManagedFile(string? relativeUrl)
        {
            if (string.IsNullOrWhiteSpace(relativeUrl) || !relativeUrl.StartsWith($"/{ProfilePicturesFolder}/"))
                return;

            var filePath = Path.Combine(WebRootPath, ProfilePicturesFolder, Path.GetFileName(relativeUrl));
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private string WebRootPath =>
            _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
    }
}
