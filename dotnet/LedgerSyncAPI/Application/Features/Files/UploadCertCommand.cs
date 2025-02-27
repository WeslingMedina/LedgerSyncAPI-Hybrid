using Application.Models;
using MediatR;
using Microsoft.AspNetCore.Http;


namespace Application.Features.Files
{
    public class UploadCertCommand : IRequest<FileUploadResult>
    {
        public IFormFile File { get; set; }
        public int UserId { get; set; }
        public string Iam { get; set; }
    }
}
