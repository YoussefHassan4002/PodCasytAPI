namespace Podcast.Core.Entities;

public class Subscription
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PodcastId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Podcast Podcast { get; set; } = null!;
}

