using todostodo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace todostodo.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser, IdentityRole<int>, int> 
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
       
    }
    
    public DbSet<Entry> Entries { get; set; }
    public DbSet<ToDo> ToDos { get; set; }
    public DbSet<Settings> Settings { get; set; }
        
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // TODO what does this comment mean
        // Configure Todo as derived from Entry (Table-per-hierarchy)
        builder.Entity<Entry>()
            .HasDiscriminator<string>("EntryType")
            .HasValue<Entry>("Entry")
            .HasValue<ToDo>("ToDo");
        
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