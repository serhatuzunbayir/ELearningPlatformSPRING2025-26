namespace LearningPlatform.API.Data;

/// <summary>
/// Resolves the shared SQLite file at the solution root so the path works on any machine
/// without editing appsettings (relative to the API project folder: ../learning_platform.db).
/// </summary>
public static class DatabasePathHelper
{
    public static string GetDatabaseFilePath(string contentRootPath)
    {
        var path = Path.GetFullPath(Path.Combine(contentRootPath, "..", "learning_platform.db"));
        return path;
    }

    public static string BuildConnectionString(string contentRootPath) =>
        $"Data Source={GetDatabaseFilePath(contentRootPath)}";
}
