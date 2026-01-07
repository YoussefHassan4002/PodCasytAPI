namespace Podcast.Core.DTOs.Auth;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
}

