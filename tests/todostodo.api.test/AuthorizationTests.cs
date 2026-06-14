using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using todostodo.api.Controllers;
using todostodo.api.Data;
using todostodo.api.Models;

namespace todostodo.api.test;

/// <summary>
/// Unit tests for authentication and ownership logic inside <see cref="EntryController"/>.
///
/// The <c>[Authorize]</c> attribute (pipeline-level enforcement) is covered by
/// <c>Integration/AuthFlowIntegrationTests.cs</c>, where the full middleware stack runs.
/// These tests cover the logic inside the action methods:
///
///   1. Defense-in-depth: the controller checks that the NameIdentifier claim exists
///      even after the framework has already authenticated the user.
///
///   2. Ownership enforcement: all mutating endpoints and both GET endpoints now
///      scope their queries to the authenticated user's entries. Cross-user access
///      returns 404 to avoid confirming that another user's entry exists.
/// </summary>
public class AuthorizationTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly User _ownerUser;
    private readonly User _otherUser;

    public AuthorizationTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _ownerUser = MakeUser("owner@test.com");
        _otherUser = MakeUser("other@test.com");
        _db.Users.AddRange(_ownerUser, _otherUser);
        _db.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    // ── Create — defense-in-depth checks ──────────────────────────────────────

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenUserIdClaimIsMissing()
    {
        // Scenario: the framework considers the user authenticated (an identity is present)
        // but the token / cookie has no NameIdentifier claim. This can happen with custom
        // auth schemes that don't set standard claims. The explicit null-check inside the
        // action prevents a NullReferenceException and returns a clean 401 instead.
        var controller = ControllerFor(userId: null);
        var req = new CreateEntryRequest("Test", null, EntryStatus.Active);

        var result = await controller.Create(req);

        result.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task Create_AssignsUserId_FromAuthenticatedUserClaim()
    {
        // The UserId on the created entry must come exclusively from the JWT/cookie claim,
        // never from the request body. This ensures one user cannot create entries on behalf
        // of another by crafting a custom payload.
        var controller = ControllerFor(_ownerUser.Id);
        var req = new CreateEntryRequest("My Entry", null, EntryStatus.Active);

        var result = await controller.Create(req);
        var created = ((CreatedAtActionResult)result.Result!).Value as Entry;

        created!.UserId.Should().Be(_ownerUser.Id);
    }

    [Fact]
    public async Task Create_DoesNotStampOtherUsersId_OnNewEntry()
    {
        // Complementary to the test above: verify the stamp is exclusively the caller's id,
        // not any other user's id that happens to exist in the database.
        var controller = ControllerFor(_ownerUser.Id);
        var req = new CreateEntryRequest("Entry", null, EntryStatus.Active);

        var result = await controller.Create(req);
        var created = ((CreatedAtActionResult)result.Result!).Value as Entry;

        created!.UserId.Should().NotBe(_otherUser.Id);
    }

    // ── Get — ownership enforcement ────────────────────────────────────────────

    [Fact]
    public async Task GetAll_ExcludesOtherUsersEntries()
    {
        // The list endpoint must only return the authenticated user's own entries.
        // Leaking other users' entries is a privacy violation regardless of whether
        // the client chooses to display them.
        await SeedEntryAsync(_ownerUser.Id, "Owner Entry");
        await SeedEntryAsync(_otherUser.Id, "Other User Entry");
        var controller = ControllerFor(_ownerUser.Id);

        var result = await controller.Get();

        result.Should().HaveCount(1);
        result.Single().Title.Should().Be("Owner Entry");
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_ForOtherUsersEntry()
    {
        // 404 (not 403) is intentional: returning 403 would confirm that the entry exists,
        // which leaks information about another user's data. 404 is the correct response
        // when the caller has no access — from their perspective, the resource doesn't exist.
        var otherEntry = await SeedEntryAsync(_otherUser.Id, "Private Entry");
        var controller = ControllerFor(_ownerUser.Id);

        var result = await controller.Get(otherEntry.Id);

        result.Result.Should().BeOfType<NotFoundResult>();
    }

    // ── Update — ownership enforcement ────────────────────────────────────────

    [Fact]
    public async Task Update_ReturnsNotFound_ForOtherUsersEntry()
    {
        // Same 404-not-403 rationale as GetById: the caller should not learn
        // that a given id belongs to a different user.
        var entry = await SeedEntryAsync(_ownerUser.Id, "Owner's Entry");
        var otherController = ControllerFor(_otherUser.Id);

        var req = new UpdateEntryRequest(entry.Id, "Tampered", null);
        var result = await otherController.Update(entry.Id, req);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Update_DoesNotPersistChange_WhenCalledByOtherUser()
    {
        // Verify the entry is actually unchanged in the DB, not just that the
        // controller returned 404. Belt-and-suspenders guard against a future
        // refactor that accidentally writes before checking ownership.
        var entry = await SeedEntryAsync(_ownerUser.Id, "Original Title");
        var otherController = ControllerFor(_otherUser.Id);

        await otherController.Update(entry.Id, new UpdateEntryRequest(entry.Id, "Tampered", null));

        _db.ChangeTracker.Clear();
        var unchanged = await _db.Entries.FindAsync(entry.Id);
        unchanged!.Title.Should().Be("Original Title");
    }

    // ── Delete — ownership enforcement ────────────────────────────────────────

    [Fact]
    public async Task Delete_ReturnsNotFound_ForOtherUsersEntry()
    {
        var entry = await SeedEntryAsync(_ownerUser.Id, "Owner's Entry");
        var otherController = ControllerFor(_otherUser.Id);

        var result = await otherController.Delete(entry.Id);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Delete_DoesNotRemoveEntry_WhenCalledByOtherUser()
    {
        // Belt-and-suspenders: confirm the row still exists after the rejected delete.
        var entry = await SeedEntryAsync(_ownerUser.Id, "Owner's Entry");
        var otherController = ControllerFor(_otherUser.Id);

        await otherController.Delete(entry.Id);

        _db.ChangeTracker.Clear();
        var stillExists = await _db.Entries.FindAsync(entry.Id);
        stillExists.Should().NotBeNull();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static User MakeUser(string email) => new()
    {
        Id = Guid.NewGuid().ToString(),
        UserName = email,
        NormalizedUserName = email.ToUpperInvariant(),
        Email = email,
        NormalizedEmail = email.ToUpperInvariant(),
        SecurityStamp = Guid.NewGuid().ToString()
    };

    private EntryController ControllerFor(string? userId)
    {
        var claims = userId is not null
            ? [new Claim(ClaimTypes.NameIdentifier, userId)]
            : Array.Empty<Claim>();

        return new EntryController(_db)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestScheme"))
                }
            }
        };
    }

    private async Task<Entry> SeedEntryAsync(string userId, string title = "Test Entry")
    {
        var entry = new Entry { Title = title, UserId = userId };
        _db.Entries.Add(entry);
        await _db.SaveChangesAsync();
        return entry;
    }
}
