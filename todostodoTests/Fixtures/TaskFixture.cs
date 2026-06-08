using todostodo.Controllers;
using todostodo.Models;

namespace todostodo.Tests.Fixtures;

/// <summary>
/// Fixture for creating test ToDo objects and request models.
/// Provides consistent test data for ToDo-related tests.
/// </summary>
public class ToDoFixture
{
    /// <summary>
    /// Creates a test ToDo with default values.
    /// </summary>
    public static Todo CreateTestToDo(
        int? id = null,
        int? userId = null,
        string? description = null,
        DateOnly? dueDate = null,
        TimeOnly? dueTime = null,
        EntryStatus status = EntryStatus.Active)
    {
        return new Todo
        {
            Id = id ?? 1,
            Description = description ?? "Test ToDo",
            DueDate = dueDate,
            DueTime = dueTime,
            Status = status,
            ApplicationUserId = userId ?? 1,
            CreatedDate = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a CreateToDoRequest for testing.
    /// </summary>
    public static CreateTodoRequest CreateToDoRequest(
        string? description = null,
        DateOnly? dueDate = null,
        TimeOnly? dueTime = null)
    {
        return new CreateTodoRequest
        {
            Description = description ?? "New Test ToDo",
            DueDate = dueDate,
            DueTime = dueTime
        };
    }

    /// <summary>
    /// Creates an UpdateTodoRequest for testing.
    /// </summary>
    public static UpdateTodoRequest CreateUpdateToDoRequest(
        string? description = null,
        DateOnly? dueDate = null,
        TimeOnly? dueTime = null,
        EntryStatus? status = null)
    {
        return new UpdateTodoRequest
        {
            Description = description,
            DueDate = dueDate,
            DueTime = dueTime,
            Status = status
        };
    }

    /// <summary>
    /// Creates multiple test ToDos.
    /// </summary>
    public static List<Todo> CreateTestToDos(int count = 3, int userId = 1)
    {
        return Enumerable.Range(1, count)
            .Select(i => CreateTestToDo(
                id: i,
                userId: userId,
                description: $"Test ToDo {i}"))
            .ToList();
    }
}
