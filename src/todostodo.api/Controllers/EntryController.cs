using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todostodo.api.Data;
using todostodo.api.Models;

namespace todostodo.api.Controllers;

/// <summary>
/// All entry endpoints require authentication. Placing [Authorize] at the controller
/// level rather than on individual methods ensures a new endpoint can never
/// accidentally be added without auth — the secure default is opt-out, not opt-in.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EntryController(AppDbContext db, ILogger<EntryController> logger) : ControllerBase
{
    // Extracted once per action rather than inline to keep action bodies readable.
    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    [ProducesResponseType<IEnumerable<Entry>>(StatusCodes.Status200OK)]
    public async Task<IEnumerable<Entry>> Get()
    {
        // ??"" is safe: a missing claim returns an empty list (no entries have UserId == "")
        // rather than a 401 inside the action — the [Authorize] attribute already
        // handles the unauthenticated case before this code runs.
        var userId = CurrentUserId ?? string.Empty;
        return await db.Entries
            .Where(e => e.UserId == userId)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    [ProducesResponseType<Entry>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Entry>> Get(int id)
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        // Combining the id and userId filters in a single query is both efficient
        // and intentionally returns 404 (not 403) when the entry belongs to another
        // user — returning 404 avoids confirming that the entry exists at all.
        var entry = await db.Entries
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (entry is null)
        {
            // Could be a missing entry or another user's entry — logged at Debug
            // because a user requesting a recently-deleted entry is normal and
            // would make this noisy at Warning in production.
            logger.LogDebug("Entry {EntryId} not found for user {UserId}", id, userId);
            return NotFound();
        }

        return Ok(entry);
    }

    [HttpPost]
    [ProducesResponseType<Entry>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Entry>> Create(CreateEntryRequest req)
    {
        var userId = CurrentUserId;
        if (userId is null)
            return Unauthorized();

        var entry = new Entry
        {
            Title = req.Title,
            Description = req.Description,
            Status = req.Status,
            UserId = userId
        };

        db.Entries.Add(entry);
        await db.SaveChangesAsync();

        // Information-level: creates are the primary write event and form the
        // audit trail if we ever need to reconstruct what a user did.
        logger.LogInformation("Entry {EntryId} created by user {UserId}", entry.Id, userId);

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

        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        // Same single-query ownership pattern as GET /{id}: 404 for both
        // "not found" and "found but owned by someone else".
        var entry = await db.Entries
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (entry is null)
        {
            // Warning-level: a PUT to an entry that doesn't exist or belongs to
            // someone else is more suspicious than a stale GET — worth surfacing
            // in log monitoring as a possible probing attempt.
            logger.LogWarning("Update rejected — entry {EntryId} not found or not owned by user {UserId}", id, userId);
            return NotFound();
        }

        if (!string.IsNullOrEmpty(req.Title) && entry.Title != req.Title)
            entry.Title = req.Title;

        if (req.Status.HasValue && entry.Status != req.Status.Value)
            entry.Status = req.Status.Value;

        await db.SaveChangesAsync();

        logger.LogInformation("Entry {EntryId} updated by user {UserId}", id, userId);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        var entry = await db.Entries
            .FirstOrDefaultAsync(e => e.Id == id && e.UserId == userId);

        if (entry is null)
        {
            // Warning-level for the same reason as Update: a DELETE to an unknown
            // or unowned entry is worth monitoring.
            logger.LogWarning("Delete rejected — entry {EntryId} not found or not owned by user {UserId}", id, userId);
            return NotFound();
        }

        db.Entries.Remove(entry);
        await db.SaveChangesAsync();

        // Deletions are always logged at Information — they're irreversible and
        // the most useful data point for "what happened to my entry?" support questions.
        logger.LogInformation("Entry {EntryId} deleted by user {UserId}", id, userId);

        return NoContent();
    }
}

