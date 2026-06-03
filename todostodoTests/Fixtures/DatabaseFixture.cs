using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using todostodo.Data;

namespace todostodo.Tests.Fixtures;

/// <summary>
/// Manages a per-test SQLite in-memory database connection and DbContext.
/// Each instance gets its own isolated connection so tests cannot bleed state.
/// </summary>
public class DatabaseFixture : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public ApplicationDbContext DbContext { get; }

    public DatabaseFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        DbContext = new ApplicationDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
