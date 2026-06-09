using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todostodo.api.Data;
using todostodo.api.Models;

namespace todostodo.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EntryController : ControllerBase
{
    private readonly AppDbContext _db;

    public EntryController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IEnumerable<Entry>> Get()
    {
        return await _db.Entries.ToListAsync();

    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Entry>> Get(int id)
    {
        var entry = await _db.Entries.FindAsync(id);
        return entry is null
            ? NotFound()
            : Ok(entry);
    }

    [HttpPost]
    public async Task<ActionResult<Entry>> Create(Entry entry)
    {
        _db.Entries.Add(entry);

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<Entry>> Update(int id, Entry entryChanges)
    {
        if (id != entryChanges.Id)
        {
            return BadRequest();
        }

        var currentEntry = await _db.Entries.FindAsync(id);
        if (currentEntry is null)
        {
            return NotFound();
        }

        // check for changed fields, update only those that have changed
        // if the title is not empty and has changed, update it
        if (!String.IsNullOrEmpty(entryChanges.Title) && currentEntry.Title != entryChanges.Title)
        {
            currentEntry.Title = entryChanges.Title;
        }
        // if the status has changed, update it
        if (currentEntry.Status != entryChanges.Status)
        {
            currentEntry.Status = entryChanges.Status;
        }

        await _db.SaveChangesAsync();
        
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entry = await _db.Entries.FindAsync(id);
        if (entry is null)        
            return NotFound();

        _db.Entries.Remove(entry);

        await _db.SaveChangesAsync();
        
        return NoContent();    
    }


}

