# QuickNotes 📝

A full-stack Notes application built with **.NET 10 Web API**, **React (Vite)**, and **PostgreSQL**, fully containerized with **Docker**.

---

## Architecture

```
┌────────────┐       ┌────────────┐       ┌────────────┐
│  Frontend   │──────▶│  Backend   │──────▶│ PostgreSQL │
│ React/Nginx │ :3000 │ .NET API   │ :5000 │            │ :5432
└────────────┘       └────────────┘       └────────────┘
```

| Service    | Technology          | Port |
|------------|---------------------|------|
| Frontend   | React + Vite + Nginx | 3000 |
| Backend    | .NET 10 Minimal API | 5000 |
| Database   | PostgreSQL 16       | 5432 |

---

## Quick Start

### Prerequisites

- [Docker](https://docs.docker.com/get-docker/) & [Docker Compose](https://docs.docker.com/compose/install/)

### Run

```bash
docker compose up --build
```

That's it! The entire stack will start up automatically.

### Access

| What       | URL                          |
|------------|------------------------------|
| Frontend   | http://localhost:3000         |
| Backend API| http://localhost:5000/api/notes |
| PostgreSQL | localhost:5432               |

---

## API Endpoints

| Method | Endpoint           | Description       |
|--------|--------------------|--------------------|
| GET    | `/api/notes`       | List all notes     |
| GET    | `/api/notes/{id}`  | Get a note by ID   |
| POST   | `/api/notes`       | Create a new note  |
| PUT    | `/api/notes/{id}`  | Update a note      |
| DELETE | `/api/notes/{id}`  | Delete a note      |

### Example: Create a Note

```bash
curl -X POST http://localhost:5000/api/notes \
  -H "Content-Type: application/json" \
  -d '{"title": "Hello", "content": "My first note!"}'
```

---

## Project Structure

```
quicknotes/
├── backend/
│   ├── Data/
│   │   └── AppDbContext.cs
│   ├── Models/
│   │   └── Note.cs
│   ├── Program.cs
│   ├── Backend.csproj
│   ├── appsettings.json
│   ├── Dockerfile
│   └── .dockerignore
├── frontend/
│   ├── src/
│   │   ├── App.jsx
│   │   ├── index.css
│   │   └── main.jsx
│   ├── index.html
│   ├── package.json
│   ├── vite.config.js
│   ├── nginx.conf
│   ├── Dockerfile
│   └── .dockerignore
├── docker-compose.yml
└── README.md
```

---

## Environment Variables

### Backend

| Variable            | Description                     | Default |
|---------------------|---------------------------------|---------|
| `CONNECTION_STRING` | PostgreSQL connection string    | Set in docker-compose.yml |

### Frontend

| Variable        | Description             | Default                  |
|-----------------|-------------------------|--------------------------|
| `VITE_API_URL`  | Backend API base URL    | `http://localhost:5000`  |

---

## Stopping the App

```bash
docker compose down
```

To also remove the database volume:

```bash
docker compose down -v
```

---

## Tech Stack

- **Backend**: .NET 10 · Entity Framework Core · Npgsql
- **Frontend**: React 19 · Vite 6
- **Database**: PostgreSQL 16
- **Containerization**: Docker · Docker Compose
