using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;

        public UserRepository(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task EnsureTableExistsAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS Users (
                Id SERIAL PRIMARY KEY,
                FullName VARCHAR(100) NOT NULL,
                UserName VARCHAR(50) UNIQUE NOT NULL,
                Email VARCHAR(100) UNIQUE NOT NULL,
                About TEXT,
                Country VARCHAR(50),
                PasswordHash TEXT NOT NULL,
                CreatedAt TIMESTAMP NOT NULL,
                UpdatedAt TIMESTAMP
            )");
        }

        public async Task<int> AddAsync(User user)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO Users
                (FullName, UserName, Email, About, Country, PasswordHash, CreatedAt)
            VALUES 
                (@FullName, @UserName, @Email, @About, @Country, @PasswordHash, @CreatedAt)
            RETURNING Id";

            return await connection.ExecuteScalarAsync<int>(sql, user);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE UserName = @username",
                new { username });
        }

        public async Task UpdateAsync(User user)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            const string sql = @"
            UPDATE Users SET
                FullName = @FullName,
                Email = @Email,
                About = @About,
                Country = @Country,
                PasswordHash = @PasswordHash,
                UpdatedAt = @UpdatedAt
            WHERE Id = @Id";

            await connection.ExecuteAsync(sql, user);
        }

        public async Task<User> GetByIdAsync(int id)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<User>(
                "SELECT * FROM Users WHERE Id = @id",
                new { id });
        }
    }
}
