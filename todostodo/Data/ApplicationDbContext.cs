using todostodo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace todostodo.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser, IdentityRole<int>, int> 
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    
    public DbSet<Entry> Entries { get; set; }
    public DbSet<Todo> ToDos { get; set; }
    public DbSet<Settings> Settings { get; set; }
        
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // TODO what does this comment mean
        // Configure Todo as derived from Entry (Table-per-hierarchy)
        builder.Entity<Entry>()
            .HasDiscriminator<string>("EntryType")
            .HasValue<Todo>("ToDo");
        
        // Configure relationships
        builder.Entity<Entry>()
            // HasOne is for dependent to principal 
            .HasOne(e => e.ApplicationUser)
            .WithOne()
            .HasForeignKey<ApplicationUser>(e => e.Id);
        
        builder.Entity<Settings>()
            .HasOne(s => s.ApplicationUser)
            .WithOne()
            .HasForeignKey<ApplicationUser>(s => s.Id);
    }
}