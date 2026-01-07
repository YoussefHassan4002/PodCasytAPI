namespace Podcast.Core.Entities;

public class Rating
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? PodcastId { get; set; }
    public int? EpisodeId { get; set; }
    public int Stars { get; set; } // 1-5
    public string? ReviewText { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Podcast? Podcast { get; set; }
    public Episode? Episode { get; set; }
}

