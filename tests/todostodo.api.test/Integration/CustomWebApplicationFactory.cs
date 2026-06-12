using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using todostodo.api.Data;

namespace todostodo.api.test.Integration;

/// <summary>
/// Bootstraps the real ASP.NET Core pipeline for integration tests.
///
/// <para>
/// <b>Why replace the database?</b><br/>
/// <c>Program.cs</c> opens a single <see cref="SqliteConnection"/> in the
/// top-level statements and registers <see cref="AppDbContext"/> against it.
/// That connection is a local variable — it cannot be intercepted after startup.
/// We remove the <see cref="DbContextOptions{AppDbContext}"/> descriptor and
/// register a fresh, test-owned in-memory SQLite connection instead.
/// </para>
///
/// <para>
/// <b>Connection lifetime:</b><br/>
/// An in-memory SQLite database lives only as long as the connection that created it.
/// We open the connection in the factory constructor and hold it open until the factory
/// is disposed, which keeps the schema alive for the lifetime of the test class.
/// </para>
///
/// <para>
/// Usage: declare <c>IClassFixture&lt;CustomWebApplicationFactory&gt;</c> on a test class.
/// xUnit creates one factory instance per test class, giving each class its own isolated
/// database. Tests within the same class share that database, so each test should use
/// unique seed data (e.g. distinct email addresses).
/// </para>
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public CustomWebApplicationFactory()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Run as "Testing" so we can gate environment-specific behaviour if needed.
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the DbContextOptions that Program.cs registered against its own
            // hard-coded connection. Leaving both registrations would cause the DI
            // container to use the last one — explicit removal avoids that ambiguity.
            var toRemove = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();

            foreach (var descriptor in toRemove)
                services.Remove(descriptor);

            // Register a fresh DbContext backed by our test-owned connection.
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Program.cs calls EnsureCreated() during startup, but it runs against the
        // original connection. Call it again here against our replacement connection
        // to guarantee the Identity + Entry schema exists in the test database.
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
    }
}
