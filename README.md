# Project Task Manager API

REST API for managing projects and tasks. .NET 9, Clean Architecture, CQRS, JWT authentication with refresh token rotation, EF Core 9 with SQL Server.

## Tech Stack

- .NET 9
- ASP.NET Core Web API
- EF Core 9
- SQL Server
- ASP.NET Core Identity
- JWT Bearer
- MediatR 12.5.0 (last MIT-licensed version)
- FluentValidation 11
- Mapster
- Serilog (Console + rolling file)
- Asp.Versioning 8
- Swashbuckle
- xUnit + FluentAssertions + EF Core InMemory
- Docker + docker-compose

## Architecture

Standard Clean Architecture with four source projects under `src/` plus a test project under `tests/`. Dependencies flow inward only.

- **Domain** — entities, enums, constants. Zero dependencies.
- **Application** — CQRS commands and queries, handlers, validators, the `Result<T>` pattern, pagination types (`PagedRequest`, `PagedResult<T>`), and abstractions (`IApplicationDbContext`, `IUserContext`, `IIdentityService`, `IJwtTokenService`). References Domain only.
- **Infrastructure** — EF Core `ApplicationDbContext`, entity configurations with explicit indexes, ASP.NET Core Identity, JWT token service, `IIdentityService` implementation. References Application.
- **API** — controllers, middleware (`GlobalExceptionHandler` via `IExceptionHandler`), `Result`→`IActionResult` mapping, `IUserContext` implementation (reads `HttpContext`), DI wiring, `Program.cs`. References Application and Infrastructure.

Notable design decisions:

- No Repository or Unit of Work abstraction. `DbContext` is already a UoW; `DbSet<T>` is already a repository. Handlers inject `IApplicationDbContext` and write LINQ directly, keeping all EF Core capabilities available.
- `Result<T>` pattern for control flow. Exceptions are reserved for unexpected failures handled by `GlobalExceptionHandler` middleware.
- Authorization is enforced in handlers, not just controller attributes. Ownership checks are folded into `WHERE` clauses for single-query authorization.
- Enum properties (`TaskItemStatus`, `TaskPriority`) are stored as strings in the database via `HasConversion<string>()` — readable SQL, indexable filters.
- Refresh tokens rotate on every use; the previous token is invalidated.
- Non-owners requesting a resource get 404 Not Found, not 403 Forbidden — prevents resource enumeration.

## Project Structure

```
ProjectTaskManager/
├── src/
│   ├── ProjectTaskManager.API/
│   │   ├── Authentication/
│   │   ├── Controllers/
│   │   ├── Extensions/
│   │   ├── Middleware/
│   │   └── Properties/
│   ├── ProjectTaskManager.Application/
│   │   ├── Common/
│   │   └── Features/
│   ├── ProjectTaskManager.Domain/
│   │   ├── Constants/
│   │   ├── Entities/
│   │   └── Enums/
│   └── ProjectTaskManager.Infrastructure/
│       ├── Authentication/
│       ├── Identity/
│       └── Persistence/
├── tests/
│   └── ProjectTaskManager.Application.UnitTests/
│       ├── Common/
│       ├── Features/
│       └── Infrastructure/
├── docker-compose.yml
├── Dockerfile
├── ProjectTaskManager.postman_collection.json
└── ProjectTaskManager.sln
```

## Prerequisites

**Development mode (no Docker):**
- .NET 9 SDK
- SQL Server LocalDB (included with Visual Studio 2022) or a local SQL Server / SQL Express instance

**Docker mode:**
- Docker Desktop (Windows/Mac) or Docker Engine + Compose v2 (Linux)

## Running the API

### Mode A — Development (no Docker required)

Uses LocalDB with Windows Authentication. Migrations and role seeding run automatically on first startup.

```
cd src/ProjectTaskManager.API
dotnet run
```

Or open `ProjectTaskManager.sln` in Visual Studio, set the API project as startup, and press F5.

API runs at:
- `http://localhost:5159`
- `https://localhost:7239`

Swagger UI: `https://localhost:7239/swagger`

If you have full SQL Server instead of LocalDB, edit `src/ProjectTaskManager.API/appsettings.Development.json` and change the `DefaultConnection` Server to your instance (e.g. `Server=localhost;` or `Server=.\SQLEXPRESS;`).

### Mode B — Docker (full stack)

The API and SQL Server both run in containers. Secrets are read from a gitignored `.env` file.

```
cp .env.example .env
```

Open `.env` and set values. Required:
- `SQL_SA_PASSWORD` — strong password (8+ chars, upper, lower, digit, symbol)
- `JWT_SECRET` — 32+ characters; generate with `openssl rand -base64 48`

Then:

```
docker compose up --build
```

API at `http://localhost:8080/swagger`. SQL Server at `localhost:1433`.

Stop: `docker compose down`. To wipe the database volume: `docker compose down -v`.

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| POST | /api/v1/Auth/register | Register new user, return tokens |
| POST | /api/v1/Auth/login | Authenticate, return tokens |
| POST | /api/v1/Auth/refresh | Rotate refresh token, return new tokens |
| GET | /api/v1/Projects | List projects (paginated, filtered, sorted) for current user |
| GET | /api/v1/Projects/{id} | Get project by id (owner or admin) |
| POST | /api/v1/Projects | Create project for current user |
| PUT | /api/v1/Projects/{id} | Update project (owner or admin) |
| DELETE | /api/v1/Projects/{id} | Delete project (owner or admin, cascades to tasks) |
| POST | /api/v1/projects/{projectId}/tasks | Create task in a project |
| GET | /api/v1/projects/{projectId}/tasks | List tasks in a project (paginated, filtered) |
| PATCH | /api/v1/projects/{projectId}/tasks/{taskId}/status | Update task status only |
| DELETE | /api/v1/projects/{projectId}/tasks/{taskId} | Delete task |

## Query Parameters (Pagination, Filtering, Sorting)

Common parameters accepted by all list endpoints:

| Parameter | Default | Notes |
|-----------|---------|-------|
| pageNumber | 1 | Min 1 |
| pageSize | 10 | Min 1, max 100 |
| sortBy | — | See per-endpoint values below |
| sortDirection | desc | `asc` or `desc` |
| search | — | Case-insensitive partial match on name/title |

**GET /api/v1/Projects** — `sortBy` values: `name`, `createdAt`

**GET /api/v1/projects/{projectId}/tasks** — `sortBy` values: `title`, `priority`, `dueDate`, `createdAt`

Additional task filters:

| Parameter | Type | Description |
|-----------|------|-------------|
| status | string | `Todo`, `InProgress`, `Done`, `Cancelled` |
| priority | string | `Low`, `Medium`, `High`, `Critical` |
| dueBefore | DateTime | ISO 8601 |
| dueAfter | DateTime | ISO 8601 |

All filtering, sorting, and pagination happens in SQL.

`PagedResult<T>` response shape:

```json
{
  "items": [],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 47,
  "totalPages": 5,
  "hasPrevious": false,
  "hasNext": true
}
```

## Authentication

1. POST `/api/v1/Auth/register` or `/api/v1/Auth/login` — returns:
   ```json
   {
     "accessToken": "...",
     "refreshToken": "...",
     "accessTokenExpiresAt": "2026-05-22T00:15:00Z",
     "refreshTokenExpiresAt": "2026-05-29T00:00:00Z"
   }
   ```
2. Send subsequent requests with `Authorization: Bearer <accessToken>`.
3. When the access token expires (15 minutes by default), POST `/api/v1/Auth/refresh` with the current refresh token to get a new pair.
4. Refresh tokens rotate — each refresh invalidates the previous refresh token.
5. Refresh tokens expire after 7 days by default.

## Roles

Two roles are seeded automatically on startup: `User` (assigned to all new registrations) and `Admin` (must be assigned manually). Admins see and modify all projects and tasks; regular users see only their own.

To grant Admin to a user in development:

```sql
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM AspNetUsers u, AspNetRoles r
WHERE u.Email = 'user@example.com' AND r.Name = 'Admin';
```

## Postman Collection

`ProjectTaskManager.postman_collection.json` at the repo root. Import into Postman.

- Variables: `baseUrl`, `accessToken`, `refreshToken`, `projectId`, `taskId`.
- Login and Register automatically capture tokens into collection variables via post-response script.
- Create Project captures the new project id; Create Task captures the new task id.
- Bearer auth is applied at the collection level; auth endpoints override to no-auth.

Default `baseUrl` is `http://localhost:8080` (Docker mode). For development mode, change it to `https://localhost:7239`.

## Testing

```
dotnet test
```

Runs the xUnit suite: 34 tests across handlers (authorization patterns, query filtering, command execution) and the JWT token service.

## Database

The initial EF migration is committed under `src/ProjectTaskManager.Infrastructure/Persistence/Migrations/`. On every startup the application:

1. Applies any pending migrations (`Database.MigrateAsync`).
2. Idempotently seeds the `User` and `Admin` roles.

No manual `dotnet ef database update` step is required.

## Repository Conventions

Branches: `master` (release-only), `develop` (integration), feature branches per change (e.g. `feat/projects-feature`, `fix/environment-configuration`). Merges into develop use `--no-ff` to preserve feature history. Commits follow Conventional Commits (`feat`, `fix`, `chore`, `docs`, `test`, `build`).
