using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Podcast.Core.DTOs;
using Podcast.Core.Entities;
using Podcast.Core.Interfaces;
using System.Security.Claims;
using Entities = Podcast.Core.Entities;

namespace Podcast.Api.Controllers;

[ApiController]
[Route("api/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<MeController> _logger;

    public MeController(IUnitOfWork unitOfWork, ILogger<MeController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpPost("subscriptions/{podcastId}")]
    public async Task<ActionResult> Subscribe(int podcastId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var podcast = await _unitOfWork.Podcasts.GetByIdAsync(podcastId);
            if (podcast == null)
            {
                return NotFound(new { message = "Podcast not found" });
            }

            var existing = (await _unitOfWork.Subscriptions.FindAsync(s => 
                ((Subscription)s).UserId == userId.Value && 
                ((Subscription)s).PodcastId == podcastId)).FirstOrDefault();

            if (existing != null)
            {
                return Conflict(new { message = "Already subscribed" });
            }

            var subscription = new Subscription
            {
                UserId = userId.Value,
                PodcastId = podcastId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Subscriptions.AddAsync(subscription);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Subscribed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to podcast {PodcastId}", podcastId);
            return StatusCode(500, new { message = "An error occurred while subscribing" });
        }
    }

    [HttpDelete("subscriptions/{podcastId}")]
    public async Task<ActionResult> Unsubscribe(int podcastId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var subscription = (await _unitOfWork.Subscriptions.FindAsync(s => 
                ((Subscription)s).UserId == userId.Value && 
                ((Subscription)s).PodcastId == podcastId)).FirstOrDefault() as Subscription;

            if (subscription == null)
            {
                return NotFound(new { message = "Subscription not found" });
            }

            await _unitOfWork.Subscriptions.DeleteAsync(subscription);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Unsubscribed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from podcast {PodcastId}", podcastId);
            return StatusCode(500, new { message = "An error occurred while unsubscribing" });
        }
    }

    [HttpGet("subscriptions")]
    public async Task<ActionResult<List<PodcastDto>>> GetSubscriptions()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var subscriptions = await _unitOfWork.Subscriptions.FindAsync(s => 
                ((Subscription)s).UserId == userId.Value);
            
            var subscriptionList = subscriptions.Cast<Subscription>().ToList();
            var podcastIds = subscriptionList.Select(s => s.PodcastId).ToList();

            var podcasts = new List<PodcastDto>();
            foreach (var podcastId in podcastIds)
            {
                var podcast = await _unitOfWork.Podcasts.GetByIdAsync(podcastId) as Entities.Podcast;
                if (podcast != null)
                {
                    var episodes = await _unitOfWork.Episodes.FindAsync(e => ((Episode)e).PodcastId == podcastId);
                    podcasts.Add(new PodcastDto
                    {
                        Id = podcast.Id,
                        Title = podcast.Title,
                        Author = podcast.Author,
                        Description = podcast.Description,
                        ImageUrl = podcast.ImageUrl,
                        Language = podcast.Language,
                        Categories = podcast.Categories,
                        CreatedAt = podcast.CreatedAt,
                        EpisodeCount = episodes.Count()
                    });
                }
            }

            return Ok(podcasts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subscriptions");
            return StatusCode(500, new { message = "An error occurred while fetching subscriptions" });
        }
    }

    [HttpPut("progress/{episodeId}")]
    public async Task<ActionResult> UpdateProgress(int episodeId, [FromBody] UpdateProgressRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var episode = await _unitOfWork.Episodes.GetByIdAsync(episodeId);
            if (episode == null)
            {
                return NotFound(new { message = "Episode not found" });
            }

            var progress = (await _unitOfWork.PlayProgresses.FindAsync(p => 
                ((PlayProgress)p).UserId == userId.Value && 
                ((PlayProgress)p).EpisodeId == episodeId)).FirstOrDefault() as PlayProgress;

            if (progress == null)
            {
                progress = new PlayProgress
                {
                    UserId = userId.Value,
                    EpisodeId = episodeId,
                    PositionSeconds = request.PositionSeconds,
                    UpdatedAt = DateTime.UtcNow
                };
                await _unitOfWork.PlayProgresses.AddAsync(progress);
            }
            else
            {
                progress.PositionSeconds = request.PositionSeconds;
                progress.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.PlayProgresses.UpdateAsync(progress);
            }

            await _unitOfWork.SaveChangesAsync();
            return Ok(new { message = "Progress updated" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for episode {EpisodeId}", episodeId);
            return StatusCode(500, new { message = "An error occurred while updating progress" });
        }
    }

    [HttpGet("progress/{episodeId}")]
    public async Task<ActionResult<PlayProgressDto>> GetProgress(int episodeId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var progress = (await _unitOfWork.PlayProgresses.FindAsync(p => 
                ((PlayProgress)p).UserId == userId.Value && 
                ((PlayProgress)p).EpisodeId == episodeId)).FirstOrDefault() as PlayProgress;

            if (progress == null)
            {
                return Ok(new PlayProgressDto { PositionSeconds = 0 });
            }

            return Ok(new PlayProgressDto
            {
                PositionSeconds = progress.PositionSeconds,
                UpdatedAt = progress.UpdatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting progress for episode {EpisodeId}", episodeId);
            return StatusCode(500, new { message = "An error occurred while fetching progress" });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class UpdateProgressRequest
{
    public int PositionSeconds { get; set; }
}

public class PlayProgressDto
{
    public int PositionSeconds { get; set; }
    public DateTime UpdatedAt { get; set; }
}

