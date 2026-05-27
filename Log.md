


# NEXT

##  add SQlite
chat directions:

1) Use EF Core with SQLite (recommended)
- Add packages:
  - dotnet add package Microsoft.EntityFrameworkCore.Sqlite
  - dotnet add package Microsoft.EntityFrameworkCore.Design
- Configure your DbContext in Program.cs:

```csharp
using Microsoft.EntityFrameworkCore;
using todostodo.Data;
// ...existing code...
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=todostodo.db"));
```

- Add a connection string to appsettings.json (or rely on the inline fallback above).
- Create migrations and update DB:
  - dotnet ef migrations add Initial
  - dotnet ef database update

Notes: Identity + EF Core works with SQLite. Migrations on SQLite have limitations (no ALTER COLUMN), so some schema changes may require manual steps.

2) Use SQLite without EF Core (raw SQL)
- Add package:
  - dotnet add package Microsoft.Data.Sqlite
- Use direct connections/commands:

```csharp
using Microsoft.Data.Sqlite;

public class SqliteService
{
    private readonly string _cs = "Data Source=todostodo.db";
    public void Example()
    {
        using var conn = new SqliteConnection(_cs);
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "CREATE TABLE IF NOT EXISTS Todos(Id INTEGER PRIMARY KEY, Title TEXT)";
        cmd.ExecuteNonQuery();
    }
}
```

I'm choosing EF Core + Sqlite for LINQ, migrations and Identity integration

# 5/27
tried to get project running before adding sqlite. remembered about Solutions vs projects so I'm going to see if I can fix this without making a new project.
-- the issue was 
also the HTTPS Developer Certificate was causing the Error, the suggested commands worked.... after a restart.
https://stackoverflow.com/questions/54371101/cannot-create-developer-certificate-on-mac
https://learn.microsoft.com/en-us/aspnet/core/breaking-changes/9/certificate-export?view=aspnetcore-10.0


# 5/26

## generate framework for react/.net core application
- authentication built in
- I haven't used C# for 5 or 6 years, so want to review and also need to set up VSC => added dotnet, added a new template
`dotnet new search <searchterm>`
`dotnet new install Asp.React.Project.Templates`

## Planning/brain dump/define MVP
- first pass / all the thoughts / mess > braindump.txt
- PLANME.md >> clean up of initial pass