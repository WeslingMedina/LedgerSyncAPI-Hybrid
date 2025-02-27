using Application.DTOs;
using Application.Interfaces;
using Application.Models;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly IJWTService _jwtService;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IPasswordService _passwordService;
        private readonly JwtSettings _jwtSettings;

        public AuthController(
            IJWTService jwtService,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IPasswordService passwordService,
            IOptions<JwtSettings> jwtSettings)
        {
            _jwtService = jwtService;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _passwordService = passwordService;
            _jwtSettings = jwtSettings.Value;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(UserRegisterRequest request)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(request.UserName);
            if (existingUser != null)
                return BadRequest("Username already exists");

            var user = new User
            {
                FullName = request.FullName,
                UserName = request.UserName,
                Email = request.Email,
                About = request.About,
                Country = request.Country,
                PasswordHash = _passwordService.HashPassword(request.Password)
            };

            var userId = await _userRepository.AddAsync(user);
            return Ok(new { userId });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(UserLoginRequest request, [FromHeader] string ipAddress)
        {
            var user = await _userRepository.GetByUsernameAsync(request.UserName);
            if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials");

            var accessToken = _jwtService.GenerateAccessToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken(ipAddress);

            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                Token = refreshToken.Token,
                Expires = refreshToken.Expires,
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserId = user.Id
            });

            return Ok(new
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token,
                ExpiresIn = _jwtSettings.AccessTokenExpiryMinutes * 60
            });
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest request, [FromHeader] string ipAddress)
        {
            var principal = _jwtService.ValidateToken(request.AccessToken);
            if (principal == null) return BadRequest("Invalid token");

            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(request.RefreshToken);

            if (storedRefreshToken == null || storedRefreshToken.UserId != userId || storedRefreshToken.Expires < DateTime.UtcNow)
                return Unauthorized("Refresh token inválido");

            var newAccessToken = _jwtService.GenerateAccessToken(await _userRepository.GetByIdAsync(userId));
            var newRefreshToken = _jwtService.GenerateRefreshToken(ipAddress);

            await _refreshTokenRepository.RevokeAsync(storedRefreshToken, ipAddress, newRefreshToken.Token);
            await _refreshTokenRepository.CreateAsync(new RefreshToken
            {
                Token = newRefreshToken.Token,
                Expires = newRefreshToken.Expires,
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                UserId = userId
            });

            return Ok(new
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var refreshToken = await _refreshTokenRepository.GetCurrentTokenAsync(userId);

            if (refreshToken != null)
            {
                await _refreshTokenRepository.RevokeAsync(refreshToken,
                    HttpContext.Connection.RemoteIpAddress?.ToString());
            }

            return NoContent();
        }
    }
}
