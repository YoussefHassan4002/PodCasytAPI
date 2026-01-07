namespace Podcast.Core.Interfaces;

public interface IRssIngestionService
{
    Task<bool> SyncPodcastAsync(int podcastId);
    Task<int> ImportPodcastFromRssAsync(string rssUrl);
}

