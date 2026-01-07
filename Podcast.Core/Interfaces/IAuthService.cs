using Podcast.Core.DTOs.Auth;

namespace Podcast.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<bool> ValidateTokenAsync(string token);
    Task<string> GenerateTokenAsync(int userId, string email);
    Task<string> GenerateRefreshTokenAsync(int userId);
    Task<bool> ValidateRefreshTokenAsync(string refreshToken);
}

