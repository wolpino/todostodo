using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using todostodo.Controllers;
using todostodo.Data;
using todostodo.Models;
using todostodo.Tests.Fixtures;

namespace todostodo.Tests.Controllers;

/// <summary>
/// Happy path tests for the ToDosController.
/// Tests successful CRUD operations and authorization scenarios.
/// </summary>
public class ToDosControllerHappyPathTests : IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly ToDosController _controller;
    private readonly ApplicationDbContext _context;
    private ApplicationUser _testUser = null!;

    public ToDosControllerHappyPathTests()
    {
        _databaseFixture = new DatabaseFixture();
        _context = _databaseFixture.DbContext;
        _controller = new ToDosController(_context);
    }

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();

        _testUser = MockUserFixture.CreateTestUser(userId: 1);
        _context.Users.Add(_testUser);
        await _context.SaveChangesAsync();

        ControllerTestFixture.SetControllerUser(_controller, _testUser.Id);
    }

    public async Task DisposeAsync()
    {
        await _databaseFixture.DisposeAsync();
    }

    /// <summary>
    /// Test: Get all ToDos returns empty list when no ToDos exist.
    /// </summary>
    [Fact]
    public async Task GetToDos_WithNoToDos_ReturnsEmptyList()
    {
        var result = await _controller.GetToDos();

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var todos = okResult.Value as IEnumerable<Todo>;
        todos.Should().NotBeNull();
        todos!.Should().BeEmpty();
    }

    /// <summary>
    /// Test: Get all ToDos returns all ToDos for the authenticated user, ordered by newest first.
    /// </summary>
    [Fact]
    public async Task GetToDos_WithMultipleToDos_ReturnsAllToDosOrderedByNewestFirst()
    {
        var todos = ToDoFixture.CreateTestToDos(count: 3, userId: _testUser.Id);

        foreach (var todo in todos)
        {
            _context.ToDos.Add(todo);
            await _context.SaveChangesAsync();
            await Task.Delay(10);
        }

        var result = await _controller.GetToDos();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTodos = (okResult!.Value as IEnumerable<Todo>)!.ToList();

        returnedTodos.Should().HaveCount(3);
        returnedTodos.Should().BeInDescendingOrder(t => t.CreatedDate);
    }

    /// <summary>
    /// Test: Get single ToDo by ID returns the correct ToDo.
    /// </summary>
    [Fact]
    public async Task GetTodo_WithValidId_ReturnsCorrectToDo()
    {
        var todo = ToDoFixture.CreateTestToDo(userId: _testUser.Id, description: "Important ToDo");
        _context.ToDos.Add(todo);
        await _context.SaveChangesAsync();

        var result = await _controller.GetTodo(todo.Id);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedTodo = okResult!.Value as Todo;

        returnedTodo.Should().NotBeNull();
        returnedTodo!.Id.Should().Be(todo.Id);
        returnedTodo.Description.Should().Be("Important ToDo");
        returnedTodo.ApplicationUserId.Should().Be(_testUser.Id);
    }

    /// <summary>
    /// Test: Create ToDo with valid request creates ToDo successfully.
    /// </summary>
    [Fact]
    public async Task CreateTodo_WithValidRequest_CreatesToDoSuccessfully()
    {
        var request = ToDoFixture.CreateToDoRequest(description: "New Todo Item");

        var result = await _controller.CreateTodo(request);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(ToDosController.GetTodo));

        var createdTodo = createdResult.Value as Todo;
        createdTodo.Should().NotBeNull();
        createdTodo!.Description.Should().Be("New Todo Item");
        createdTodo.ApplicationUserId.Should().Be(_testUser.Id);
        createdTodo.Status.Should().Be(EntryStatus.Active);
        createdTodo.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Test: Create ToDo with optional due date and time includes them in the created ToDo.
    /// </summary>
    [Fact]
    public async Task CreateTodo_WithDueDateAndTime_IncludesThemInCreatedToDo()
    {
        var dueDate = new DateOnly(2026, 6, 30);
        var dueTime = new TimeOnly(14, 30, 0);
        var request = ToDoFixture.CreateToDoRequest(
            description: "ToDo with deadline",
            dueDate: dueDate,
            dueTime: dueTime);

        var result = await _controller.CreateTodo(request);

        var createdResult = result.Result as CreatedAtActionResult;
        var createdTodo = createdResult!.Value as Todo;

        createdTodo!.DueDate.Should().Be(dueDate);
        createdTodo.DueTime.Should().Be(dueTime);
    }

    /// <summary>
    /// Test: Update ToDo with valid request updates ToDo successfully.
    /// </summary>
    [Fact]
    public async Task UpdateTodo_WithValidRequest_UpdatesToDoSuccessfully()
    {
        var originalTodo = ToDoFixture.CreateTestToDo(userId: _testUser.Id, description: "Original Description");
        _context.ToDos.Add(originalTodo);
        await _context.SaveChangesAsync();

        var updateRequest = ToDoFixture.CreateUpdateToDoRequest(description: "Updated Description");

        var result = await _controller.UpdateTodo(originalTodo.Id, updateRequest);

        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var updatedTodo = okResult!.Value as Todo;

        updatedTodo!.Id.Should().Be(originalTodo.Id);
        updatedTodo.Description.Should().Be("Updated Description");
        updatedTodo.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Test: Update ToDo status changes the status correctly.
    /// </summary>
    [Fact]
    public async Task UpdateTodo_WithStatusChange_UpdatesStatusSuccessfully()
    {
        var todo = ToDoFixture.CreateTestToDo(userId: _testUser.Id, status: EntryStatus.Active);
        _context.ToDos.Add(todo);
        await _context.SaveChangesAsync();

        var updateRequest = ToDoFixture.CreateUpdateToDoRequest(status: EntryStatus.Inactive);

        var result = await _controller.UpdateTodo(todo.Id, updateRequest);

        var okResult = result.Result as OkObjectResult;
        var updatedTodo = okResult!.Value as Todo;

        updatedTodo!.Status.Should().Be(EntryStatus.Inactive);
    }

    /// <summary>
    /// Test: Update ToDo with partial data updates only provided fields.
    /// </summary>
    [Fact]
    public async Task UpdateTodo_WithPartialData_UpdatesOnlyProvidedFields()
    {
        var originalTodo = ToDoFixture.CreateTestToDo(
            userId: _testUser.Id,
            description: "Original Description",
            dueDate: new DateOnly(2026, 5, 31));
        _context.ToDos.Add(originalTodo);
        await _context.SaveChangesAsync();

        var updateRequest = ToDoFixture.CreateUpdateToDoRequest(description: "Updated Description");

        var result = await _controller.UpdateTodo(originalTodo.Id, updateRequest);

        var okResult = result.Result as OkObjectResult;
        var updatedTodo = okResult!.Value as Todo;

        updatedTodo!.Description.Should().Be("Updated Description");
        updatedTodo.DueDate.Should().Be(new DateOnly(2026, 5, 31));
    }

    /// <summary>
    /// Test: Delete ToDo removes ToDo from database successfully.
    /// </summary>
    [Fact]
    public async Task DeleteTodo_WithValidId_DeletesToDoSuccessfully()
    {
        var todo = ToDoFixture.CreateTestToDo(userId: _testUser.Id);
        _context.ToDos.Add(todo);
        await _context.SaveChangesAsync();

        var result = await _controller.DeleteTodo(todo.Id);

        result.Should().BeOfType<NoContentResult>();
        (result as NoContentResult)!.StatusCode.Should().Be(204);

        var deletedTodo = await _context.ToDos.FindAsync(todo.Id);
        deletedTodo.Should().BeNull();
    }

    /// <summary>
    /// Test: Get ToDo with invalid ID returns NotFound.
    /// </summary>
    [Fact]
    public async Task GetTodo_WithInvalidId_ReturnsNotFound()
    {
        var result = await _controller.GetTodo(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Test: User can only access their own ToDos.
    /// </summary>
    [Fact]
    public async Task GetTodo_WithToDoFromDifferentUser_ReturnsNotFound()
    {
        var otherUser = MockUserFixture.CreateTestUser(userId: 2);
        _context.Users.Add(otherUser);

        var todoFromOtherUser = ToDoFixture.CreateTestToDo(userId: otherUser.Id);
        _context.ToDos.Add(todoFromOtherUser);
        await _context.SaveChangesAsync();

        var result = await _controller.GetTodo(todoFromOtherUser.Id);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Test: Delete ToDo with invalid ID returns NotFound.
    /// </summary>
    [Fact]
    public async Task DeleteTodo_WithInvalidId_ReturnsNotFound()
    {
        var result = await _controller.DeleteTodo(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
