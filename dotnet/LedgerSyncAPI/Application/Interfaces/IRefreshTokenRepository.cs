using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task CreateAsync(RefreshToken token);
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeAsync(RefreshToken token, string ipAddress, string? replacedByToken = null);
        Task<RefreshToken?> GetCurrentTokenAsync(int userId);
        Task EnsureTableExistsAsync();
    }
}
