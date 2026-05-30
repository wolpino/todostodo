using todostodo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using TaskModel = todostodo.Models.Task;

namespace todostodo.Data;

public class ApplicationDbContext: IdentityDbContext<ApplicationUser, IdentityRole<int>, int> 
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
       
    }
    
    public DbSet<Entry> Entries { get; set; }
    public DbSet<TaskModel> Tasks { get; set; }
    public DbSet<Settings> Settings { get; set; }
        
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