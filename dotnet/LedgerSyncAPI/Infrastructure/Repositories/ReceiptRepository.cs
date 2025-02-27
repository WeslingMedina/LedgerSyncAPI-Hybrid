using Application.Interfaces;
using Dapper;
using Domain.Entities;
using Infrastructure.Database;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ReceiptRepository : IReceiptRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;

        public ReceiptRepository(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task EnsureTableExistsAsync()
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS receipt_logs (
                    id SERIAL PRIMARY KEY,
                    clave VARCHAR(50) NOT NULL,
                    fecha VARCHAR(50) NOT NULL,
                    emisor_tipo VARCHAR(50) NOT NULL,
                    emisor_numero VARCHAR(50) NOT NULL,
                    receptor_tipo VARCHAR(50),
                    receptor_numero VARCHAR(50),
                    has_callback BOOLEAN NOT NULL,
                    client_id VARCHAR(20) NOT NULL,
                    status_code INT NOT NULL,
                    created_at TIMESTAMP NOT NULL
                )");
        }

        public async Task LogReceiptRequestAsync(ReceiptSenderRequest request, string clientId, int statusCode)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO receipt_logs (clave, fecha, emisor_tipo, emisor_numero, 
                receptor_tipo, receptor_numero, has_callback, client_id, status_code, created_at)
                VALUES (@Clave, @Fecha, @EmisorTipo, @EmisorNumero, 
                @ReceptorTipo, @ReceptorNumero, @HasCallback, @ClientId, @StatusCode, @CreatedAt)";

            await connection.ExecuteAsync(sql, new
            {
                request.clave,
                request.fecha,
                EmisorTipo = request.emisor.tipoIdentificacion,
                EmisorNumero = request.emisor.numeroIdentificacion,
                ReceptorTipo = request.receptor?.tipoIdentificacion,
                ReceptorNumero = request.receptor?.numeroIdentificacion,
                HasCallback = !string.IsNullOrEmpty(request.callbackUrl),
                ClientId = clientId,
                StatusCode = statusCode,
                CreatedAt = DateTime.UtcNow
            });
        }
    }
}
