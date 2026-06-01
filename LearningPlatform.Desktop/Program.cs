using System.Text.Json;
using LearningPlatform.Desktop.Forms;
using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;

namespace LearningPlatform.Desktop;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var apiClient = new ApiClient(ResolveApiBaseUrl());
        var sessionStore = new SessionStore();

        while (true)
        {
            var session = sessionStore.Load();
            if (session is null)
            {
                using var login = new LoginForm(apiClient, sessionStore);
                if (login.ShowDialog() != DialogResult.OK || login.Session is null)
                {
                    return;
                }
                session = login.Session;
            }

            apiClient.SetToken(session.Token);
            using var shell = new Form1(apiClient, sessionStore, session);
            Application.Run(shell);

            if (!shell.ShouldReturnToLogin)
            {
                return;
            }
        }
    }

    private static string ResolveApiBaseUrl()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            if (!File.Exists(path)) return "http://localhost:5215";

            using var doc = JsonDocument.Parse(File.ReadAllText(path));
            if (doc.RootElement.TryGetProperty("ApiSettings", out var api) &&
                api.TryGetProperty("BaseUrl", out var url))
            {
                var value = url.GetString();
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }
        }
        catch
        {
            // fall back to default local API
        }

        return "http://localhost:5215";
    }
}