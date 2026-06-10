using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using todostodo.api.Data;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// :memory: is what makes it in-memory
var connection = new SqliteConnection("Data Source=:memory:");

connection.Open();

// add db context with the in-memory connection
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connection)
);

// adds Identity services to the dependency injection container
builder.Services.AddAuthorization();

// activate API endpoints for Identity
// by default cookies and proprietary tokens are activated >> issuesd at login if the useCookies query string param is true
builder.Services.AddIdentityApiEndpoints<IdentityUser>().AddEntityFrameworkStores<AppDbContext>(); 

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// replacement for swagger
builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "todostodo.web", policy =>
    {
        policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod();
    });
}); 

var app = builder.Build();

// map identity api endpoints
app.MapIdentityApi<IdentityUser>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// TODO secure endpoints - in controller [Authorize] attribute

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

// only in dev >> remove for openapi?
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("todostodo.web");

app.MapControllers();

app.Run();