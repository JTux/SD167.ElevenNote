using System.Security.Claims;
using ElevenNote.Data;
using ElevenNote.Data.Entities;
using ElevenNote.Models.Note;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ElevenNote.Services.Note;

public class NoteService : INoteService
{
    private readonly int _userId;
    private readonly ApplicationDbContext _dbContext;
    public NoteService(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
    {
        var userClaims = httpContextAccessor.HttpContext.User.Identity as ClaimsIdentity;
        var value = userClaims?.FindFirst("Id")?.Value;
        var validId = int.TryParse(value, out _userId);
        if (!validId)
            throw new Exception("Attempted to build NoteService without User Id claim.");

        _dbContext = dbContext;
    }

    public async Task<NoteListItem?> CreateNoteAsync(NoteCreate request)
    {
        var noteEntity = new NoteEntity
        {
            Title = request.Title,
            Content = request.Content,
            CreatedUtc = DateTimeOffset.Now,
            OwnerId = _userId
        };

        _dbContext.Notes.Add(noteEntity);
        var numberOfChanges = await _dbContext.SaveChangesAsync();

        if(numberOfChanges == 1)
        {
            NoteListItem response = new()
            {
                Id = noteEntity.Id,
                Title = noteEntity.Title,
                CreatedUtc = noteEntity.CreatedUtc
            };
            return response;
        }

        return null;
    }

    public async Task<IEnumerable<NoteListItem>> GetAllNotesAsync()
    {
        var notes = await _dbContext.Notes
            .Where(entity => entity.OwnerId == _userId)
            .Select(entity => new NoteListItem
            {
                Id = entity.Id,
                Title = entity.Title,
                CreatedUtc = entity.CreatedUtc
            })
            .ToListAsync();

        return notes;
    }

    public async Task<NoteDetail?> GetNoteByIdAsync(int noteId)
    {
        // Find the first note that has the given Id
        // and an OwnerId that matches the requesting _userId
        var noteEntity = await _dbContext.Notes
            .FirstOrDefaultAsync(e =>
                e.Id == noteId && e.OwnerId == _userId
            );

        // If noteEntity is null then return null
        // Otherwise initialize and return a new NoteDetail
        return noteEntity is null ? null : new NoteDetail
        {
            Id = noteEntity.Id,
            Title = noteEntity.Title,
            Content = noteEntity.Content,
            CreatedUtc = noteEntity.CreatedUtc,
            ModifiedUtc = noteEntity.ModifiedUtc
        };
    }
}