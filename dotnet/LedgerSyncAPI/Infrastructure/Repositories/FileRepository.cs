using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Database;
using Dapper;

namespace Infrastructure.Repositories
{
    public class FileRepository : IFileRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;

        public FileRepository(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }
        public async Task EnsureTableExistsAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS filesNET (
                    IdFile SERIAL PRIMARY KEY,
                    Md5 VARCHAR(32),
                    Name VARCHAR(255),
                    Timestamp TIMESTAMP,
                    Size BIGINT,
                    IdUser INT,
                    DownloadCode VARCHAR(32),
                    FileType VARCHAR(50),
                    Type VARCHAR(50)
                )");
        }

        public async Task<int> SaveAsync(FileEntity file)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QuerySingleAsync<int>(@"
            INSERT INTO filesNET (Md5, Name, Timestamp, Size, IdUser, DownloadCode, FileType, Type)
            VALUES (@Md5, @Name, @Timestamp, @Size, @IdUser, @DownloadCode, @FileType, @Type)
            RETURNING IdFile", file);
        }

        public async Task<FileEntity> GetByDownloadCodeAsync(string code)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<FileEntity>("SELECT * FROM filesNET WHERE DownloadCode = @code", new { code });

        }
    }
}
