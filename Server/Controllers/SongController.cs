using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using System;
using Microsoft.Extensions.Logging;


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
            try
            {
                var songs = await _context.Songs.ToListAsync();
                _logger.LogInformation("{Count} songs retrieved", songs.Count);
                return Ok(songs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during song retrieval");
                return StatusCode(500, "An error occurred during song retrieval. Please try again.");
            }
        }


        [HttpPost]
        public async Task<ActionResult<Song>> PostSong(Song song)
        {
            if (song == null)
            {
                _logger.LogWarning("Could not post null song");
                return BadRequest("The posted song cannot be null.");
            }


            // Add additional validation as needed
            if (string.IsNullOrWhiteSpace(song.Title))
            {
                return BadRequest("Please provide a song title.");
            }


            try
            {
                _context.Songs.Add(song);
                await _context.SaveChangesAsync();
               
                _logger.LogInformation("Added song with ID {SongId}", song.Id);
                return CreatedAtAction(nameof(GetSongs), new { id = song.Id }, song);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error occurred during song retrieval of song {Title}", song.Title);
                return StatusCode(500, "AError occurred during song retrieval. Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred during processing of song {Title}", song.Title);
                return StatusCode(500, "Enexpected error occurred. Please try again.");
            }
        }
    }
}
