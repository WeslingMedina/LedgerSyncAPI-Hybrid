using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize]
    public class UsersController : Controller
    {
        private readonly IUserRepository _userRepository;

        public UsersController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return NotFound("Usuario no encontrado");

            var response = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                About = user.About,
                Country = user.Country,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser(UserUpdateRequest request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _userRepository.GetByIdAsync(userId);

            // Actualizar solo los campos proporcionados
            user.FullName = request.FullName ?? user.FullName;
            user.Email = request.Email ?? user.Email;
            user.About = request.About ?? user.About;
            user.Country = request.Country ?? user.Country;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Mapear a DTO de respuesta
            var response = new UserResponse
            {
                Id = user.Id,
                FullName = user.FullName,
                UserName = user.UserName,
                Email = user.Email,
                About = user.About,
                Country = user.Country,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Ok(response);
        }
    }
}
