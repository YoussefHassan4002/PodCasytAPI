namespace Podcast.Core.Entities;

public class PlaylistItem
{
    public int Id { get; set; }
    public int PlaylistId { get; set; }
    public int EpisodeId { get; set; }
    public int Order { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Playlist Playlist { get; set; } = null!;
    public Episode Episode { get; set; } = null!;
}

