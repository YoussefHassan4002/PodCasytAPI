namespace Podcast.Core.Entities;

public class Episode
{
    public int Id { get; set; }
    public int PodcastId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string AudioUrl { get; set; } = string.Empty;
    public int? DurationSeconds { get; set; }
    public DateTime? PublishDate { get; set; }
    public int? EpisodeNumber { get; set; }
    public int? SeasonNumber { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Podcast Podcast { get; set; } = null!;
    public ICollection<PlayProgress> PlayProgresses { get; set; } = new List<PlayProgress>();
    public ICollection<PlaylistItem> PlaylistItems { get; set; } = new List<PlaylistItem>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

