using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer; // Add this using directive
using TaskManagerApi.Data;
using TaskManagerApi.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

var builder = WebApplication.CreateBuilder(args);

// Get connection string from environment or configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=tasks.db"; // Fallback to SQLite for local development

// Choose database provider based on connection string
if (connectionString.Contains("Data Source"))
{
    builder.Services.AddDbContext<TaskDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<TaskDbContext>(options =>
        options.UseSqlServer(connectionString));
}

// Add CORS with your deployed Angular app URL
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://happy-field-0f3f8b210.2.azurestaticapps.net")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAngular");

// API Endpoints using Entity Framework
app.MapGet("/api/tasks", async (TaskDbContext db) =>
    await db.Tasks.ToListAsync());

app.MapPost("/api/tasks", async (TaskItem task, TaskDbContext db) =>
{
    task.CreatedAt = DateTime.UtcNow;
    db.Tasks.Add(task);
    await db.SaveChangesAsync();
    return Results.Created($"/api/tasks/{task.Id}", task);
});

app.MapPut("/api/tasks/{id}", async (int id, TaskItem updatedTask, TaskDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task == null) return Results.NotFound();

    task.Title = updatedTask.Title;
    task.IsCompleted = updatedTask.IsCompleted;
    await db.SaveChangesAsync();

    return Results.Ok(task);
});

app.MapDelete("/api/tasks/{id}", async (int id, TaskDbContext db) =>
{
    var task = await db.Tasks.FindAsync(id);
    if (task == null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// Auto-run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TaskDbContext>();
    context.Database.Migrate();
}

app.Run(); // locally "http://localhost:5000"