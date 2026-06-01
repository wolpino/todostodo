using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todostodo.Data;
using todostodo.Models;
using ToDo = todostodo.Models.ToDo;

namespace todostodo.Controllers;

/// <summary>
/// Controller for managing user todos with full CRUD operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ToDosController(
    ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    /// Retrieves all todos for the current authenticated user, ordered by creation date (newest first).
    /// </summary>
    /// <returns>A list of todos belonging to the user.</returns>
    /// <response code="200">Successfully retrieved todos.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ToDo>>> GetToDos()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        // TODO change to todo in ocntext
        var todos = await context.ToDos
            .Where(t => t.ApplicationUserId == userId)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();

        return Ok(todos);
    }

    /// <summary>
    /// Retrieves a specific todo by ID for the current user.
    /// </summary>
    /// <param name="id">The ID of the todo to retrieve.</param>
    /// <returns>The requested todo.</returns>
    /// <response code="200">Todo found.</response>
    /// <response code="404">Todo not found or does not belong to user.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<ToDo>> GetTodo(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var todo = await context.ToDos
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationUserId == userId);

        if (todo == null)
            return NotFound();

        return Ok(todo);
    }

    /// <summary>
    /// Creates a new todo for the current user.
    /// </summary>
    /// <param name="request">The todo creation request containing description and optional due date/time.</param>
    /// <returns>The created todo.</returns>
    /// <response code="201">Todo created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    public async Task<ActionResult<ToDo>> CreateTodo([FromBody] CreateTodoRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var todo = new ToDo
        {
            Description = request.Description,
            DueDate = request.DueDate,
            DueTime = request.DueTime,
            Status = EntryStatus.Active,
            ApplicationUserId = userId.Value,
            CreatedDate = DateTime.UtcNow
        };

        context.ToDos.Add(todo);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTodo), new { id = todo.Id }, todo);
    }

    /// <summary>
    /// Updates an existing todo for the current user.
    /// </summary>
    /// <param name="id">The ID of the todo to update.</param>
    /// <param name="request">The update request with optional fields.</param>
    /// <returns>The updated todo.</returns>
    /// <response code="200">Todo updated successfully.</response>
    /// <response code="404">Todo not found or does not belong to user.</response>
    [HttpPut("{id}")]
    public async Task<ActionResult<ToDo>> UpdateTodo(int id, [FromBody] UpdateTodoRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var todo = await context.ToDos
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationUserId == userId);

        if (todo == null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Description))
            todo.Description = request.Description;

        if (request.DueDate.HasValue)
            todo.DueDate = request.DueDate.Value;

        if (request.DueTime.HasValue)
            todo.DueTime = request.DueTime.Value;

        if (request.Status.HasValue)
            todo.Status = request.Status.Value;

        todo.ModifiedDate = DateTime.UtcNow;

        context.ToDos.Update(todo);
        await context.SaveChangesAsync();

        return Ok(todo);
    }

    /// <summary>
    /// Deletes a todo for the current user.
    /// </summary>
    /// <param name="id">The ID of the todo to delete.</param>
    /// <response code="204">Todo deleted successfully.</response>
    /// <response code="404">Todo not found or does not belong to user.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTodo(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var todo = await context.ToDos
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationUserId == userId);

        if (todo == null)
            return NotFound();

        context.ToDos.Remove(todo);
        await context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Retrieves the currently authenticated user's ID from the claims principal.
    /// </summary>
    /// <returns>The user's ID or null if not authenticated.</returns>
    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim != null && int.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}

/// <summary>
/// Request model for creating a new todo.
/// </summary>
public class CreateTodoRequest
{
    /// <summary>
    /// The todo description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The optional due date for the todo.
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// The optional due time for the todo.
    /// </summary>
    public TimeOnly? DueTime { get; set; }
}

/// <summary>
/// Request model for updating an existing todo.
/// </summary>
public class UpdateTodoRequest
{
    /// <summary>
    /// The updated todo description (optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The updated due date (optional).
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// The updated due time (optional).
    /// </summary>
    public TimeOnly? DueTime { get; set; }

    /// <summary>
    /// The updated todo status (optional).
    /// </summary>
    public EntryStatus? Status { get; set; }
}
