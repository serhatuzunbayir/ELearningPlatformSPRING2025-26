using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Forms;

public class RegisterForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly TextBox _nameText = new();
    private readonly TextBox _emailText = new();
    private readonly TextBox _passwordText = new();

    public RegisterForm(ApiClient apiClient)
    {
        _apiClient = apiClient;
        Text = "Create Account — E-Learning Platform";
        ClientSize = new Size(580, 720);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        BackColor = Theme.AuthGradient1;

        var backdrop = new AuthBackdropPanel();

        var card = new AuthRoundedCardPanel
        {
            Width = 500,
            Height = 556,
        };

        _nameText.PlaceholderText = "Full Name";
        _emailText.PlaceholderText = "Email Address";
        _passwordText.PlaceholderText = "Password";
        _passwordText.UseSystemPasswordChar = true;

        var nameShell = new AuthRoundedFieldPanel(_nameText);
        var emailShell = new AuthRoundedFieldPanel(_emailText);
        var passwordShell = new AuthRoundedFieldPanel(_passwordText);

        var title = new Label
        {
            Text = "Join Us",
            Font = Theme.UiFont(20f, FontStyle.Bold),
            ForeColor = Theme.AccentMint,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            AutoSize = false
        };

        var subtitle = new Label
        {
            Text = "Create your account to start learning.",
            Font = Theme.UiFont(9.5f),
            ForeColor = Theme.AuthSubtitle,
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            AutoSize = false
        };

        var registerButton = new AuthRoundedButton { Text = "Create Account", Dock = DockStyle.Fill, VisualStyle = AuthRoundedButtonVisualStyle.Primary };
        registerButton.Click += async (_, _) => await RegisterAsync();

        var linkRow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 0)
        };
        linkRow.Controls.Add(new Label
        {
            Text = "Already have an account? ",
            AutoSize = true,
            ForeColor = Theme.AuthSubtitle,
            Font = Theme.UiFont(9f),
            Margin = new Padding(0, 6, 0, 0)
        });
        var signInLink = new LinkLabel
        {
            Text = "Sign in",
            AutoSize = true,
            LinkColor = Theme.AccentCyan,
            ActiveLinkColor = Theme.AccentMint,
            Font = Theme.UiFont(9f, FontStyle.Bold),
            Margin = new Padding(0, 4, 0, 0)
        };
        signInLink.LinkClicked += (_, _) => Close();
        linkRow.Controls.Add(signInLink);

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 11,
            Padding = Padding.Empty
        };
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 44f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 10f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 18f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 48f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 12f));
        inner.RowStyles.Add(new RowStyle(SizeType.Absolute, 36f));

        inner.Controls.Add(title, 0, 0);
        inner.Controls.Add(subtitle, 0, 1);
        inner.Controls.Add(nameShell, 0, 2);
        inner.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 3);
        inner.Controls.Add(emailShell, 0, 4);
        inner.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 5);
        inner.Controls.Add(passwordShell, 0, 6);
        inner.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 7);
        inner.Controls.Add(registerButton, 0, 8);
        inner.Controls.Add(new Panel { Dock = DockStyle.Fill }, 0, 9);
        inner.Controls.Add(linkRow, 0, 10);

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

    private async Task RegisterAsync()
    {
        try
        {
            var ok = await _apiClient.PostAsync("/api/auth/register", new RegisterDto(_nameText.Text.Trim(), _emailText.Text.Trim(), _passwordText.Text));
            MessageBox.Show(ok ? "Registration successful." : "Registration failed.", Text);
            if (ok) Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Register error");
        }
    }
}
