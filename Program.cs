using Microsoft.EntityFrameworkCore;
using TaskManagerApi.Data;
using TaskManagerApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add Entity Framework
builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlite("Data Source=tasks.db"));

// Add CORS for Angular
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
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

app.Run("http://localhost:5000");