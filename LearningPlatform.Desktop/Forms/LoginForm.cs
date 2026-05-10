using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Forms;

public class LoginForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly SessionStore _sessionStore;
    private readonly TextBox _emailText = new();
    private readonly TextBox _passwordText = new();

    public UserSession? Session { get; private set; }

    public LoginForm(ApiClient apiClient, SessionStore sessionStore)
    {
        _apiClient = apiClient;
        _sessionStore = sessionStore;
        Text = "Sign In — E-Learning Platform";
        ClientSize = new Size(580, 640);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        BackColor = Theme.AuthGradient1;

        var backdrop = new AuthBackdropPanel();

        var card = new AuthRoundedCardPanel
        {
            Width = 500,
            Height = 416,
        };

        _emailText.PlaceholderText = "Email Address";
        _passwordText.PlaceholderText = "Password";
        _passwordText.UseSystemPasswordChar = true;

        var emailShell = new AuthRoundedFieldPanel(_emailText) { Dock = DockStyle.Fill };
        var passwordShell = new AuthRoundedFieldPanel(_passwordText) { Dock = DockStyle.Fill };

        var title = new Label
        {
            Text = "Welcome Back",
            Font = Theme.UiFont(20f, FontStyle.Bold),
            ForeColor = Theme.AccentMint,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            AutoSize = false
        };

        var subtitle = new Label
        {
            Text = "Sign in to continue your learning journey.",
            Font = Theme.UiFont(9.5f),
            ForeColor = Theme.AuthSubtitle,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            AutoSize = false
        };

        var loginButton = new AuthRoundedButton { Text = "Sign In", Dock = DockStyle.Fill, VisualStyle = AuthRoundedButtonVisualStyle.Primary };
        loginButton.Click += async (_, _) => await LoginAsync();

        var linkRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0)
        };
        linkRow.Controls.Add(new Label
        {
            Text = "Don't have an account? ",
            AutoSize = true,
            ForeColor = Theme.AuthSubtitle,
            Font = Theme.UiFont(9f),
            Margin = new Padding(0, 6, 0, 0)
        });
        var createLink = new LinkLabel
        {
            Text = "Create one",
            AutoSize = true,
            LinkColor = Theme.AccentCyan,
            ActiveLinkColor = Theme.AccentMint,
            VisitedLinkColor = Theme.AccentCyan,
            Font = Theme.UiFont(9f, FontStyle.Bold),
            Margin = new Padding(0, 4, 0, 0)
        };
        createLink.LinkClicked += (_, _) =>
        {
            using var f = new RegisterForm(_apiClient);
            f.ShowDialog(this);
        };
        linkRow.Controls.Add(createLink);

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 8,
            Padding = Padding.Empty
        };
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 18f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));

        inner.Controls.Add(title, 0, 0);
        inner.Controls.Add(subtitle, 0, 1);
        inner.Controls.Add(emailShell, 0, 2);
        inner.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 3);
        inner.Controls.Add(passwordShell, 0, 4);
        inner.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 5);
        inner.Controls.Add(loginButton, 0, 6);
        inner.Controls.Add(linkRow, 0, 7);

        card.Controls.Add(inner);

        void CenterCard(object? sender, LayoutEventArgs args)
        {
            card.Left = Math.Max(0, (backdrop.Width - card.Width) / 2);
            card.Top = Math.Max(0, (backdrop.Height - card.Height) / 2);
        }

        backdrop.Layout += CenterCard;
        backdrop.Controls.Add(card);

        Controls.Add(backdrop);
    }

    private async Task LoginAsync()
    {
        try
        {
            var dto = new LoginDto(_emailText.Text.Trim(), _passwordText.Text);
            var response = await _apiClient.PostAsync<LoginDto, AuthResponseDto>("/api/auth/login", dto);
            if (response is null)
            {
                MessageBox.Show("Login failed.", Text);
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
