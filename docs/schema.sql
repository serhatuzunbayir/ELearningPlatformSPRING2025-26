-- E-Learning Platform — SQLite schema (reference for SE 410 report)
-- Generated from EF Core migration InitialCreate. Runtime DB file: learning_platform.db (repository root)

PRAGMA foreign_keys = ON;

CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Email TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    Role INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL,
    PreferredCategory TEXT,
    TwoFactorEnabled INTEGER NOT NULL DEFAULT 0,
    TwoFactorCode TEXT,
    TwoFactorCodeExpiry TEXT
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Email ON Users (Email);

CREATE TABLE IF NOT EXISTS Courses (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT NOT NULL,
    Description TEXT NOT NULL,
    Category TEXT NOT NULL,
    Difficulty INTEGER NOT NULL,
    EctsCredit INTEGER NOT NULL,
    CreatedAt TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS CourseModules (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    CourseId INTEGER NOT NULL,
    Title TEXT NOT NULL,
    Content TEXT NOT NULL,
    "Order" INTEGER NOT NULL,
    FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE
);
CREATE INDEX IF NOT EXISTS IX_CourseModules_CourseId ON CourseModules (CourseId);

CREATE TABLE IF NOT EXISTS Enrollments (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    CourseId INTEGER NOT NULL,
    Status INTEGER NOT NULL,
    RequestedAt TEXT NOT NULL,
    ApprovedAt TEXT,
    FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_Enrollments_UserId_CourseId ON Enrollments (UserId, CourseId);

CREATE TABLE IF NOT EXISTS ModuleCompletions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    ModuleId INTEGER NOT NULL,
    CompletedAt TEXT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE,
    FOREIGN KEY (ModuleId) REFERENCES CourseModules (Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_ModuleCompletions_UserId_ModuleId ON ModuleCompletions (UserId, ModuleId);

CREATE TABLE IF NOT EXISTS StudyPlans (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    UserId INTEGER NOT NULL,
    GeneratedAt TEXT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users (Id) ON DELETE CASCADE
);
CREATE UNIQUE INDEX IF NOT EXISTS IX_StudyPlans_UserId ON StudyPlans (UserId);

CREATE TABLE IF NOT EXISTS StudyPlanItems (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    StudyPlanId INTEGER NOT NULL,
    CourseId INTEGER NOT NULL,
    ScheduledDay TEXT NOT NULL,
    RecommendedHours INTEGER NOT NULL,
    FOREIGN KEY (StudyPlanId) REFERENCES StudyPlans (Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses (Id) ON DELETE CASCADE
);

-- Example queries for report
-- SELECT * FROM Courses;
-- SELECT u.Name, c.Title, e.Status FROM Enrollments e JOIN Users u ON e.UserId = u.Id JOIN Courses c ON e.CourseId = c.Id;
