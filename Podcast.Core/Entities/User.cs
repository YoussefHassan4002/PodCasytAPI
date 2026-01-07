namespace Podcast.Core.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<PlayProgress> PlayProgresses { get; set; } = new List<PlayProgress>();
    public ICollection<Playlist> Playlists { get; set; } = new List<Playlist>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

