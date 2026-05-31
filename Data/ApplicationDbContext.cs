using todostodo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TaskModel = todostodo.Models.Task;

namespace todostodo.Data;

/// <summary>
/// Entity Framework Core database context for the application, managing users, tasks, entries, and settings.
/// </summary>
public class ApplicationDbContext: IdentityDbContext<ApplicationUser, IdentityRole<int>, int> 
{
    /// <summary>
    /// Initializes a new instance of the ApplicationDbContext.
    /// </summary>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
       
    }
    
    /// <summary>
    /// Gets or sets the collection of Entry entities.
    /// </summary>
    public DbSet<Entry> Entries { get; set; }

    /// <summary>
    /// Gets or sets the collection of Task entities.
    /// </summary>
    public DbSet<TaskModel> Tasks { get; set; }

    /// <summary>
    /// Gets or sets the collection of Settings entities.
    /// </summary>
    public DbSet<Settings> Settings { get; set; }
        
    /// <summary>
    /// Configures the model for the database, including relationships and inheritance.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // Configure Task as derived from Entry (Table-per-hierarchy)
        builder.Entity<Entry>()
            .HasDiscriminator<string>("EntryType")
            .HasValue<Entry>("Entry")
            .HasValue<TaskModel>("Task");
        
        // Configure relationships
        builder.Entity<Entry>()
            .HasOne(e => e.ApplicationUser)
            .WithMany()
            .HasForeignKey(e => e.ApplicationUserId);
        
        builder.Entity<Settings>()
            .HasOne(s => s.ApplicationUser)
            .WithMany()
            .HasForeignKey(s => s.ApplicationUserId);
    }
}