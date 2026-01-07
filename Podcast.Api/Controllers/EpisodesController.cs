using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Podcast.Core.DTOs;
using Podcast.Core.Entities;
using Podcast.Core.Interfaces;
using System.Security.Claims;

namespace Podcast.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EpisodesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EpisodesController> _logger;

    public EpisodesController(IUnitOfWork unitOfWork, ILogger<EpisodesController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EpisodeDto>> GetEpisode(int id)
    {
        try
        {
            var episode = await _unitOfWork.Episodes.GetByIdAsync(id) as Episode;
            if (episode == null)
            {
                return NotFound(new { message = "Episode not found" });
            }

            var userId = GetCurrentUserId();
            if (userId.HasValue)
            {
                var progress = (await _unitOfWork.PlayProgresses.FindAsync(p => 
                    ((PlayProgress)p).UserId == userId.Value && 
                    ((PlayProgress)p).EpisodeId == id)).FirstOrDefault() as PlayProgress;
                
                return Ok(new EpisodeDto
                {
                    Id = episode.Id,
                    PodcastId = episode.PodcastId,
                    Title = episode.Title,
                    Description = episode.Description,
                    AudioUrl = episode.AudioUrl,
                    DurationSeconds = episode.DurationSeconds,
                    PublishDate = episode.PublishDate,
                    EpisodeNumber = episode.EpisodeNumber,
                    SeasonNumber = episode.SeasonNumber,
                    ImageUrl = episode.ImageUrl,
                    CurrentPosition = progress?.PositionSeconds
                });
            }

            return Ok(new EpisodeDto
            {
                Id = episode.Id,
                PodcastId = episode.PodcastId,
                Title = episode.Title,
                Description = episode.Description,
                AudioUrl = episode.AudioUrl,
                DurationSeconds = episode.DurationSeconds,
                PublishDate = episode.PublishDate,
                EpisodeNumber = episode.EpisodeNumber,
                SeasonNumber = episode.SeasonNumber,
                ImageUrl = episode.ImageUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting episode {EpisodeId}", id);
            return StatusCode(500, new { message = "An error occurred while fetching the episode" });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

