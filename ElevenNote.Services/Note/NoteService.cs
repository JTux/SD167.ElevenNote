using ElevenNote.Data;
using ElevenNote.Data.Entities;
using ElevenNote.Models.Note;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ElevenNote.Services.Note;

public class NoteService : INoteService
{
    private readonly int _userId;
    private readonly ApplicationDbContext _dbContext;
    public NoteService(
        ApplicationDbContext dbContext,
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager)
    {
        var userIdClaim = userManager.GetUserId(signInManager.Context.User);
        var validId = int.TryParse(userIdClaim, out _userId);
        if (!validId)
            throw new Exception("Attempted to build NoteService without User Id claim.");

        _dbContext = dbContext;
    }

    public async Task<NoteListItem?> CreateNoteAsync(NoteCreate request)
    {
        // Create a new note entity based on the content from the request model
        // Also set the CreatedUtc based on the current time, and the OwnerId by the _userId field
        NoteEntity noteEntity = new()
        {
            Title = request.Title,
            Content = request.Content,
            CreatedUtc = DateTimeOffset.Now,
            OwnerId = _userId
        };
        
        // Add the new entity to our Notes DbSet, prepping it to be saved to the database
        _dbContext.Notes.Add(noteEntity);

        // Attempt to close out the transaction, saving the number of rows modified in the database
        var numberOfChanges = await _dbContext.SaveChangesAsync();

        // If the number of changes is equal to 1, then we know the note was saved
        if (numberOfChanges == 1)
        {
            // After saved, create our response model with the entity information
            // This includes the new Id, assigned when the entity was saved
            NoteListItem response = new()
            {
                Id = noteEntity.Id,
                Title = noteEntity.Title,
                CreatedUtc = noteEntity.CreatedUtc
            };
            // Return the response model
            return response;
        }

        // If the number of changes saved is not equal to 1, return null instead of an object
        return null;
    }

    public async Task<IEnumerable<NoteListItem>> GetAllNotesAsync()
    {
        // Query the _dbContext and Notes DbSet
        // The .Where filters all Notes that match the current _userId value
        // Using Select, convert our Entities to our NoteListItem model type
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

    public async Task<bool> UpdateNoteAsync(NoteUpdate request)
    {
        // Find the note and validate it's owned by the user
        var noteEntity = await _dbContext.Notes.FindAsync(request.Id);

        // By using the null conditioanl operator we can check if it's null
        // And at the same time we check the OwnerId vs the _userId
        if (noteEntity?.OwnerId != _userId)
            return false;

        // Now we update the entity's properties
        noteEntity.Title = request.Title;
        noteEntity.Content = request.Content;
        noteEntity.ModifiedUtc = DateTimeOffset.Now;

        // Save teh changes to the database and capture how many rows were updated
        var numberOfChanges = await _dbContext.SaveChangesAsync();

        // numberOfChanges is stated to be equal to 1 because only one row is updated
        return numberOfChanges == 1;
    }

    public async Task<bool> DeleteNoteAsync(int noteId)
    {
        // Find the note by the given Id
        var noteEntity = await _dbContext.Notes.FindAsync(noteId);

        // Validate the note exists and is owned by the user
        if (noteEntity?.OwnerId != _userId)
            return false;

        // Remove the note from the DbContext and assert that the one change was saved
        _dbContext.Notes.Remove(noteEntity);
        return await _dbContext.SaveChangesAsync() == 1;
    }
}
