using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "QuickNotes API",
        Version = "v1",
        Description = "A simple CRUD API for managing notes."
    });
});

// Database
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
        ?? builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Database=notesdb;Username=postgres;Password=postgres";

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(connectionString));
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.UseCors();

// Swagger (always enabled so it works inside Docker too)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "QuickNotes API v1");
    options.RoutePrefix = "swagger";
});

// ─── API Endpoints ──────────────────────────────────────────

// GET /api/notes
app.MapGet("/api/notes", async (AppDbContext db) =>
    await db.Notes.OrderByDescending(n => n.CreatedAt).ToListAsync());

// GET /api/notes/{id}
app.MapGet("/api/notes/{id:int}", async (int id, AppDbContext db) =>
    await db.Notes.FindAsync(id) is Note note
        ? Results.Ok(note)
        : Results.NotFound());

// POST /api/notes
app.MapPost("/api/notes", async (Note note, AppDbContext db) =>
{
    note.CreatedAt = DateTime.UtcNow;
    db.Notes.Add(note);
    await db.SaveChangesAsync();
    return Results.Created($"/api/notes/{note.Id}", note);
});

// PUT /api/notes/{id}
app.MapPut("/api/notes/{id:int}", async (int id, Note input, AppDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();

    note.Title = input.Title;
    note.Content = input.Content;
    await db.SaveChangesAsync();
    return Results.Ok(note);
});

// DELETE /api/notes/{id}
app.MapDelete("/api/notes/{id:int}", async (int id, AppDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();

    db.Notes.Remove(note);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

// Make the implicit Program class accessible for integration tests
public partial class Program { }
