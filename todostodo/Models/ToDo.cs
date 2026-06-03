namespace todostodo.Models;

public class Todo : Entry
{
    public DateOnly? DueDate { get; set; }
    
    public TimeOnly? DueTime { get; set; }
}
