using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using Podcast.Core.Entities;
using Podcast.Core.Interfaces;
using Entities = Podcast.Core.Entities;

namespace Podcast.Infrastructure.Services;

public class RssIngestionService : IRssIngestionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RssIngestionService> _logger;

    public RssIngestionService(IUnitOfWork unitOfWork, ILogger<RssIngestionService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<bool> SyncPodcastAsync(int podcastId)
    {
        try
        {
            var podcast = await _unitOfWork.Podcasts.GetByIdAsync(podcastId);
            if (podcast == null || string.IsNullOrEmpty(podcast.RssUrl))
            {
                return false;
            }

            var feed = await FeedReader.ReadAsync(podcast.RssUrl);

            // Update podcast metadata if available
            if (!string.IsNullOrEmpty(feed.Title))
                podcast.Title = feed.Title;
            if (!string.IsNullOrEmpty(feed.Description))
                podcast.Description = feed.Description;
            if (!string.IsNullOrEmpty(feed.ImageUrl))
                podcast.ImageUrl = feed.ImageUrl;
            if (!string.IsNullOrEmpty(feed.Language))
                podcast.Language = feed.Language;

            podcast.LastSyncedAt = DateTime.UtcNow;
            await _unitOfWork.Podcasts.UpdateAsync(podcast);

            // Process episodes
            var existingEpisodes = (await _unitOfWork.Episodes.FindAsync(e => e.PodcastId == podcastId)).ToList();
            var existingGuids = existingEpisodes.Select(e => e.AudioUrl).ToHashSet();

            foreach (var item in feed.Items)
            {
                var enclosure = item.SpecificItem?.Element?.Elements()
                    .FirstOrDefault(e => e.Name.LocalName == "enclosure");

                var audioUrl = enclosure?.Attribute("url")?.Value ?? string.Empty;
                if (string.IsNullOrEmpty(audioUrl))
                {
                    // Try to find audio URL in description or content
                    audioUrl = ExtractAudioUrl(item.Content ?? item.Description ?? string.Empty);
                }

                if (string.IsNullOrEmpty(audioUrl) || existingGuids.Contains(audioUrl))
                {
                    continue;
                }

                var episode = new Entities.Episode
                {
                    PodcastId = podcastId,
                    Title = item.Title ?? "Untitled Episode",
                    Description = item.Description ?? item.Content,
                    AudioUrl = audioUrl,
                    PublishDate = item.PublishingDate,
                    CreatedAt = DateTime.UtcNow
                };

                // Try to extract episode/season numbers from title
                ExtractEpisodeNumbers(item.Title ?? string.Empty, out var episodeNum, out var seasonNum);
                episode.EpisodeNumber = episodeNum;
                episode.SeasonNumber = seasonNum;

                // Try to extract duration
                var duration = ExtractDuration(item.Description ?? item.Content ?? string.Empty);
                episode.DurationSeconds = duration;

                // Try to extract image
                episode.ImageUrl = ExtractImageUrl(item.Description ?? item.Content ?? string.Empty) ?? podcast.ImageUrl;

                await _unitOfWork.Episodes.AddAsync(episode);
                existingGuids.Add(audioUrl);
            }

            await _unitOfWork.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing podcast {PodcastId}", podcastId);
            return false;
        }
    }

    public async Task<int> ImportPodcastFromRssAsync(string rssUrl)
    {
        try
        {
            var feed = await FeedReader.ReadAsync(rssUrl);

            var podcast = new Entities.Podcast
            {
                Title = feed.Title ?? "Untitled Podcast",
                Description = feed.Description ?? string.Empty,
                ImageUrl = feed.ImageUrl,
                Language = feed.Language,
                RssUrl = rssUrl,
                CreatedAt = DateTime.UtcNow,
                LastSyncedAt = DateTime.UtcNow
            };

            // Try to extract author from feed
            var author = feed.Items.FirstOrDefault()?.Author ?? string.Empty;
            podcast.Author = author;

            await _unitOfWork.Podcasts.AddAsync(podcast);
            await _unitOfWork.SaveChangesAsync();

            // Sync episodes
            await SyncPodcastAsync(podcast.Id);

            return podcast.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing podcast from RSS {RssUrl}", rssUrl);
            throw;
        }
    }

    private string? ExtractAudioUrl(string content)
    {
        // Simple regex to find audio URLs
        var patterns = new[]
        {
            @"https?://[^\s""<>]+\.(mp3|m4a|wav|ogg|aac)",
            @"<enclosure[^>]+url=[""']([^""']+)[""']"
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                return match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
            }
        }

        return null;
    }

    private void ExtractEpisodeNumbers(string title, out int? episodeNumber, out int? seasonNumber)
    {
        episodeNumber = null;
        seasonNumber = null;

        // Try patterns like "S01E05", "Episode 5", "Ep. 5", "Season 1 Episode 5"
        var patterns = new[]
        {
            @"[Ss](\d+)[Ee](\d+)", // S01E05
            @"[Ee]pisode\s+(\d+)", // Episode 5
            @"[Ee]p\.\s*(\d+)", // Ep. 5
            @"[Ss]eason\s+(\d+)\s+[Ee]pisode\s+(\d+)" // Season 1 Episode 5
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(title, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (match.Groups.Count >= 2 && int.TryParse(match.Groups[1].Value, out var epNum))
                {
                    episodeNumber = epNum;
                }
                if (match.Groups.Count >= 3 && int.TryParse(match.Groups[2].Value, out var seasonNum))
                {
                    seasonNumber = seasonNum;
                }
                break;
            }
        }
    }

    private int? ExtractDuration(string content)
    {
        // Try to find duration patterns like "1:23:45", "83:45", "5034 seconds"
        var patterns = new[]
        {
            @"(\d+):(\d+):(\d+)", // HH:MM:SS
            @"(\d+):(\d+)", // MM:SS
            @"(\d+)\s*(?:seconds?|secs?)" // seconds
        };

        foreach (var pattern in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(content, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (match.Success)
            {
                if (match.Groups.Count == 4) // HH:MM:SS
                {
                    var hours = int.Parse(match.Groups[1].Value);
                    var minutes = int.Parse(match.Groups[2].Value);
                    var seconds = int.Parse(match.Groups[3].Value);
                    return hours * 3600 + minutes * 60 + seconds;
                }
                else if (match.Groups.Count == 3) // MM:SS
                {
                    var minutes = int.Parse(match.Groups[1].Value);
                    var seconds = int.Parse(match.Groups[2].Value);
                    return minutes * 60 + seconds;
                }
                else if (match.Groups.Count == 2) // seconds
                {
                    if (int.TryParse(match.Groups[1].Value, out var secs))
                        return secs;
                }
            }
        }

        return null;
    }

    private string? ExtractImageUrl(string content)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            content,
            @"<img[^>]+src=[""']([^""']+)[""']",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[1].Value : null;
    }
}

