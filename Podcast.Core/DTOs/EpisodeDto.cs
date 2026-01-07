namespace Podcast.Core.DTOs;

public class EpisodeDto
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
    public int? CurrentPosition { get; set; }
}

