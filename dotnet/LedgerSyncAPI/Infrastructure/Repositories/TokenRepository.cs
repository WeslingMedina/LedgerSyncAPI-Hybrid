using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Infrastructure.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public TokenRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<TokenResponse> RequestTokenAsync(TokenRequest request)
        {
            var baseUrls = _configuration.GetSection("AuthUrls").Get<Dictionary<string, string>>() ?? new();

            if (!baseUrls.TryGetValue(request.ClientId, out var url))
            {
                throw new ArgumentException("Invalid Client ID");
            }

            var content = new FormUrlEncodedContent(new Dictionary<string, string?>
                {
                    { "client_id", request.ClientId },
                    { "client_secret", request.ClientSecret },
                    { "grant_type", request.GrantType },
                    { "username", request.Username },
                    { "password", request.Password },
                    { "refresh_token", request.RefreshToken }
            }.Where(kv => kv.Value != null).ToDictionary(kv => kv.Key, kv => kv.Value!));

            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            var tokenData = JsonSerializer.Deserialize<Dictionary<string, object>>(responseString);

            return new TokenResponse
            {
                Status = (int)response.StatusCode,
                AccessToken = response.IsSuccessStatusCode ? tokenData?["access_token"]?.ToString() : null,
                Error = !response.IsSuccessStatusCode ? responseString : null
            };
        }
    }
}
