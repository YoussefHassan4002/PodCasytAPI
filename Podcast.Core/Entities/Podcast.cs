namespace Podcast.Core.Entities;

public class Podcast
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Language { get; set; }
    public string? Categories { get; set; } // JSON array or comma-separated
    public string? RssUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastSyncedAt { get; set; }

    // Navigation properties
    public ICollection<Episode> Episodes { get; set; } = new List<Episode>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
}

