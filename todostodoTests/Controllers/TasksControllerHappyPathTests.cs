using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using todostodo.todostodo.Controllers;
using todostodo.Data;
using todostodo.Models;
//?? no test folder
using todostodo.Tests.Fixtures;
using Xunit;
using todostodo.Models.ToDo;

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
    private ApplicationUser _testUser;

    public ToDosControllerHappyPathTests()
    {
        _databaseFixture = new DatabaseFixture();
        _context = _databaseFixture.DbContext;
        _controller = new ToDosController(_context);
    }

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();

        // Create and add test user
        _testUser = MockUserFixture.CreateTestUser(userId: 1);
        _context.Users.Add(_testUser);
        await _context.SaveChangesAsync();

        // Setup controller with authenticated user
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
        // Act
        var result = await _controller.GetToDos();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);

        var ToDos = okResult.Value as IEnumerable<ToDo>;
        ToDos.Should().NotBeNull();
        ToDos!.Should().BeEmpty();
    }

    /// <summary>
    /// Test: Get all ToDos returns all ToDos for the authenticated user, ordered by newest first.
    /// </summary>
    [Fact]
    public async Task GetToDos_WithMultipleToDos_ReturnsAllToDosOrderedByNewestFirst()
    {
        // Arrange
        var ToDos = ToDoFixture.CreateTestToDos(count: 3, userId: _testUser.Id);
        
        // Add small delays to ensure different creation dates
        var createdToDos = new List<ToDo>();
        foreach (var ToDo in ToDos)
        {
            _context.ToDos.Add(ToDo);
            await _context.SaveChangesAsync();
            await ToDo.Delay(10); // Small delay to ensure different timestamps
            createdToDos.Add(ToDo);
        }

        // Act
        var result = await _controller.GetToDos();

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedToDos = (okResult!.Value as IEnumerable<ToDo>)!.ToList();

        returnedToDos.Should().HaveCount(3);
        // Verify they are ordered by creation date descending (newest first)
        returnedToDos.Should().BeInDescendingOrder(t => t.CreatedDate);
    }

    /// <summary>
    /// Test: Get single ToDo by ID returns the correct ToDo.
    /// </summary>
    [Fact]
    public async Task GetToDo_WithValidId_ReturnsCorrectToDo()
    {
        // Arrange
        var ToDo = ToDoFixture.CreateTestToDo(id: 1, userId: _testUser.Id, description: "Important ToDo");
        _context.ToDos.Add(ToDo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetToDo(ToDo.Id);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var returnedToDo = okResult!.Value as ToDo;

        returnedToDo.Should().NotBeNull();
        returnedToDo!.Id.Should().Be(ToDo.Id);
        returnedToDo.Description.Should().Be("Important ToDo");
        returnedToDo.ApplicationUserId.Should().Be(_testUser.Id);
    }

    /// <summary>
    /// Test: Create ToDo with valid request creates ToDo successfully.
    /// </summary>
    [Fact]
    public async Task CreateToDo_WithValidRequest_CreatesToDoSuccessfully()
    {
        // Arrange
        var request = ToDoFixture.CreateToDoRequest(description: "New Todo Item");

        // Act
        var result = await _controller.CreateToDo(request);

        // Assert
        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = result.Result as CreatedAtActionResult;
        createdResult!.StatusCode.Should().Be(201);
        createdResult.ActionName.Should().Be(nameof(ToDosController.GetToDo));

        var createdToDo = createdResult.Value as ToDo;
        createdToDo.Should().NotBeNull();
        createdToDo!.Description.Should().Be("New Todo Item");
        createdToDo.ApplicationUserId.Should().Be(_testUser.Id);
        createdToDo.Status.Should().Be(EntryStatus.Active);
        createdToDo.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Test: Create ToDo with optional due date and time includes them in the created ToDo.
    /// </summary>
    [Fact]
    public async Task CreateToDo_WithDueDateAndTime_IncludesThemInCreatedToDo()
    {
        // Arrange
        var dueDate = new DateOnly(2026, 6, 30);
        var dueTime = new TimeOnly(14, 30, 0);
        var request = ToDoFixture.CreateToDoRequest(
            description: "ToDo with deadline",
            dueDate: dueDate,
            dueTime: dueTime);

        // Act
        var result = await _controller.CreateToDo(request);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        var createdToDo = createdResult!.Value as ToDo;

        createdToDo!.DueDate.Should().Be(dueDate);
        createdToDo.DueTime.Should().Be(dueTime);
    }

    /// <summary>
    /// Test: Update ToDo with valid request updates ToDo successfully.
    /// </summary>
    [Fact]
    public async Task UpdateToDo_WithValidRequest_UpdatesToDoSuccessfully()
    {
        // Arrange
        var originalToDo = ToDoFixture.CreateTestToDo(id: 1, userId: _testUser.Id, description: "Original Description");
        _context.ToDos.Add(originalToDo);
        await _context.SaveChangesAsync();

        var updateRequest = ToDoFixture.CreateUpdateToDoRequest(description: "Updated Description");

        // Act
        var result = await _controller.UpdateToDo(originalToDo.Id, updateRequest);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
        var okResult = result.Result as OkObjectResult;
        var updatedToDo = okResult!.Value as ToDo;

        updatedToDo!.Id.Should().Be(originalToDo.Id);
        updatedToDo.Description.Should().Be("Updated Description");
        updatedToDo.ModifiedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Test: Update ToDo status changes the status correctly.
    /// </summary>
    [Fact]
    public async Task UpdateToDo_WithStatusChange_UpdatesStatusSuccessfully()
    {
        // Arrange
        var ToDo = ToDoFixture.CreateTestToDo(
            id: 1, 
            userId: _testUser.Id, 
            status: EntryStatus.Active);
        _context.ToDos.Add(ToDo);
        await _context.SaveChangesAsync();

        var updateRequest = ToDoFixture.CreateUpdateToDoRequest(status: EntryStatus.Completed);

        // Act
        var result = await _controller.UpdateToDo(ToDo.Id, updateRequest);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var updatedToDo = okResult!.Value as ToDo;

        updatedToDo!.Status.Should().Be(EntryStatus.Completed);
    }

    /// <summary>
    /// Test: Update ToDo with partial data updates only provided fields.
    /// </summary>
    [Fact]
    public async Task UpdateToDo_WithPartialData_UpdatesOnlyProvidedFields()
    {
        // Arrange
        var originalToDo = ToDoFixture.CreateTestToDo(
            id: 1,
            userId: _testUser.Id,
            description: "Original Description",
            dueDate: new DateOnly(2026, 5, 31));
        _context.ToDos.Add(originalToDo);
        await _context.SaveChangesAsync();

        var updateRequest = ToDoFixture.CreateUpdateToDoRequest(description: "Updated Description");
        // DueDate is not set in the request

        // Act
        var result = await _controller.UpdateToDo(originalToDo.Id, updateRequest);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var updatedToDo = okResult!.Value as ToDo;

        updatedToDo!.Description.Should().Be("Updated Description");
        updatedToDo.DueDate.Should().Be(new DateOnly(2026, 5, 31)); // Should remain unchanged
    }

    /// <summary>
    /// Test: Delete ToDo removes ToDo from database successfully.
    /// </summary>
    [Fact]
    public async Task DeleteToDo_WithValidId_DeletesToDoSuccessfully()
    {
        // Arrange
        var ToDo = ToDoFixture.CreateTestToDo(id: 1, userId: _testUser.Id);
        _context.ToDos.Add(ToDo);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteToDo(ToDo.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        var noContentResult = result as NoContentResult;
        noContentResult!.StatusCode.Should().Be(204);

        // Verify ToDo is deleted
        var deletedToDo = await _context.ToDos.FindAsync(ToDo.Id);
        deletedToDo.Should().BeNull();
    }

    /// <summary>
    /// Test: Get ToDo with invalid ID returns NotFound.
    /// </summary>
    [Fact]
    public async Task GetToDo_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetToDo(999);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Test: User can only access their own ToDos.
    /// </summary>
    [Fact]
    public async Task GetToDo_WithToDoFromDifferentUser_ReturnsNotFound()
    {
        // Arrange
        var otherUser = MockUserFixture.CreateTestUser(userId: 2);
        _context.Users.Add(otherUser);
        
        var ToDoFromOtherUser = ToDoFixture.CreateTestToDo(id: 1, userId: otherUser.Id);
        _context.ToDos.Add(ToDoFromOtherUser);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetToDo(ToDoFromOtherUser.Id);

        // Assert
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    /// <summary>
    /// Test: Delete ToDo with invalid ID returns NotFound.
    /// </summary>
    [Fact]
    public async Task DeleteToDo_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.DeleteToDo(999);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
}
