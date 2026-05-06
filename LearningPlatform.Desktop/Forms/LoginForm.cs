using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;

namespace LearningPlatform.Desktop.Forms;

public class LoginForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly SessionStore _sessionStore;
    private readonly TextBox _emailText = new() { PlaceholderText = "Email", Width = 280 };
    private readonly TextBox _passwordText = new() { PlaceholderText = "Password", Width = 280, UseSystemPasswordChar = true };
    public UserSession? Session { get; private set; }

    public LoginForm(ApiClient apiClient, SessionStore sessionStore)
    {
        _apiClient = apiClient;
        _sessionStore = sessionStore;
        Text = "Learning Platform - Login";
        Width = 380;
        Height = 230;
        StartPosition = FormStartPosition.CenterScreen;

        var loginButton = new Button { Text = "Login", Width = 120 };
        loginButton.Click += async (_, _) => await LoginAsync();
        var registerButton = new Button { Text = "Register", Width = 120 };
        registerButton.Click += (_, _) =>
        {
            using var form = new RegisterForm(_apiClient);
            form.ShowDialog(this);
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(20),
            WrapContents = false
        };
        panel.Controls.AddRange([_emailText, _passwordText, loginButton, registerButton]);
        Controls.Add(panel);
    }

    private async Task LoginAsync()
    {
        try
        {
            var dto = new LoginDto(_emailText.Text.Trim(), _passwordText.Text);
            var response = await _apiClient.PostAsync<LoginDto, AuthResponseDto>("/api/auth/login", dto);
            if (response is null)
            {
                MessageBox.Show("Login failed.");
                return;
            }

            Session = new UserSession(response.Token, response.Name, response.Email, response.Role);
            _sessionStore.Save(Session);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Login error");
        }
    }
}
