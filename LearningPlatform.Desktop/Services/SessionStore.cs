using System.Text.Json;
using LearningPlatform.Desktop.Models;

namespace LearningPlatform.Desktop.Services;

public class SessionStore
{
    private readonly string _sessionPath;

    public SessionStore()
    {
        var root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LearningPlatformDesktop");
        Directory.CreateDirectory(root);
        _sessionPath = Path.Combine(root, "session.json");
    }

    public void Save(UserSession session)
    {
        var json = JsonSerializer.Serialize(session);
        File.WriteAllText(_sessionPath, json);
    }

    public UserSession? Load()
    {
        if (!File.Exists(_sessionPath))
            return null;

        var json = File.ReadAllText(_sessionPath);
        return JsonSerializer.Deserialize<UserSession>(json);
    }

    public void Clear()
    {
        if (File.Exists(_sessionPath))
            File.Delete(_sessionPath);
    }
}
