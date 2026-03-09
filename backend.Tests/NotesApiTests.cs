using System.Net;
using System.Net.Http.Json;
using Backend.Models;
using Xunit;

namespace Backend.Tests;

/// <summary>
/// Integration tests for the QuickNotes CRUD API.
/// Each test gets a fresh in-memory database via TestWebApplicationFactory.
/// </summary>
public class NotesApiTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public NotesApiTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Helpers ────────────────────────────────────────────────

    private async Task<Note> CreateTestNote(string title = "Test Note", string content = "Test content")
    {
        var response = await _client.PostAsJsonAsync("/api/notes", new { title, content });
        response.EnsureSuccessStatusCode();
        var note = await response.Content.ReadFromJsonAsync<Note>();
        return note!;
    }

    // ── GET /api/notes ────────────────────────────────────────

    [Fact]
    public async Task GetNotes_ReturnsEmptyList_WhenNoNotesExist()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/api/notes");

        // Assert
        response.EnsureSuccessStatusCode();
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
    }

    [Fact]
    public async Task GetNotes_ReturnsNotes_AfterCreating()
    {
        // Arrange
        await CreateTestNote("Note 1", "Content 1");
        await CreateTestNote("Note 2", "Content 2");

        // Act
        var response = await _client.GetAsync("/api/notes");

        // Assert
        response.EnsureSuccessStatusCode();
        var notes = await response.Content.ReadFromJsonAsync<List<Note>>();
        Assert.NotNull(notes);
        Assert.True(notes.Count >= 2);
    }

    // ── GET /api/notes/{id} ───────────────────────────────────

    [Fact]
    public async Task GetNoteById_ReturnsNote_WhenExists()
    {
        // Arrange
        var created = await CreateTestNote("Find Me", "Some content");

        // Act
        var response = await _client.GetAsync($"/api/notes/{created.Id}");

        // Assert
        response.EnsureSuccessStatusCode();
        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        Assert.Equal("Find Me", note.Title);
        Assert.Equal("Some content", note.Content);
    }

    [Fact]
    public async Task GetNoteById_ReturnsNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/api/notes/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── POST /api/notes ───────────────────────────────────────

    [Fact]
    public async Task CreateNote_ReturnsCreated_WithValidData()
    {
        // Act
        var response = await _client.PostAsJsonAsync("/api/notes", new
        {
            title = "My New Note",
            content = "Hello world"
        });

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        Assert.Equal("My New Note", note.Title);
        Assert.Equal("Hello world", note.Content);
        Assert.True(note.Id > 0);
        Assert.True(note.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateNote_SetsCreatedAtToUtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var response = await _client.PostAsJsonAsync("/api/notes", new
        {
            title = "Time Test",
            content = ""
        });

        var after = DateTime.UtcNow.AddSeconds(1);

        // Assert
        response.EnsureSuccessStatusCode();
        var note = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(note);
        Assert.InRange(note.CreatedAt, before, after);
    }

    // ── PUT /api/notes/{id} ───────────────────────────────────

    [Fact]
    public async Task UpdateNote_ReturnsOk_WithUpdatedData()
    {
        // Arrange
        var created = await CreateTestNote("Original Title", "Original Content");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/notes/{created.Id}", new
        {
            title = "Updated Title",
            content = "Updated Content"
        });

        // Assert
        response.EnsureSuccessStatusCode();
        var updated = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal("Updated Content", updated.Content);
    }

    [Fact]
    public async Task UpdateNote_ReturnsNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.PutAsJsonAsync("/api/notes/99999", new
        {
            title = "Nope",
            content = "Nope"
        });

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateNote_PersistsChanges()
    {
        // Arrange
        var created = await CreateTestNote("Before", "Before content");

        // Act
        await _client.PutAsJsonAsync($"/api/notes/{created.Id}", new
        {
            title = "After",
            content = "After content"
        });

        // Assert – re-fetch to verify persistence
        var response = await _client.GetAsync($"/api/notes/{created.Id}");
        response.EnsureSuccessStatusCode();
        var fetched = await response.Content.ReadFromJsonAsync<Note>();
        Assert.NotNull(fetched);
        Assert.Equal("After", fetched.Title);
        Assert.Equal("After content", fetched.Content);
    }

    // ── DELETE /api/notes/{id} ────────────────────────────────

    [Fact]
    public async Task DeleteNote_ReturnsNoContent_WhenExists()
    {
        // Arrange
        var created = await CreateTestNote("Delete Me");

        // Act
        var response = await _client.DeleteAsync($"/api/notes/{created.Id}");

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNote_ReturnsNotFound_WhenDoesNotExist()
    {
        // Act
        var response = await _client.DeleteAsync("/api/notes/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteNote_RemovesNoteFromDatabase()
    {
        // Arrange
        var created = await CreateTestNote("Gone Soon");

        // Act
        await _client.DeleteAsync($"/api/notes/{created.Id}");

        // Assert – verify it's gone
        var response = await _client.GetAsync($"/api/notes/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
