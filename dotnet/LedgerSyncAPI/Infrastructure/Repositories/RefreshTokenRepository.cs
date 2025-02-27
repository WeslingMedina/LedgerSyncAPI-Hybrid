using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Infrastructure.Database;

namespace Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;

        public RefreshTokenRepository(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task EnsureTableExistsAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS RefreshTokens (
                Token VARCHAR(88) PRIMARY KEY,
                Expires TIMESTAMP NOT NULL,
                Created TIMESTAMP NOT NULL,
                CreatedByIp VARCHAR(45) NOT NULL,
                Revoked TIMESTAMP NULL,
                RevokedByIp VARCHAR(45) NULL,
                ReplacedByToken VARCHAR(88) NULL,
                UserId INT NOT NULL REFERENCES Users(Id)
            )");
        }

        public async Task CreateAsync(RefreshToken token)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
            INSERT INTO RefreshTokens 
                (Token, Expires, Created, CreatedByIp, UserId)
            VALUES 
                (@Token, @Expires, @Created, @CreatedByIp, @UserId)";

            await connection.ExecuteAsync(sql, token);
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
            SELECT 
                Token,
                Expires,
                Created,
                CreatedByIp,
                Revoked,
                RevokedByIp,
                ReplacedByToken,
                UserId
            FROM RefreshTokens 
            WHERE Token = @Token";

            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { Token = token });
        }

        public async Task RevokeAsync(RefreshToken token, string ipAddress, string? replacedByToken = null)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
            UPDATE RefreshTokens SET
                Revoked = @Revoked,
                RevokedByIp = @RevokedByIp,
                ReplacedByToken = @ReplacedByToken
            WHERE Token = @Token";

            await connection.ExecuteAsync(sql, new
            {
                Token = token.Token,
                Revoked = DateTime.UtcNow,
                RevokedByIp = ipAddress,
                ReplacedByToken = replacedByToken
            });
        }

        public async Task<RefreshToken?> GetCurrentTokenAsync(int userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
            SELECT * FROM RefreshTokens 
            WHERE UserId = @UserId 
            AND Revoked IS NULL 
            AND Expires > CURRENT_TIMESTAMP
            ORDER BY Created DESC 
            LIMIT 1";

            return await connection.QueryFirstOrDefaultAsync<RefreshToken>(sql, new { UserId = userId });
        }
    }
}
