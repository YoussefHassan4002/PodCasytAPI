namespace Podcast.Core.Entities;

public class Playlist
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();
}

