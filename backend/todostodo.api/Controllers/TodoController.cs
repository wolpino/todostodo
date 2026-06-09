using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todostodo.api.Data;
using todostodo.api.Models;

namespace todostodo.api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _db;

    public TodoController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<Entry>>> GetEntries()
    {
        var entries = await _db.Entries.ToListAsync();

        return Ok(entries);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Entry>> GetEntry(int id)
    {
        var entry = await _db.Entries.FindAsync(id);
        return entry is null
            ? NotFound()
            : Ok(entry);
    }

}