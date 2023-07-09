using ElevenNote.Models.Note;
using ElevenNote.Services.Note;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ElevenNote.WebMvc.Controllers;

[Authorize]
public class NoteController : Controller
{
    private readonly INoteService _noteService;

    public NoteController(INoteService noteService)
    {
        _noteService = noteService;
    }

    [Route("Notes")]
    public async Task<IActionResult> Index()
    {
        var notes = await _noteService.GetAllNotesAsync();
        return View(notes);
    }

    // GET: Note/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Note/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(NoteCreate model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (await _noteService.CreateNoteAsync(model) is not null)
        {
            TempData["SaveResult"] = "Your note was created.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Note could not be created");
        return View(model);
    }

    // GET: Note/Details/{id}
    public async Task<IActionResult> Details(int id)
    {
        var model = await _noteService.GetNoteByIdAsync(id);
        return View(model);
    }

    // GET: Note/Edit/{id}
    public async Task<IActionResult> Edit(int id)
    {

        var detail = await _noteService.GetNoteByIdAsync(id);

        if (detail is null)
            return RedirectToAction(nameof(Index));

        var model =
            new NoteUpdate
            {
                Id = detail.Id,
                Title = detail.Title,
                Content = detail.Content
            };

        return View(model);
    }

    // POST: Note/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, NoteUpdate model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Id != id)
        {
            ModelState.AddModelError("", "Id Mismatch");
            return View(model);
        }

        if (await _noteService.UpdateNoteAsync(model))
        {
            TempData["SaveResult"] = "Your note was updated.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("", "Your note could not be updated.");
        return View();
    }

    // GET: Note/Delete/{id}
    [ActionName("Delete")]
    public async Task<IActionResult> Delete(int id)
    {
        var model = await _noteService.GetNoteByIdAsync(id);

        return View(model);
    }

    // POST: Note/Delete/{id}
    [HttpPost]
    [ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeletePost(int id)
    {
        await _noteService.DeleteNoteAsync(id);

        TempData["SaveResult"] = "Your note was deleted.";
        return RedirectToAction("Index");
    }
}