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

        var apiClient = new ApiClient("http://localhost:5215");
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
}