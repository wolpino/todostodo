namespace todostodo.Models;

public class ToDo : Entry
{
    public DateOnly? DueDate { get; set; }
    
    public TimeOnly? DueTime { get; set; }
}
