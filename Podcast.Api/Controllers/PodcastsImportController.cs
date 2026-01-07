using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Podcast.Core.Interfaces;

namespace Podcast.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PodcastsImportController : ControllerBase
{
    private readonly IRssIngestionService _rssIngestionService;
    private readonly ILogger<PodcastsImportController> _logger;

    public PodcastsImportController(IRssIngestionService rssIngestionService, ILogger<PodcastsImportController> logger)
    {
        _rssIngestionService = rssIngestionService;
        _logger = logger;
    }

    [HttpPost("import")]
    public async Task<ActionResult<ImportResponse>> ImportPodcast([FromBody] ImportRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RssUrl))
            {
                return BadRequest(new { message = "RSS URL is required" });
            }

            var podcastId = await _rssIngestionService.ImportPodcastFromRssAsync(request.RssUrl);
            return Ok(new ImportResponse
            {
                PodcastId = podcastId,
                Message = "Podcast imported successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing podcast from RSS {RssUrl}", request.RssUrl);
            return StatusCode(500, new { message = "An error occurred while importing podcast" });
        }
    }
}

public class ImportRequest
{
    public string RssUrl { get; set; } = string.Empty;
}

public class ImportResponse
{
    public int PodcastId { get; set; }
    public string Message { get; set; } = string.Empty;
}

