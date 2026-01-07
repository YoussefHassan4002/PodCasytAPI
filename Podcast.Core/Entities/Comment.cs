namespace Podcast.Core.Entities;

public class Comment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int EpisodeId { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = null!;
    public Episode Episode { get; set; } = null!;
}

