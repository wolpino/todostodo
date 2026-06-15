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

public class SettingsControllerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly SettingsController _controller;
    private readonly User _testUser;

    public SettingsControllerTests()
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

        _controller = new SettingsController(_db, NullLogger<SettingsController>.Instance)
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

    [Fact]
    public async Task Get_CreatesDefaultSettings_WhenNoneExist()
    {
        var result = await _controller.Get();

        result.Result.Should().BeOfType<OkObjectResult>();
        var settings = ((OkObjectResult)result.Result!).Value as Settings;
        settings!.Font.Should().Be(SettingsController.DefaultFont);
        settings.UserId.Should().Be(_testUser.Id);
    }

    [Fact]
    public async Task Get_ReturnsExistingSettings_WhenPresent()
    {
        _db.Settings.Add(new Settings { Font = "caveat", UserId = _testUser.Id });
        await _db.SaveChangesAsync();

        var result = await _controller.Get();

        var settings = ((OkObjectResult)result.Result!).Value as Settings;
        settings!.Font.Should().Be("caveat");
    }

    [Fact]
    public async Task Put_UpdatesFont_WhenSettingsExist()
    {
        _db.Settings.Add(new Settings { Font = SettingsController.DefaultFont, UserId = _testUser.Id });
        await _db.SaveChangesAsync();

        var result = await _controller.Put(new UpdateSettingsRequest("courier-prime"));

        result.Should().BeOfType<NoContentResult>();
        _db.ChangeTracker.Clear();
        var updated = await _db.Settings.SingleAsync(s => s.UserId == _testUser.Id);
        updated.Font.Should().Be("courier-prime");
    }

    [Fact]
    public async Task Put_CreatesSettings_WhenNoneExist()
    {
        var result = await _controller.Put(new UpdateSettingsRequest("caveat"));

        result.Should().BeOfType<NoContentResult>();
        _db.ChangeTracker.Clear();
        var created = await _db.Settings.SingleAsync(s => s.UserId == _testUser.Id);
        created.Font.Should().Be("caveat");
    }

    [Fact]
    public async Task Put_ReturnsBadRequest_WhenFontIsInvalid()
    {
        var result = await _controller.Put(new UpdateSettingsRequest("comic-sans"));

        result.Should().BeOfType<BadRequestResult>();
    }
}
