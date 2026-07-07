using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using todostodo.api.Models;

namespace todostodo.api.Data;

/// <summary>
/// EF Core context for Identity users, todo entries, and per-user settings.
/// Extends <see cref="IdentityDbContext{TUser}"/> so auth and app data share one database.
/// </summary>
public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Entry> Entries => Set<Entry>();
    public DbSet<Settings> Settings => Set<Settings>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // One settings row per user — enforced at the DB with unique index, not only in controller logic.
        builder.Entity<Settings>()
            .HasIndex(s => s.UserId)
            .IsUnique();
    }
}
