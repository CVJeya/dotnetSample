# Task Management System

Enterprise-style **Task Management** reference implementation for technical assignments: **Angular 17** + **.NET 8 Web API** + **SQL Server**, with **layered architecture**, **DTOs**, **repository & service patterns**, **global exception handling**, and **Swagger** documentation.

---

## Features

- Full **CRUD** for tasks (create, list, read one, update, delete)
- **Search** across title, description, status, and remarks (`GET /api/tasks/search?keyword=`)
- **Validation** on DTOs (DataAnnotations) and Angular reactive forms
- **Audit columns**: `CreatedOn`, `LastUpdatedOn`, `CreatedBy`, `LastUpdatedBy`
- **Responsive** Bootstrap 5 UI (table list + cards on detail)
- **Loading** states, **empty** states, **toast** notifications (ngx-toastr)
- **Lazy-loaded** feature routes (standalone components)

---

## Tech Stack

| Area | Technology |
|------|------------|
| Frontend | Angular 17 (standalone), TypeScript, Bootstrap 5, RxJS |
| Backend | ASP.NET Core 8 Web API, AutoMapper |
| Data | Entity Framework Core 8, SQL Server / LocalDB |
| Tooling | Swagger / OpenAPI, EF Core CLI |

---

## Architecture

Layered flow (clean separation of concerns):

```
Angular SPA (HttpClient)
        ↓ JSON over HTTP
Controllers (HTTP + validation surface)
        ↓
Service layer (business rules, mapping orchestration)
        ↓
Repository layer (EF Core queries / persistence)
        ↓
ApplicationDbContext → SQL Server
```

**Why this shape:** controllers stay thin, persistence is replaceable behind `ITaskRepository`, and API contracts stay stable via DTOs even when the schema evolves.

**Why Code First + EF Core:** migrations live in source control, schema changes are reviewable, and environments stay aligned (dev/stage/prod). EF Core translates LINQ to parameterized SQL, reducing typical SQL injection risk versus string concatenation.

---

## Database Design

### Table: `Tasks`

| Column | Type | Notes |
|--------|------|-------|
| `Id` | `int` IDENTITY | Primary key |
| `TaskTitle` | `nvarchar(200)` | Required |
| `TaskDescription` | `nvarchar(2000)` | Optional |
| `TaskDueDate` | `datetime2` | Required |
| `TaskStatus` | `nvarchar(50)` | `Pending`, `InProgress`, `Completed`, `Cancelled` |
| `TaskRemarks` | `nvarchar(1000)` | Optional |
| `CreatedOn` / `LastUpdatedOn` | `datetime2` | Defaults via DB / app |
| `CreatedBy` / `LastUpdatedBy` | `nvarchar(100)` | Audit trail |

**Indexes:** non-clustered indexes on `TaskTitle` and `TaskStatus` to support search/filter paths.

### ER Diagram (Conceptual)

Single-entity model for the assignment scope:

```
┌──────────── Tasks ────────────┐
│ PK Id                         │
│    TaskTitle, TaskDescription   │
│    TaskDueDate, TaskStatus      │
│    TaskRemarks                  │
│    CreatedOn, LastUpdatedOn     │
│    CreatedBy, LastUpdatedBy     │
└───────────────────────────────┘
```

---

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/tasks` | List all tasks (newest first) |
| `GET` | `/api/tasks/{id}` | Task details |
| `GET` | `/api/tasks/search?keyword=` | Keyword search (empty returns all) |
| `POST` | `/api/tasks` | Create (`CreateTaskDto`) |
| `PUT` | `/api/tasks/{id}` | Update (`UpdateTaskDto`) |
| `DELETE` | `/api/tasks/{id}` | Delete |

Interactive docs: **Swagger UI** (Development) at `/swagger`.

See also: `docs/API.md` for example payloads.

---

## Repository Layout

```
TaskManagement/
├── TaskManagement.sln
├── dotnet-tools.json             # Local dotnet-ef tool version (optional)
├── .github/workflows/ci.yml      # GitHub Actions CI (API + Angular build)
├── TaskManagement.API/          # Web API
│   ├── Controllers/
│   ├── Services/
│   ├── Repositories/
│   ├── DTOs/
│   ├── Models/
│   ├── Data/
│   │   └── Migrations/           # EF Core migrations (InitialCreate)
│   ├── Middleware/
│   ├── Helpers/
│   └── Mappings/
├── TaskManagementUI/             # Angular SPA
│   └── src/app/
│       ├── components/
│       ├── services/
│       ├── models/
│       ├── shared/
│       ├── helpers/
│       └── environments/
├── Database/Scripts/             # SQL bootstrap: SQL Server (*.sql) + MySQL (*_MySQL.sql)
├── docs/
│   ├── API.md
│   └── screenshots/              # UI screenshots for README
└── README.md
```

### Angular Routes

| Route | Screen |
|-------|--------|
| `/tasks` | List + search |
| `/tasks/create` | Create |
| `/tasks/edit/:id` | Edit |
| `/tasks/details/:id` | Details |

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/) and npm
- SQL Server, **SQL Server Express**, or **LocalDB** (connection string targets LocalDB by default)

---

## Setup & Run (Backend)

```powershell
cd "d:\Task Management\TaskManagement.API"
dotnet restore
dotnet build
```

### Database (choose one path)

**Path A — EF Core migrations (recommended; migrations are included in source control)**

From the repository root you can use the pinned **dotnet-ef** tool:

```powershell
cd "d:\Task Management"
dotnet tool restore   # reads dotnet-tools.json
dotnet tool run dotnet-ef database update --project TaskManagement.API/TaskManagement.API.csproj --startup-project TaskManagement.API/TaskManagement.API.csproj
```

Alternatively, install `dotnet-ef` globally once (`dotnet tool install --global dotnet-ef --version 8.0.11`) and run from `TaskManagement.API`:

```powershell
cd "d:\Task Management\TaskManagement.API"
dotnet ef database update
```

This applies `Data/Migrations` (includes `Tasks` table plus `IX_Tasks_TaskTitle` / `IX_Tasks_TaskStatus`).

**Path B — SQL script (matches the EF model)**

Run `Database/Scripts/01_CreateTasks.sql` in SSMS / Azure Data Studio against your instance (use when you prefer not to run EF migrations).  
For **MySQL 8+**, use `Database/Scripts/01_CreateTasks_MySQL.sql` (and optional `02_SeedSampleData_MySQL.sql`) in MySQL Workbench or your client — the default .NET API in this repo targets **SQL Server** unless you change the EF provider.  
Optionally run `Database/Scripts/02_SeedSampleData.sql` (SQL Server) for demo rows.

> **Note:** If you already created the database using Path B, either drop `dbo.Tasks` before running Path A, or skip Path A to avoid duplicate schema work.

### Run API

```powershell
cd "d:\Task Management\TaskManagement.API"
dotnet run --launch-profile http
```

- HTTP API (default profile in `launchSettings.json`): `http://localhost:5000`
- Swagger (Development): `http://localhost:5000/swagger`

> **Note:** HTTPS redirection is **disabled in Development** so the Angular dev server can call `http://localhost:5000` without TLS friction. Use HTTPS + proper CORS origins in production.

---

## Setup & Run (Frontend)

```powershell
cd "d:\Task Management\TaskManagementUI"
npm install
npm start
```

If you see **`ERESOLVE`** / mixed Angular versions (e.g. old Angular 21 files under `node_modules`), run a clean install:

```powershell
cd "d:\Task Management\TaskManagementUI"
.\clean-install.ps1
```

Application: `http://localhost:4200`  
API base URL is configured in `src/environments/environment.ts` (`http://localhost:5000/api`).

Production build:

```powershell
npm run build
```

---

## Continuous Integration

This repo includes **GitHub Actions** (`.github/workflows/ci.yml`) that on every **push** and **pull request**:

- Restores and builds **`TaskManagement.API`** in **Release** (compiles EF migration metadata).
- Runs **`npm install`** (or **`npm ci`** when `package-lock.json` exists) and **`npm run build`** for **`TaskManagementUI`**.

Commit a `package-lock.json` after `npm install` in `TaskManagementUI` if you want reproducible installs and faster CI (`npm ci`).

---

## Configuration

| Setting | Location |
|---------|----------|
| Connection string | `TaskManagement.API/appsettings.json` → `ConnectionStrings:DefaultConnection` |
| CORS origins | `appsettings.json` → `Cors:AllowedOrigins` |
| Angular API URL | `TaskManagementUI/src/environments/environment.ts` |

---

## Screenshots

Angular UI at **`http://localhost:4200`** — Bootstrap 5 layout (navbar, tables/cards, status badges). Illustrations below are **SVG placeholders** in **`docs/screenshots/`** (replace with PNG captures from your browser if your assignment requires raster screenshots).

### Task list

Search across title, description, status, and remarks; responsive table with **View / Edit / Delete**.

![Task list](docs/screenshots/01-task-list.svg)

### Create task

Reactive form with validation; **Created by** maps to audit columns until authentication is added.

![Create task](docs/screenshots/02-create-task.svg)

### Task details

Full task metadata, audit timestamps (created / updated), description and remarks.

![Task details](docs/screenshots/03-task-details.svg)

### Edit task

Load by ID; prefilled fields; **Last updated by** for audit trail.

![Edit task](docs/screenshots/04-edit-task.svg)

### Delete confirmation

Browser confirmation before calling the delete API.

![Delete confirmation](docs/screenshots/05-delete-confirm.svg)

---

## Future Improvements

- **Authentication & authorization** (JWT / OIDC), link audit columns to real user identities
- **Paging & sorting** on list/search endpoints
- **FluentValidation** or **minimal APIs filter** for richer domain validation
- **Domain events** / **outbox** for integrations (email reminders)
- **Integration tests** (WebApplicationFactory) + **Angular component tests**
- **Containerize** with Docker Compose (SQL + API + UI); extend CI with deployment stages (Azure DevOps / GitHub Environments)
- **Observability**: structured logging, metrics, health checks (`/health`)

---

## Security & Production Notes

- Replace manual audit strings with authenticated user claims.
- Use **secrets management** (Azure Key Vault, User Secrets in dev) — never commit production connection strings.
- Enable **HTTPS**, **HSTS**, tight **CORS** policy, and an **API gateway** or **reverse proxy** in production.
- Add **rate limiting** and **input sanitization** if exposing publicly.

---

## License

Sample assignment code — use and adapt per your institution’s academic integrity rules.
