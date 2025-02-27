using Application.Interfaces;
using Application.Models;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Files
{
    public class UploadCertHandler : IRequestHandler<UploadCertCommand, FileUploadResult>
    {
        private readonly IFileRepository _repository;
        private readonly IFileStorage _storage;

        public UploadCertHandler(IFileRepository repository, IFileStorage storage)
        {
            _repository = repository;
            _storage = storage;
        }

        public async Task<FileUploadResult> Handle(UploadCertCommand request, CancellationToken cancellationToken)
        {
            var fileName = await _storage.SaveFileAsync(
                request.File,
                request.UserId,
                "certificate",
                new[] { "p12" });

            var file = new FileEntity
            {
                Md5 = ComputeMd5(request.File),
                Name = fileName,
                Timestamp = DateTime.UtcNow,
                Size = request.File.Length,
                IdUser = request.UserId,
                DownloadCode = GenerateDownloadCode(fileName, request.UserId),
                FileType = "certificate",
                Type = "p12"
            };

            var id = await _repository.SaveAsync(file);

            return new FileUploadResult(id, fileName, file.DownloadCode);
        }

        private static string ComputeMd5(IFormFile file)
        {
            using var md5 = MD5.Create();
            using var stream = file.OpenReadStream();
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
        }

        private static string GenerateDownloadCode(string fileName, int userId)
        {
            return Convert.ToHexString(MD5.HashData(
                Encoding.UTF8.GetBytes($"{fileName}{userId}{DateTime.UtcNow.Ticks}")));
        }
    }
}
