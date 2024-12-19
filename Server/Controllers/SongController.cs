using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using music_manager_starter.Data;
using music_manager_starter.Data.Models;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using music_manager_starter.Server.Services;
using System;

namespace music_manager_starter.Server.Controllers
{
   [Route("api/[controller]")]
   [ApiController]
   public class SongsController : ControllerBase
   {
       private readonly DataDbContext _context;
       private readonly ILogger<SongsController> _logger;
       private readonly IFileService _fileService;

       public SongsController(
           DataDbContext context, 
           ILogger<SongsController> logger,
           IFileService fileService)
       {
           _context = context ?? throw new ArgumentNullException(nameof(context));
           _logger = logger ?? throw new ArgumentNullException(nameof(logger));
           _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
       }

       [HttpGet]
       public async Task<ActionResult<IEnumerable<Song>>> GetSongs()
       {
           using (LogContext.PushProperty("Endpoint", "GetSongs"))
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

       [HttpPost("upload-art/{id}")]
       public async Task<IActionResult> UploadAlbumArt(Guid id, IFormFile file)
       {
           using (LogContext.PushProperty("Endpoint", "UploadAlbumArt"))
           {
               try
               {
                   var song = await _context.Songs.FindAsync(id);
                   if (song == null)
                   {
                       _logger.LogWarning("Attempted to upload art for non-existent song: {Id}", id);
                       return NotFound("Song not found");
                   }

                   if (file == null || file.Length == 0)
                   {
                       _logger.LogWarning("No file uploaded for song: {Id}", id);
                       return BadRequest("No file uploaded");
                   }

                   // Delete old album art if exists
                   if (!string.IsNullOrEmpty(song.AlbumArtUrl))
                   {
                       _fileService.DeleteFile(song.AlbumArtUrl);
                   }

                   // Save new file
                   string fileUrl = await _fileService.SaveFileAsync(file);
                   
                   // Update song
                   song.AlbumArtUrl = fileUrl;
                   await _context.SaveChangesAsync();

                   _logger.LogInformation("Album art uploaded for song {SongId}", id);
                   return Ok(new { albumArtUrl = fileUrl });
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Error uploading album art for song {SongId}", id);
                   return StatusCode(500, "Error uploading album art");
               }
           }
       }

       [HttpDelete("{id}")]
       public async Task<IActionResult> DeleteSong(Guid id)
       {
           using (LogContext.PushProperty("Endpoint", "DeleteSong"))
           {
               try
               {
                   var song = await _context.Songs.FindAsync(id);
                   if (song == null)
                   {
                       _logger.LogWarning("Attempted to delete non-existent song: {Id}", id);
                       return NotFound("Song not found");
                   }

                   // Delete album art if exists
                   if (!string.IsNullOrEmpty(song.AlbumArtUrl))
                   {
                       _fileService.DeleteFile(song.AlbumArtUrl);
                   }

                   _context.Songs.Remove(song);
                   await _context.SaveChangesAsync();

                   _logger.LogInformation("Successfully deleted song {SongId}", id);
                   return Ok();
               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "Error deleting song {SongId}", id);
                   return StatusCode(500, "Error deleting song");
               }
           }
       }
   }
}