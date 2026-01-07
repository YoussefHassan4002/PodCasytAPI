using Microsoft.Extensions.Logging;
using Podcast.Core.Interfaces;
using Entities = Podcast.Core.Entities;

namespace Podcast.Infrastructure.Services;

public class RssSyncJobService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRssIngestionService _rssIngestionService;
    private readonly ILogger<RssSyncJobService> _logger;

    public RssSyncJobService(
        IUnitOfWork unitOfWork,
        IRssIngestionService rssIngestionService,
        ILogger<RssSyncJobService> logger)
    {
        _unitOfWork = unitOfWork;
        _rssIngestionService = rssIngestionService;
        _logger = logger;
    }

    public async Task SyncAllPodcasts()
    {
        try
        {
            _logger.LogInformation("Starting RSS sync job at {Time}", DateTime.UtcNow);

            var podcasts = await _unitOfWork.Podcasts.GetAllAsync();
            var podcastList = podcasts.Cast<Entities.Podcast>()
                .Where(p => !string.IsNullOrEmpty(p.RssUrl))
                .ToList();

            _logger.LogInformation("Found {Count} podcasts to sync", podcastList.Count);

            foreach (var podcast in podcastList)
            {
                try
                {
                    await _rssIngestionService.SyncPodcastAsync(podcast.Id);
                    _logger.LogInformation("Successfully synced podcast {PodcastId}: {Title}", podcast.Id, podcast.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error syncing podcast {PodcastId}: {Title}", podcast.Id, podcast.Title);
                }
            }

            _logger.LogInformation("Completed RSS sync job at {Time}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RSS sync job");
            throw;
        }
    }
}

