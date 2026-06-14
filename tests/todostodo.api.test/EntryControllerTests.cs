using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using todostodo.api.Controllers;
using todostodo.api.Data;
using todostodo.api.Models;

namespace todostodo.api.test;

public class EntryControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly EntryController _controller;
    private readonly User _testUser;

    public EntryControllerTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _testUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "testuser",
            NormalizedUserName = "TESTUSER",
            Email = "test@test.com",
            NormalizedEmail = "TEST@TEST.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        _db.Users.Add(_testUser);
        _db.SaveChanges();

        _controller = new EntryController(_db, NullLogger<EntryController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        [new Claim(ClaimTypes.NameIdentifier, _testUser.Id)],
                        "TestScheme"))
                }
            }
        };
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private async Task<Entry> SeedEntryAsync(
        string title = "Test Entry",
        EntryStatus status = EntryStatus.Active)
    {
        var entry = new Entry
        {
            Title = title,
            Status = status,
            UserId = _testUser.Id
        };
        _db.Entries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }

    // ── GET all ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Get_ReturnsEmptyList_WhenNoEntries()
    {
        var result = await _controller.Get();

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Get_ReturnsAllEntries()
    {
        await SeedEntryAsync("First");
        await SeedEntryAsync("Second");

        var result = await _controller.Get();

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Get_DoesNotReturnOtherUsersEntries()
    {
        // Entries scoped to another user must never appear in the current user's list.
        // This is the core data-isolation requirement: users can only see their own work.
        var otherUser = new User
        {
            Id = Guid.NewGuid().ToString(),
            UserName = "other@test.com",
            NormalizedUserName = "OTHER@TEST.COM",
            Email = "other@test.com",
            NormalizedEmail = "OTHER@TEST.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        };
        _db.Users.Add(otherUser);
        await _db.SaveChangesAsync();

        _db.Entries.Add(new Entry { Title = "My Entry", UserId = _testUser.Id });
        _db.Entries.Add(new Entry { Title = "Their Entry", UserId = otherUser.Id });
        await _db.SaveChangesAsync();

        var result = await _controller.Get();

        result.Should().HaveCount(1);
        result.Single().Title.Should().Be("My Entry");
    }

    // ── GET by id ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetById_ReturnsEntry_WhenFound()
    {
        var seeded = await SeedEntryAsync("Find Me");

        var result = await _controller.Get(seeded.Id);

        result.Result.Should().BeOfType<OkObjectResult>();
        var entry = ((OkObjectResult)result.Result!).Value as Entry;
        entry!.Title.Should().Be("Find Me");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        var result = await _controller.Get(999);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsCreatedResult_WithEntry()
    {
        var req = new CreateEntryRequest("New Entry", EntryStatus.Active);

        var result = await _controller.Create(req);

        result.Result.Should().BeOfType<CreatedAtActionResult>();
        var created = ((CreatedAtActionResult)result.Result!).Value as Entry;
        created!.Title.Should().Be("New Entry");
        created.Status.Should().Be(EntryStatus.Active);
    }

    [Fact]
    public async Task Create_PersistsEntry()
    {
        var req = new CreateEntryRequest("Persisted Entry", EntryStatus.InProgress);

        var result = await _controller.Create(req);
        var created = ((CreatedAtActionResult)result.Result!).Value as Entry;

        _db.ChangeTracker.Clear();
        var fromDb = await _db.Entries.FindAsync(created!.Id);
        fromDb.Should().NotBeNull();
        fromDb!.Title.Should().Be("Persisted Entry");
        fromDb.Status.Should().Be(EntryStatus.InProgress);
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsNoContent_WhenSuccessful()
    {
        var seeded = await SeedEntryAsync("Original");
        var req = new UpdateEntryRequest(seeded.Id, "Updated", null);

        var result = await _controller.Update(seeded.Id, req);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Update_PersistsNewTitle()
    {
        var seeded = await SeedEntryAsync("Original");
        var req = new UpdateEntryRequest(seeded.Id, "Updated", null);

        await _controller.Update(seeded.Id, req);

        _db.ChangeTracker.Clear();
        var updated = await _db.Entries.FindAsync(seeded.Id);
        updated!.Title.Should().Be("Updated");
    }

    [Fact]
    public async Task Update_PersistsNewStatus()
    {
        var seeded = await SeedEntryAsync(status: EntryStatus.Active);
        var req = new UpdateEntryRequest(seeded.Id, null, EntryStatus.Completed);

        await _controller.Update(seeded.Id, req);

        _db.ChangeTracker.Clear();
        var updated = await _db.Entries.FindAsync(seeded.Id);
        updated!.Status.Should().Be(EntryStatus.Completed);
    }

    [Fact]
    public async Task Update_DoesNotChangeTitle_WhenTitleIsEmpty()
    {
        var seeded = await SeedEntryAsync("Unchanged");
        var req = new UpdateEntryRequest(seeded.Id, "", null);

        await _controller.Update(seeded.Id, req);

        _db.ChangeTracker.Clear();
        var updated = await _db.Entries.FindAsync(seeded.Id);
        updated!.Title.Should().Be("Unchanged");
    }

    [Fact]
    public async Task Update_SetsCompletedAt_WhenStatusChangesToCompleted()
    {
        var seeded = await SeedEntryAsync(status: EntryStatus.Active);
        var req = new UpdateEntryRequest(seeded.Id, null, EntryStatus.Completed);

        await _controller.Update(seeded.Id, req);

        _db.ChangeTracker.Clear();
        var updated = await _db.Entries.FindAsync(seeded.Id);
        updated!.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_SetsModifiedAt_OnAnyUpdate()
    {
        var seeded = await SeedEntryAsync("Original");
        var req = new UpdateEntryRequest(seeded.Id, "Updated", null);

        await _controller.Update(seeded.Id, req);

        _db.ChangeTracker.Clear();
        var updated = await _db.Entries.FindAsync(seeded.Id);
        updated!.ModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenIdMismatch()
    {
        var req = new UpdateEntryRequest(99, "Title", null);

        var result = await _controller.Update(1, req);

        result.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        var req = new UpdateEntryRequest(999, "Title", null);

        var result = await _controller.Update(999, req);

        result.Should().BeOfType<NotFoundResult>();
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSuccessful()
    {
        var seeded = await SeedEntryAsync();

        var result = await _controller.Delete(seeded.Id);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_SetsEntryToInactive_NotRemovedFromDatabase()
    {
        var seeded = await SeedEntryAsync();

        await _controller.Delete(seeded.Id);

        _db.ChangeTracker.Clear();
        var entry = await _db.Entries.FindAsync(seeded.Id);
        entry.Should().NotBeNull();
        entry!.Status.Should().Be(EntryStatus.Inactive);
    }

    [Fact]
    public async Task Get_DoesNotReturnInactiveEntries()
    {
        await SeedEntryAsync("Active Entry", EntryStatus.Active);
        await SeedEntryAsync("Inactive Entry", EntryStatus.Inactive);

        var result = await _controller.Get();

        result.Should().HaveCount(1);
        result.Single().Title.Should().Be("Active Entry");
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenEntryDoesNotExist()
    {
        var result = await _controller.Delete(999);

        result.Should().BeOfType<NotFoundResult>();
    }
}
