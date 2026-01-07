namespace Podcast.Core.Entities;

public class PlayProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EpisodeId { get; set; }
    public int PositionSeconds { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Episode Episode { get; set; } = null!;
}

