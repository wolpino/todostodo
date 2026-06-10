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
    [ProducesResponseType<IEnumerable<Entry>>(StatusCodes.Status200OK)]
    public async Task<IEnumerable<Entry>> Get()
    {
        return await _db.Entries.ToListAsync();
    }

    [HttpGet("{id}")]
    [ProducesResponseType<Entry>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Entry>> Get(int id)
    {
        var entry = await _db.Entries.FindAsync(id);
        return entry is null
            ? NotFound()
            : Ok(entry);
    }

    [HttpPost]
    [ProducesResponseType<Entry>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Entry>> Create(CreateEntryRequest req)
    {
        var entry = new Entry
        {
            Title = req.Title,
            Description = req.Description,
            Status = req.Status,
            UserId = 0,
            User = null!
        };

        _db.Entries.Add(entry);

        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = entry.Id }, entry);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, UpdateEntryRequest req)
    {
        if (id != req.Id)
            return BadRequest();

        var entry = await _db.Entries.FindAsync(id);
        if (entry is null)
            return NotFound();

        if (!string.IsNullOrEmpty(req.Title) && entry.Title != req.Title)
            entry.Title = req.Title;

        if (req.Status.HasValue && entry.Status != req.Status.Value)
            entry.Status = req.Status.Value;

        await _db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
