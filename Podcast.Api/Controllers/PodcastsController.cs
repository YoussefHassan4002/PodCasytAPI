using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Podcast.Core.DTOs;
using Podcast.Core.Entities;
using Podcast.Core.Interfaces;
using Entities = Podcast.Core.Entities;

namespace Podcast.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PodcastsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PodcastsController> _logger;

    public PodcastsController(IUnitOfWork unitOfWork, ILogger<PodcastsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PodcastDto>>> GetPodcasts(
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var podcasts = await _unitOfWork.Podcasts.GetAllAsync();
            var podcastList = podcasts.Cast<Entities.Podcast>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                podcastList = podcastList.Where(p => 
                    p.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Author.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                podcastList = podcastList.Where(p => 
                    p.Categories != null && p.Categories.Contains(category, StringComparison.OrdinalIgnoreCase));
            }

            var totalCount = podcastList.Count();
            var pagedPodcasts = podcastList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var items = new List<PodcastDto>();
            foreach (var p in pagedPodcasts)
            {
                var episodes = await _unitOfWork.Episodes.FindAsync(e => ((Entities.Episode)e).PodcastId == p.Id);
                items.Add(new PodcastDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Author = p.Author,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    Language = p.Language,
                    Categories = p.Categories,
                    CreatedAt = p.CreatedAt,
                    EpisodeCount = episodes.Count()
                });
            }

            return Ok(new PagedResult<PodcastDto>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcasts");
            return StatusCode(500, new { message = "An error occurred while fetching podcasts" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PodcastDto>> GetPodcast(int id)
    {
        try
        {
            var podcast = await _unitOfWork.Podcasts.GetByIdAsync(id) as Entities.Podcast;
            if (podcast == null)
            {
                return NotFound(new { message = "Podcast not found" });
            }

            var episodes = await _unitOfWork.Episodes.FindAsync(e => ((Episode)e).PodcastId == id);
            var episodeList = episodes.Cast<Episode>().ToList();

            return Ok(new PodcastDto
            {
                Id = podcast.Id,
                Title = podcast.Title,
                Author = podcast.Author,
                Description = podcast.Description,
                ImageUrl = podcast.ImageUrl,
                Language = podcast.Language,
                Categories = podcast.Categories,
                CreatedAt = podcast.CreatedAt,
                EpisodeCount = episodeList.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting podcast {PodcastId}", id);
            return StatusCode(500, new { message = "An error occurred while fetching the podcast" });
        }
    }

    [HttpGet("{id}/episodes")]
    public async Task<ActionResult<List<EpisodeDto>>> GetEpisodes(int id)
    {
        try
        {
            var podcast = await _unitOfWork.Podcasts.GetByIdAsync(id);
            if (podcast == null)
            {
                return NotFound(new { message = "Podcast not found" });
            }

            var episodes = await _unitOfWork.Episodes.FindAsync(e => ((Episode)e).PodcastId == id);
            var episodeList = episodes.Cast<Episode>()
                .OrderByDescending(e => e.PublishDate ?? e.CreatedAt)
                .Select(e => new EpisodeDto
                {
                    Id = e.Id,
                    PodcastId = e.PodcastId,
                    Title = e.Title,
                    Description = e.Description,
                    AudioUrl = e.AudioUrl,
                    DurationSeconds = e.DurationSeconds,
                    PublishDate = e.PublishDate,
                    EpisodeNumber = e.EpisodeNumber,
                    SeasonNumber = e.SeasonNumber,
                    ImageUrl = e.ImageUrl
                })
                .ToList();

            return Ok(episodeList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting episodes for podcast {PodcastId}", id);
            return StatusCode(500, new { message = "An error occurred while fetching episodes" });
        }
    }
}

