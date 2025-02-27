using Application.Interfaces;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class FileStorage : IFileStorage
    {
        private readonly string _basePath;

        public FileStorage(IConfiguration config)
        {
            _basePath = config["FileStorage:BasePath"] ?? "App_Data/files";
        }

        public async Task<string> SaveFileAsync(IFormFile file, int userId, string type, string[] allowedExtensions)
        {
            var extension = Path.GetExtension(file.FileName).TrimStart('.');
            if (!allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                throw new InvalidOperationException("Invalid file extension");

            var directoryPath = Path.Combine(_basePath, userId.ToString(), type);
            Directory.CreateDirectory(directoryPath);

            var fileName = $"{Guid.NewGuid()}.{extension}";
            var filePath = Path.Combine(directoryPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return fileName;
        }

        public string GetFilePath(FileEntity file)
        {
            return Path.Combine(_basePath, file.IdUser.ToString(), file.FileType, file.Name);
        }
    }
}
