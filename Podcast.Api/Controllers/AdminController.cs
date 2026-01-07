using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Podcast.Core.DTOs;
using Podcast.Core.Entities;
using Podcast.Core.Interfaces;
using Entities = Podcast.Core.Entities;

namespace Podcast.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize] // TODO: Add admin role check
public class AdminController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRssIngestionService _rssIngestionService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IUnitOfWork unitOfWork,
        IRssIngestionService rssIngestionService,
        ILogger<AdminController> logger)
    {
        _unitOfWork = unitOfWork;
        _rssIngestionService = rssIngestionService;
        _logger = logger;
    }

    [HttpPost("podcasts")]
    public async Task<ActionResult<PodcastDto>> AddPodcast([FromBody] AddPodcastRequest request)
    {
        try
        {
            var podcast = new Entities.Podcast
            {
                Title = request.Title,
                Author = request.Author ?? string.Empty,
                Description = request.Description ?? string.Empty,
                ImageUrl = request.ImageUrl,
                Language = request.Language,
                Categories = request.Categories,
                RssUrl = request.RssUrl,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Podcasts.AddAsync(podcast);
            await _unitOfWork.SaveChangesAsync();

            if (!string.IsNullOrEmpty(request.RssUrl))
            {
                await _rssIngestionService.SyncPodcastAsync(podcast.Id);
            }

            return Ok(new
            {
                Id = podcast.Id,
                Title = podcast.Title,
                Author = podcast.Author,
                Description = podcast.Description,
                ImageUrl = podcast.ImageUrl,
                Language = podcast.Language,
                Categories = podcast.Categories,
                RssUrl = podcast.RssUrl,
                CreatedAt = podcast.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding podcast");
            return StatusCode(500, new { message = "An error occurred while adding podcast" });
        }
    }

    [HttpPost("podcasts/{id}/sync")]
    public async Task<ActionResult> SyncPodcast(int id)
    {
        try
        {
            var result = await _rssIngestionService.SyncPodcastAsync(id);
            if (!result)
            {
                return NotFound(new { message = "Podcast not found or RSS URL not configured" });
            }

            return Ok(new { message = "Podcast synced successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing podcast {PodcastId}", id);
            return StatusCode(500, new { message = "An error occurred while syncing podcast" });
        }
    }
}

public class AddPodcastRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public string? Language { get; set; }
    public string? Categories { get; set; }
    public string? RssUrl { get; set; }
}


