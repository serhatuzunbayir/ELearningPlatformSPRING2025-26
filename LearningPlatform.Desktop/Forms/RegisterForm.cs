using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;

namespace LearningPlatform.Desktop.Forms;

public class RegisterForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly TextBox _nameText = new() { PlaceholderText = "Name", Width = 280 };
    private readonly TextBox _emailText = new() { PlaceholderText = "Email", Width = 280 };
    private readonly TextBox _passwordText = new() { PlaceholderText = "Password", Width = 280, UseSystemPasswordChar = true };

    public RegisterForm(ApiClient apiClient)
    {
        _apiClient = apiClient;
        Text = "Register";
        Width = 380;
        Height = 240;
        StartPosition = FormStartPosition.CenterParent;

        var registerButton = new Button { Text = "Create Account", Width = 160 };
        registerButton.Click += async (_, _) => await RegisterAsync();

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(20),
            WrapContents = false
        };
        panel.Controls.AddRange([_nameText, _emailText, _passwordText, registerButton]);
        Controls.Add(panel);
    }

    private async Task RegisterAsync()
    {
        try
        {
            var ok = await _apiClient.PostAsync("/api/auth/register", new RegisterDto(_nameText.Text.Trim(), _emailText.Text.Trim(), _passwordText.Text));
            MessageBox.Show(ok ? "Registration successful." : "Registration failed.");
            if (ok) Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Register error");
        }
    }
}
