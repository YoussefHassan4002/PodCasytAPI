namespace Podcast.Core.DTOs;

public class PodcastDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Language { get; set; }
    public string? Categories { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EpisodeCount { get; set; }
}

