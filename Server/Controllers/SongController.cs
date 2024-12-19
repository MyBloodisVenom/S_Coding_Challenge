using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using Microsoft.Extensions.Logging; // Add this for ILogger
using Serilog.Context; // Add this for LogContext
using System;

namespace music_manager_starter.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SongsController : ControllerBase
    {
        private readonly DataDbContext _context;
        private readonly ILogger<SongsController> _logger;

        public SongsController(DataDbContext context, ILogger<SongsController> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
        {
            using (LogContext.PushProperty("Endpoint", "GetSongs")) // Add correlation ID
            {
                try
                {
                    _logger.LogInformation("Retrieving all songs");
                    var songs = await _context.Songs.ToListAsync();
                    _logger.LogInformation("Successfully retrieved {Count} songs", songs.Count);
                    return Ok(songs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to retrieve songs");
                    return StatusCode(500, "An error occurred during song retrieval. Please try again.");
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult<Song>> PostSong(Song song)
        {
            using (LogContext.PushProperty("Endpoint", "PostSong"))
            {
                if (song == null)
                {
                    _logger.LogWarning("Attempted to post null song");
                    return BadRequest("The posted song cannot be null.");
                }

                if (string.IsNullOrWhiteSpace(song.Title))
                {
                    _logger.LogWarning("Attempted to post song with empty title");
                    return BadRequest("Please provide a song title.");
                }

                try
                {
                    _logger.LogInformation("Adding new song: {@Song}", song);
                    _context.Songs.Add(song);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("Successfully added song: {Title} with ID {SongId}", song.Title, song.Id);
                    return CreatedAtAction(nameof(GetSongs), new { id = song.Id }, song);
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex, "Database error occurred while adding song: {@Song}", song);
                    return StatusCode(500, "Error occurred while saving the song. Please try again.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error occurred while processing song: {@Song}", song);
                    return StatusCode(500, "Unexpected error occurred. Please try again.");
                }
            }
        }
    }
}