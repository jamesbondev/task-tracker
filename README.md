# TaskTracker API

A minimal .NET 8 REST API for managing tasks. Built as a test target for an AI DevOps agent that autonomously picks up work items, creates feature branches, and implements changes.

## Project Structure

```
task-tracker/
├── TaskTracker.sln
├── Directory.Build.props
├── work-items.md
├── src/
│   └── TaskTracker.Api/          # Web API project
│       ├── Models/               # TaskItem, TaskItemStatus
│       ├── Data/                 # ITaskRepository, InMemoryTaskRepository
│       └── Endpoints/            # Minimal API endpoint mappings
└── tests/
    └── TaskTracker.Api.Tests/    # xUnit + FluentAssertions
```

## Getting Started

```bash
# Build
dotnet build

# Run
dotnet run --project src/TaskTracker.Api

# Test
dotnet test
```

The API runs on `http://localhost:5000` by default.

## API Endpoints

| Method | Route              | Description         |
|--------|--------------------|---------------------|
| GET    | /api/tasks         | List all tasks      |
| GET    | /api/tasks/{id}    | Get task by ID      |
| POST   | /api/tasks         | Create a new task   |
| PUT    | /api/tasks/{id}    | Update a task       |
| DELETE | /api/tasks/{id}    | Delete a task       |

## Data Model

Tasks are stored in-memory using a `ConcurrentDictionary`. The store is seeded with 3 sample tasks on startup. Data does not persist across restarts.

```json
{
  "id": 1,
  "title": "Example task",
  "description": "Optional description",
  "status": "Todo",
  "createdAt": "2026-02-06T12:00:00Z",
  "updatedAt": null
}
```

**Status values:** `Todo`, `InProgress`, `Done`

## Work Items

See [work-items.md](work-items.md) for a list of scoped changes designed to be implemented by an AI agent. Each work item includes a title, description, and acceptance criteria.

## Tech Stack

- .NET 8 / ASP.NET Core Minimal APIs
- xUnit + FluentAssertions
- In-memory storage (no database)
