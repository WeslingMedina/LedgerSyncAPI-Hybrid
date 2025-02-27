using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IJWTService
    {
        string GenerateAccessToken(User user); // Método para generar el token de acceso
        RefreshToken GenerateRefreshToken(string ipAddress); // Método para generar el token de refresco
        ClaimsPrincipal? ValidateToken(string token); // Método para validar un token
    }
}
