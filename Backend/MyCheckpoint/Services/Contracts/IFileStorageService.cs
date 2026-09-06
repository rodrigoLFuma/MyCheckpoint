using Microsoft.AspNetCore.Http;

namespace MyCheckpoint.Services.Contracts
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Guarda la imagen en disco (bajo wwwroot) y devuelve la URL relativa
        /// con la que servirla (p. ej. "/uploads/profile-pictures/3_ab12.jpg").
        /// Lanza BadRequestException si el archivo no es una imagen válida.
        /// </summary>
        Task<string> SaveProfilePictureAsync(int userId, IFormFile file);

        /// <summary>
        /// Borra el archivo si la URL es una que nosotros mismos gestionamos
        /// (p. ej. una foto subida antes). Si es una URL externa (o nula), no
        /// hace nada: no tenemos ningún archivo propio que borrar.
        /// </summary>
        void DeleteManagedFile(string? relativeUrl);
    }
}
