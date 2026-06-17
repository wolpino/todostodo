using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todostodo.api.Data;
using todostodo.api.Models;

namespace todostodo.api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController(AppDbContext db, ILogger<SettingsController> logger) : ControllerBase
{
    public const string DefaultFont = "comic-shanns";

    private static readonly HashSet<string> AllowedFonts =
        [DefaultFont, "courier-prime", "patrick-hand", "caveat"];

    private string? CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

    [HttpGet]
    [ProducesResponseType<Settings>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Settings>> Get()
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        var settings = await db.Settings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings is null)
        {
            settings = new Settings { Font = DefaultFont, UserId = userId };
            db.Settings.Add(settings);
            await db.SaveChangesAsync();
            logger.LogInformation("Default settings created for user {UserId}", userId);
        }

        return Ok(settings);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Put(UpdateSettingsRequest req)
    {
        var userId = CurrentUserId;
        if (userId is null) return Unauthorized();

        if (!AllowedFonts.Contains(req.Font))
            return BadRequest();

        var settings = await db.Settings.FirstOrDefaultAsync(s => s.UserId == userId);
        if (settings is null)
        {
            settings = new Settings { Font = req.Font, UserId = userId };
            db.Settings.Add(settings);
        }
        else
        {
            settings.Font = req.Font;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Settings updated for user {UserId}", userId);

        return NoContent();
    }
}
