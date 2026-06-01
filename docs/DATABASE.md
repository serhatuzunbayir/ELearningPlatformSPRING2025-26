# Database

- **Engine:** SQLite  
- **File:** `learning_platform.db` at the **repository root** (next to `learning_platform.sln`)  
- **Access:** Only through `LearningPlatform.API` (EF Core). Web and Desktop clients call the API.

## Path resolution

The API resolves the database path in [`LearningPlatform.API/Data/DatabasePathHelper.cs`](../LearningPlatform.API/Data/DatabasePathHelper.cs) as:

`{API project folder}/../learning_platform.db`

No manual connection string edits are required when cloning the repo on another computer.

## Schema script

See [`schema.sql`](schema.sql) for CREATE TABLE statements used in the project report.

## Migrations

EF Core migrations live in `LearningPlatform.API/Migrations/`. On startup, `Program.cs` runs `Database.Migrate()`.
