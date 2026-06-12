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
/// These tests cover two separate concerns that live inside the action methods themselves:
///
///   1. Defense-in-depth: the controller checks that the NameIdentifier claim exists
///      even after the framework has already authenticated the user, guarding against
///      edge cases like tokens without standard claims.
///
///   2. Ownership gap documentation: PUT and DELETE are currently open to any caller.
///      The tests below assert the current (insecure) behaviour and will act as canaries
///      when ownership enforcement is eventually added — they should then be updated to
///      expect 403/404 instead of 204.
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

    // ── Update / Delete — current ownership gap ────────────────────────────────

    // These two tests document that PUT and DELETE have no [Authorize] attribute and
    // perform no ownership check. They are written as "currently passes" so that adding
    // the [Authorize] attribute or an ownership guard will break them, signalling that
    // the tests below need to be updated to expect 401 / 403 / 404.

    [Fact]
    public async Task Update_CurrentlyAllows_CrossUserModification()
    {
        var entry = await SeedEntryAsync(_ownerUser.Id, "Owner's Entry");
        var otherController = ControllerFor(_otherUser.Id);

        var req = new UpdateEntryRequest(entry.Id, "Tampered by Other", null);
        var result = await otherController.Update(entry.Id, req);

        // TODO: update this expectation to 403/404 once [Authorize] + ownership check is added.
        result.Should().BeOfType<NoContentResult>(
            "ownership enforcement is not yet implemented");
    }

    [Fact]
    public async Task Delete_CurrentlyAllows_CrossUserDeletion()
    {
        var entry = await SeedEntryAsync(_ownerUser.Id, "Owner's Entry");
        var otherController = ControllerFor(_otherUser.Id);

        var result = await otherController.Delete(entry.Id);

        // TODO: update this expectation to 403/404 once [Authorize] + ownership check is added.
        result.Should().BeOfType<NoContentResult>(
            "ownership enforcement is not yet implemented");
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
