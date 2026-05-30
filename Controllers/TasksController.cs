using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using todostodo.Data;
using todostodo.Models;
using TaskModel = todostodo.Models.Task;

namespace todostodo.Controllers;

/// <summary>
/// Controller for managing user tasks with full CRUD operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(
    ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    /// Retrieves all tasks for the current authenticated user, ordered by creation date (newest first).
    /// </summary>
    /// <returns>A list of tasks belonging to the user.</returns>
    /// <response code="200">Successfully retrieved tasks.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskModel>>> GetTasks()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var tasks = await context.Tasks
            .Where(t => t.ApplicationUserId == userId)
            .OrderByDescending(t => t.CreatedDate)
            .ToListAsync();

        return Ok(tasks);
    }

    /// <summary>
    /// Retrieves a specific task by ID for the current user.
    /// </summary>
    /// <param name="id">The ID of the task to retrieve.</param>
    /// <returns>The requested task.</returns>
    /// <response code="200">Task found.</response>
    /// <response code="404">Task not found or does not belong to user.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult<TaskModel>> GetTask(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var task = await context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationUserId == userId);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    /// <summary>
    /// Creates a new task for the current user.
    /// </summary>
    /// <param name="request">The task creation request containing description and optional due date/time.</param>
    /// <returns>The created task.</returns>
    /// <response code="201">Task created successfully.</response>
    /// <response code="400">Invalid request data.</response>
    [HttpPost]
    public async Task<ActionResult<TaskModel>> CreateTask([FromBody] CreateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var task = new TaskModel
        {
            Description = request.Description,
            DueDate = request.DueDate,
            DueTime = request.DueTime,
            Status = EntryStatus.Active,
            ApplicationUserId = userId.Value,
            CreatedDate = DateTime.UtcNow
        };

        context.Tasks.Add(task);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
    }

    /// <summary>
    /// Updates an existing task for the current user.
    /// </summary>
    /// <param name="id">The ID of the task to update.</param>
    /// <param name="request">The update request with optional fields.</param>
    /// <returns>The updated task.</returns>
    /// <response code="200">Task updated successfully.</response>
    /// <response code="404">Task not found or does not belong to user.</response>
    [HttpPut("{id}")]
    public async Task<ActionResult<TaskModel>> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var task = await context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationUserId == userId);

        if (task == null)
            return NotFound();

        if (!string.IsNullOrEmpty(request.Description))
            task.Description = request.Description;

        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate.Value;

        if (request.DueTime.HasValue)
            task.DueTime = request.DueTime.Value;

        if (request.Status.HasValue)
            task.Status = request.Status.Value;

        task.ModifiedDate = DateTime.UtcNow;

        context.Tasks.Update(task);
        await context.SaveChangesAsync();

        return Ok(task);
    }

    /// <summary>
    /// Deletes a task for the current user.
    /// </summary>
    /// <param name="id">The ID of the task to delete.</param>
    /// <response code="204">Task deleted successfully.</response>
    /// <response code="404">Task not found or does not belong to user.</response>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized();

        var task = await context.Tasks
            .FirstOrDefaultAsync(t => t.Id == id && t.ApplicationUserId == userId);

        if (task == null)
            return NotFound();

        context.Tasks.Remove(task);
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
/// Request model for creating a new task.
/// </summary>
public class CreateTaskRequest
{
    /// <summary>
    /// The task description.
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The optional due date for the task.
    /// </summary>
    public DateOnly? DueDate { get; set; }

    /// <summary>
    /// The optional due time for the task.
    /// </summary>
    public TimeOnly? DueTime { get; set; }
}

/// <summary>
/// Request model for updating an existing task.
/// </summary>
public class UpdateTaskRequest
{
    /// <summary>
    /// The updated task description (optional).
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
    /// The updated task status (optional).
    /// </summary>
    public EntryStatus? Status { get; set; }
}
