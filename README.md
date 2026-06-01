# E-Learning Platform (SE 410 — Group 7)

ASP.NET Core Web API + SQLite, **Windows Forms desktop** (admin-focused), and **ASP.NET MVC web** (student-focused). Both clients share one database file via the API.

## Requirements

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- **Windows** for `LearningPlatform.Desktop` (WinForms)
- macOS/Linux: API + web MVC only

## Database

Single SQLite file: `learning_platform.db` at the repository root. Created/updated automatically when the API starts. See [docs/DATABASE.md](docs/DATABASE.md) and [docs/schema.sql](docs/schema.sql).

## How to run

### 1. API (required first)

```bash
cd LearningPlatform.API
dotnet restore
dotnet run
```

Runs at: http://localhost:5215

### 2. Desktop (Windows)

```bash
cd LearningPlatform.Desktop
dotnet restore
dotnet run
```

### 3. Web (optional, any OS)

```bash
cd learning_platform
dotnet restore
dotnet run
```

Runs at: http://localhost:5084

### Default admin

- Email: `admin@platform.com`
- Password: `Admin1234!`

## Solution structure

| Project | Role |
|---------|------|
| `LearningPlatform.API` | Backend, EF Core, JWT, business logic |
| `LearningPlatform.Desktop` | WinForms UI (admin + student flows) |
| `learning_platform` | MVC web UI (admin + student flows) |

## Releases

- **V1:** Initial desktop + API features  
- **V2:** Portable DB, module management, admin analytics, 2FA, unit tests (see GitHub Releases)
