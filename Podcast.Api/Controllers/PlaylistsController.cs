using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Podcast.Core.Entities;
using Podcast.Core.Interfaces;
using System.Security.Claims;

namespace Podcast.Api.Controllers;

[ApiController]
[Route("api/me/playlists")]
[Authorize]
public class PlaylistsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PlaylistsController> _logger;

    public PlaylistsController(IUnitOfWork unitOfWork, ILogger<PlaylistsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PlaylistDto>> CreatePlaylist([FromBody] CreatePlaylistRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var playlist = new Playlist
            {
                UserId = userId.Value,
                Name = request.Name,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Playlists.AddAsync(playlist);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new PlaylistDto
            {
                Id = playlist.Id,
                Name = playlist.Name,
                CreatedAt = playlist.CreatedAt
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating playlist");
            return StatusCode(500, new { message = "An error occurred while creating playlist" });
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<PlaylistDto>>> GetPlaylists()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var playlists = await _unitOfWork.Playlists.FindAsync(p => ((Playlist)p).UserId == userId.Value);
            var playlistList = playlists.Cast<Playlist>()
                .Select(p => new PlaylistDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CreatedAt = p.CreatedAt
                })
                .ToList();

            return Ok(playlistList);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting playlists");
            return StatusCode(500, new { message = "An error occurred while fetching playlists" });
        }
    }

    [HttpPost("{playlistId}/items")]
    public async Task<ActionResult> AddEpisode(int playlistId, [FromBody] AddEpisodeRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var playlist = await _unitOfWork.Playlists.GetByIdAsync(playlistId) as Playlist;
            if (playlist == null || playlist.UserId != userId.Value)
            {
                return NotFound(new { message = "Playlist not found" });
            }

            var episode = await _unitOfWork.Episodes.GetByIdAsync(request.EpisodeId);
            if (episode == null)
            {
                return NotFound(new { message = "Episode not found" });
            }

            var existingItems = await _unitOfWork.PlaylistItems.FindAsync(i => 
                ((PlaylistItem)i).PlaylistId == playlistId);
            var maxOrder = existingItems.Cast<PlaylistItem>().Any() 
                ? existingItems.Cast<PlaylistItem>().Max(i => i.Order) 
                : 0;

            var item = new PlaylistItem
            {
                PlaylistId = playlistId,
                EpisodeId = request.EpisodeId,
                Order = maxOrder + 1,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.PlaylistItems.AddAsync(item);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Episode added to playlist" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding episode to playlist");
            return StatusCode(500, new { message = "An error occurred while adding episode" });
        }
    }

    [HttpDelete("{playlistId}/items/{episodeId}")]
    public async Task<ActionResult> RemoveEpisode(int playlistId, int episodeId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized();
            }

            var playlist = await _unitOfWork.Playlists.GetByIdAsync(playlistId) as Playlist;
            if (playlist == null || playlist.UserId != userId.Value)
            {
                return NotFound(new { message = "Playlist not found" });
            }

            var item = (await _unitOfWork.PlaylistItems.FindAsync(i => 
                ((PlaylistItem)i).PlaylistId == playlistId && 
                ((PlaylistItem)i).EpisodeId == episodeId)).FirstOrDefault() as PlaylistItem;

            if (item == null)
            {
                return NotFound(new { message = "Episode not found in playlist" });
            }

            await _unitOfWork.PlaylistItems.DeleteAsync(item);
            await _unitOfWork.SaveChangesAsync();

            return Ok(new { message = "Episode removed from playlist" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing episode from playlist");
            return StatusCode(500, new { message = "An error occurred while removing episode" });
        }
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}

public class CreatePlaylistRequest
{
    public string Name { get; set; } = string.Empty;
}

public class PlaylistDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class AddEpisodeRequest
{
    public int EpisodeId { get; set; }
}

