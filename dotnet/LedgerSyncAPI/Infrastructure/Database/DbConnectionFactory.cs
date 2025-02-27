using MySqlConnector;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;

namespace Infrastructure.Database
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _connectionString = configuration.GetSection("ConnectionStrings")["DefaultConnection"]
                ?? throw new ArgumentNullException("Connection string is not configured.");
        }

        public IDbConnection CreateConnection()
        {
            return new MySqlConnection(_connectionString); // Use mysql for PostgreSQL
        }
    }
}
