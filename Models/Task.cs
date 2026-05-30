namespace todostodo.Models;

public class Task : Entry
{
    public DateOnly? DueDate { get; set; }
    
    public TimeOnly? DueTime { get; set; }
}
