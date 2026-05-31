namespace todostodo.Models;

/// <summary>
/// Represents a task entry with optional due date and time.
/// </summary>
public class Task : Entry
{
    /// <summary>Gets or sets the optional due date for the task.</summary>
    public DateOnly? DueDate { get; set; }
    
    /// <summary>Gets or sets the optional due time for the task.</summary>
    public TimeOnly? DueTime { get; set; }
}
