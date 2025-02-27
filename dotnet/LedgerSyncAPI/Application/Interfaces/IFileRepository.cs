using Domain.Entities;

namespace Application.Interfaces
{
    public interface IFileRepository
    {
        Task<int> SaveAsync(FileEntity file);
        Task<FileEntity> GetByDownloadCodeAsync(string code);
        Task EnsureTableExistsAsync();
    }
}
