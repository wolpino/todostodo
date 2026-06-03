using todostodo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace todostodo.Data;

public static class DevDataSeeder
{
    private record DogUser(string UserName, string Email, string Password);

    private record DogToDo(string Description, DateOnly? DueDate, TimeOnly? DueTime, EntryStatus Status = EntryStatus.Active);

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
            return;

        var dogs = new[]
        {
            new DogUser("Rex",     "rex@dogmail.com",     "W00fWoof1"),
            new DogUser("Bella",   "bella@dogmail.com",   "W00fWoof1"),
            new DogUser("Biscuit", "biscuit@dogmail.com", "W00fWoof1"),
            new DogUser("Luna",    "luna@dogmail.com",    "W00fWoof1"),
            new DogUser("Cheddar", "cheddar@dogmail.com", "W00fWoof1"),
        };

        var todosPerDog = new Dictionary<string, DogToDo[]>
        {
            ["Rex"] =
            [
                new("Bark at the mail carrier — must be very loud and sustained", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(9, 0)),
                new("Patrol the backyard perimeter for squirrel activity (3 laps minimum)", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 30)),
                new("Demand belly rubs from every human in the house", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(14, 0)),
                new("Claim the sunny spot on the couch before the cat does", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(15, 30)),
            ],
            ["Bella"] =
            [
                new("Check if the food bowl magically refilled itself (check hourly)", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(8, 0)),
                new("Chase tail until it is definitively caught — do not give up", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(11, 0)),
                new("Steal one sock from laundry basket and relocate it to the yard", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(13, 30)),
                new("Alert the entire household about the suspicious vacuum cleaner", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(16, 0)),
            ],
            ["Biscuit"] =
            [
                new("Mark every single tree on the morning walk — thorough coverage only", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(7, 30)),
                new("Sniff at least 47 things during the walk before accepting to come home", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(7, 45)),
                new("Perfect the puppy eyes technique before dinner time", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(17, 45)),
                new("Guard the couch aggressively from being sat on by other dogs", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(20, 0)),
            ],
            ["Luna"] =
            [
                new("Greet human at the door as though they have been gone for seven years", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(18, 0)),
                new("Rearrange all couch pillows by sitting on them repeatedly", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(12, 0)),
                new("Investigate the suspicious crinkling noise coming from the kitchen", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(9, 30)),
                new("Execute full zoomies lap sequence in backyard — 5 laps, no stops", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(17, 0)),
            ],
            ["Cheddar"] =
            [
                new("Stare intensely at human until walk time is acknowledged", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(8, 30)),
                new("Monitor squirrel activity from the living room window (2-hour surveillance shift)", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(10, 0)),
                new("Remind human it is dinner time by sitting next to the bowl since 3 PM", DateOnly.FromDateTime(DateTime.Today), new TimeOnly(15, 0)),
                new("Attempt to sleep through the thunderstorm with dignity — results may vary", DateOnly.FromDateTime(DateTime.Today), null, EntryStatus.Inactive),
            ],
        };

        foreach (var dog in dogs)
        {
            var user = new ApplicationUser
            {
                UserName = dog.UserName,
                Email = dog.Email,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(user, dog.Password);
            if (!result.Succeeded)
                continue;

            var todos = todosPerDog[dog.UserName].Select(t => new Todo
            {
                Description = t.Description,
                DueDate = t.DueDate,
                DueTime = t.DueTime,
                Status = t.Status,
                CreatedDate = DateTime.UtcNow,
                ApplicationUserId = user.Id,
            });

            await db.ToDos.AddRangeAsync(todos);
        }

        await db.SaveChangesAsync();
    }
}
