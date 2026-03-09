import { useState, useEffect } from 'react';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

export default function App() {
    const [notes, setNotes] = useState([]);
    const [title, setTitle] = useState('');
    const [content, setContent] = useState('');
    const [loading, setLoading] = useState(true);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState(null);
    const [deletingId, setDeletingId] = useState(null);
    const [editingId, setEditingId] = useState(null);
    const [editTitle, setEditTitle] = useState('');
    const [editContent, setEditContent] = useState('');
    const [updating, setUpdating] = useState(false);

    // ── Fetch all notes ──────────────────────────────────────
    const fetchNotes = async () => {
        try {
            setLoading(true);
            const res = await fetch(`${API_URL}/api/notes`);
            if (!res.ok) throw new Error('Failed to fetch notes');
            const data = await res.json();
            setNotes(data);
            setError(null);
        } catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchNotes();
    }, []);

    // ── Create a note ────────────────────────────────────────
    const handleCreate = async (e) => {
        e.preventDefault();
        if (!title.trim()) return;

        try {
            setSaving(true);
            const res = await fetch(`${API_URL}/api/notes`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ title: title.trim(), content: content.trim() }),
            });
            if (!res.ok) throw new Error('Failed to create note');
            setTitle('');
            setContent('');
            await fetchNotes();
        } catch (err) {
            setError(err.message);
        } finally {
            setSaving(false);
        }
    };

    // ── Delete a note ────────────────────────────────────────
    const handleDelete = async (id) => {
        try {
            setDeletingId(id);
            const res = await fetch(`${API_URL}/api/notes/${id}`, { method: 'DELETE' });
            if (!res.ok) throw new Error('Failed to delete note');
            await fetchNotes();
        } catch (err) {
            setError(err.message);
        } finally {
            setDeletingId(null);
        }
    };

    // ── Start editing a note ────────────────────────────────
    const startEditing = (note) => {
        setEditingId(note.id);
        setEditTitle(note.title);
        setEditContent(note.content || '');
    };

    const cancelEditing = () => {
        setEditingId(null);
        setEditTitle('');
        setEditContent('');
    };

    // ── Update a note ───────────────────────────────────────
    const handleUpdate = async (id) => {
        if (!editTitle.trim()) return;
        try {
            setUpdating(true);
            const res = await fetch(`${API_URL}/api/notes/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ title: editTitle.trim(), content: editContent.trim() }),
            });
            if (!res.ok) throw new Error('Failed to update note');
            setEditingId(null);
            await fetchNotes();
        } catch (err) {
            setError(err.message);
        } finally {
            setUpdating(false);
        }
    };

    // ── Format date ──────────────────────────────────────────
    const formatDate = (dateStr) => {
        const d = new Date(dateStr);
        return d.toLocaleDateString('en-US', {
            month: 'short',
            day: 'numeric',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit',
        });
    };

    return (
        <div className="app">
            {/* ── Ambient blobs ────────────────────────────────── */}
            <div className="blob blob-1" aria-hidden="true"></div>
            <div className="blob blob-2" aria-hidden="true"></div>
            <div className="blob blob-3" aria-hidden="true"></div>

            {/* ── Header ───────────────────────────────────────── */}
            <header className="header">
                <div className="logo">
                    <span className="logo-icon">📝</span>
                    <h1>Quick Notes</h1>
                </div>
                <p className="tagline">Capture thoughts at the speed of light!</p>
            </header>

            <main className="main">
                {/* ── Create form ──────────────────────────────── */}
                <section className="card create-card" id="create-note-section">
                    <h2 className="section-title">New Note</h2>
                    <form onSubmit={handleCreate} className="form" id="create-note-form">
                        <div className="input-group">
                            <input
                                id="note-title-input"
                                type="text"
                                placeholder="Title"
                                value={title}
                                onChange={(e) => setTitle(e.target.value)}
                                className="input"
                                required
                            />
                        </div>
                        <div className="input-group">
                            <textarea
                                id="note-content-input"
                                placeholder="Write your thoughts here…"
                                value={content}
                                onChange={(e) => setContent(e.target.value)}
                                className="textarea"
                                rows={4}
                            />
                        </div>
                        <button
                            id="create-note-button"
                            type="submit"
                            className="btn btn-primary"
                            disabled={saving || !title.trim()}
                        >
                            {saving ? (
                                <>
                                    <span className="spinner"></span> Saving…
                                </>
                            ) : (
                                <>
                                    <span className="btn-icon">+</span> Add Note
                                </>
                            )}
                        </button>
                    </form>
                </section>

                {/* ── Error banner ─────────────────────────────── */}
                {error && (
                    <div className="error-banner" id="error-banner">
                        <span>⚠️ {error}</span>
                        <button className="error-dismiss" onClick={() => setError(null)}>×</button>
                    </div>
                )}

                {/* ── Notes list ───────────────────────────────── */}
                <section className="notes-section" id="notes-list-section">
                    <h2 className="section-title">
                        Your Notes
                        {!loading && <span className="badge">{notes.length}</span>}
                    </h2>

                    {loading ? (
                        <div className="loading" id="notes-loading">
                            <div className="loading-dots">
                                <span></span><span></span><span></span>
                            </div>
                            <p>Loading notes…</p>
                        </div>
                    ) : notes.length === 0 ? (
                        <div className="empty" id="notes-empty">
                            <span className="empty-icon">🗒️</span>
                            <p>No notes yet. Create your first one!</p>
                        </div>
                    ) : (
                        <div className="notes-grid" id="notes-grid">
                            {notes.map((note) => (
                                <article key={note.id} className={`card note-card ${editingId === note.id ? 'note-card-editing' : ''}`} id={`note-card-${note.id}`}>
                                    {editingId === note.id ? (
                                        /* ── Inline Edit Mode ──── */
                                        <div className="edit-form" id={`edit-form-${note.id}`}>
                                            <input
                                                id={`edit-title-${note.id}`}
                                                type="text"
                                                className="input"
                                                value={editTitle}
                                                onChange={(e) => setEditTitle(e.target.value)}
                                                placeholder="Title"
                                                autoFocus
                                            />
                                            <textarea
                                                id={`edit-content-${note.id}`}
                                                className="textarea"
                                                value={editContent}
                                                onChange={(e) => setEditContent(e.target.value)}
                                                placeholder="Content"
                                                rows={3}
                                            />
                                            <div className="edit-actions">
                                                <button
                                                    className="btn btn-save"
                                                    id={`save-note-${note.id}`}
                                                    onClick={() => handleUpdate(note.id)}
                                                    disabled={updating || !editTitle.trim()}
                                                >
                                                    {updating ? (
                                                        <><span className="spinner spinner-sm"></span> Saving…</>
                                                    ) : (
                                                        '✓ Save'
                                                    )}
                                                </button>
                                                <button
                                                    className="btn btn-cancel"
                                                    id={`cancel-edit-${note.id}`}
                                                    onClick={cancelEditing}
                                                    disabled={updating}
                                                >
                                                    ✕ Cancel
                                                </button>
                                            </div>
                                        </div>
                                    ) : (
                                        /* ── Display Mode ──── */
                                        <>
                                            <div className="note-header">
                                                <h3 className="note-title">{note.title}</h3>
                                                <div className="note-actions">
                                                    <button
                                                        className="btn btn-edit"
                                                        id={`edit-note-${note.id}`}
                                                        onClick={() => startEditing(note)}
                                                        title="Edit note"
                                                    >
                                                        ✏️
                                                    </button>
                                                    <button
                                                        className="btn btn-delete"
                                                        id={`delete-note-${note.id}`}
                                                        onClick={() => handleDelete(note.id)}
                                                        disabled={deletingId === note.id}
                                                        title="Delete note"
                                                    >
                                                        {deletingId === note.id ? (
                                                            <span className="spinner spinner-sm"></span>
                                                        ) : (
                                                            '🗑️'
                                                        )}
                                                    </button>
                                                </div>
                                            </div>
                                            {note.content && <p className="note-content">{note.content}</p>}
                                            <time className="note-date">{formatDate(note.createdAt)}</time>
                                        </>
                                    )}
                                </article>
                            ))}
                        </div>
                    )}
                </section>
            </main>

            <footer className="footer">
                <p>QuickNotes &copy; {new Date().getFullYear()}</p>
            </footer>
        </div>
    );
}
