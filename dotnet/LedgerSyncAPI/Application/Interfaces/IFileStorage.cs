using Domain.Entities;
using Microsoft.AspNetCore.Http;


namespace Application.Interfaces
{
    public interface IFileStorage
    {
        Task<string> SaveFileAsync(IFormFile file, int userId, string type, string[] allowedExtensions);
        string GetFilePath(FileEntity file);
    }
}
