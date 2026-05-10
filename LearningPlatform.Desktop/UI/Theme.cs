using System.Drawing.Drawing2D;

namespace LearningPlatform.Desktop.UI;

public static class Theme
{
    public static readonly Color Primary = Color.FromArgb(13, 110, 253);
    public static readonly Color Success = Color.FromArgb(25, 135, 84);
    public static readonly Color Info = Color.FromArgb(13, 202, 240);
    public static readonly Color Warning = Color.FromArgb(255, 193, 7);
    public static readonly Color Danger = Color.FromArgb(220, 53, 69);
    public static readonly Color Secondary = Color.FromArgb(108, 117, 125);

    public static readonly Color SidebarDark1 = Color.FromArgb(15, 32, 39);
    public static readonly Color SidebarDark2 = Color.FromArgb(32, 58, 67);
    public static readonly Color SidebarDark3 = Color.FromArgb(44, 83, 100);

    /// <summary>Sidebar nav labels on dark gradient (readable light blue).</summary>
    public static readonly Color SidebarNavText = Color.FromArgb(168, 218, 255);

    public static readonly Color AuthGradient1 = Color.FromArgb(15, 32, 39);
    public static readonly Color AuthGradient2 = Color.FromArgb(32, 58, 67);
    public static readonly Color AuthGradient3 = Color.FromArgb(44, 83, 100);

    public static readonly Color AccentCyan = Color.FromArgb(79, 172, 254);
    public static readonly Color AccentMint = Color.FromArgb(0, 242, 254);

    public static readonly Color PageBackground = Color.FromArgb(248, 249, 250);
    public static readonly Color CardBackground = Color.White;
    public static readonly Color HeaderBar = Color.White;
    public static readonly Color TextMuted = Color.FromArgb(108, 117, 125);

    /// <summary>Frosted-card panel on auth screens (approximates web rgba glass).</summary>
    public static readonly Color AuthCardFill = Color.FromArgb(52, 62, 78);

    public static readonly Color AuthFieldFill = Color.FromArgb(42, 55, 72);
    public static readonly Color AuthFieldBorder = Color.FromArgb(180, 255, 255, 255);
    public static readonly Color AuthSubtitle = Color.FromArgb(210, 220, 230);

    public static Font UiFont(float emSize, FontStyle style = FontStyle.Regular) =>
        new("Segoe UI", emSize, style, GraphicsUnit.Point);

    public static void PaintVerticalGradient(Graphics g, Rectangle bounds, Color top, Color bottom)
    {
        using var brush = new LinearGradientBrush(bounds, top, bottom, LinearGradientMode.Vertical);
        g.FillRectangle(brush, bounds);
    }

    public static void PaintDiagonalGradient(Graphics g, Rectangle bounds, Color c1, Color c2, Color c3)
    {
        using var brush = new LinearGradientBrush(bounds, c1, c3, LinearGradientMode.ForwardDiagonal);
        var blend = new ColorBlend(3)
        {
            Colors = [c1, c2, c3],
            Positions = [0f, 0.5f, 1f]
        };
        brush.InterpolationColors = blend;
        g.FillRectangle(brush, bounds);
    }

    public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var path = new GraphicsPath();
        var d = radius * 2;
        path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    /// <summary>Matches web login: .auth-container 20px, .form-control / .btn-primary ~10px.</summary>
    public const int AuthCardCornerRadius = 20;

    public const int AuthInputCornerRadius = 10;
    public const int AuthButtonCornerRadius = 10;

    public static int ClampCornerRadius(Rectangle bounds, int requested)
    {
        if (bounds.Width < 4 || bounds.Height < 4) return 2;
        var maxR = Math.Min(bounds.Width, bounds.Height) / 2;
        return Math.Max(2, Math.Min(requested, maxR));
    }

    public static void StylePrimaryButton(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.BackColor = Primary;
        b.ForeColor = Color.White;
        b.Cursor = Cursors.Hand;
        b.Font = UiFont(9.5f, FontStyle.Bold);
        b.Padding = new Padding(12, 8, 12, 8);
    }

    public static void StyleOutlineButton(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderColor = Primary;
        b.FlatAppearance.BorderSize = 1;
        b.BackColor = Color.White;
        b.ForeColor = Primary;
        b.Cursor = Cursors.Hand;
        b.Font = UiFont(9f);
    }

    public static void StyleSidebarButton(Button b, bool selected)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.FlatAppearance.MouseOverBackColor = Color.FromArgb(48, 72, 84);
        b.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 62, 74);
        b.UseVisualStyleBackColor = false;
        b.Height = 44;
        b.TextAlign = ContentAlignment.MiddleLeft;
        b.Padding = new Padding(16, 0, 0, 0);
        b.ForeColor = SidebarNavText;
        b.Cursor = Cursors.Hand;
        b.Font = UiFont(10f);
        b.BackColor = selected ? Color.FromArgb(60, 90, 100) : Color.Transparent;
    }

    public static void StyleDangerOutline(Button b)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderColor = Danger;
        b.FlatAppearance.BorderSize = 1;
        b.BackColor = Color.White;
        b.ForeColor = Danger;
        b.Cursor = Cursors.Hand;
        b.Font = UiFont(9f, FontStyle.Bold);
    }

    public static void StyleAuthTextBox(TextBox tb)
    {
        tb.BorderStyle = BorderStyle.FixedSingle;
        tb.BackColor = AuthFieldFill;
        tb.ForeColor = Color.White;
        tb.Font = UiFont(10f);
        tb.Margin = Padding.Empty;
    }

    public static Label MakeBadge(string text, Color bg)
    {
        return new Label
        {
            Text = text,
            AutoSize = true,
            BackColor = bg,
            ForeColor = Color.White,
            Padding = new Padding(10, 4, 10, 4),
            Font = UiFont(8.5f, FontStyle.Bold),
            Margin = new Padding(0, 0, 8, 0)
        };
    }
}
