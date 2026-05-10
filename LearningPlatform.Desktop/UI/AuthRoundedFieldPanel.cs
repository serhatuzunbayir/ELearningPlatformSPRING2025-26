using System.Drawing.Drawing2D;

namespace LearningPlatform.Desktop.UI;

/// <summary>Rounded email/password field: anti-aliased fill + border drawn after child TextBox (masks square TB corners).</summary>
public sealed class AuthRoundedFieldPanel : Panel
{
    private readonly int _radius;

    public AuthRoundedFieldPanel(TextBox inner, int radius = Theme.AuthInputCornerRadius)
    {
        _radius = radius;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        BackColor = Color.Transparent;
        Margin = Padding.Empty;

        Theme.StyleAuthTextBox(inner);
        inner.BorderStyle = BorderStyle.None;
        inner.Margin = Padding.Empty;

        inner.Padding = new Padding(4, 8, 4, 8);

        Padding = new Padding(14, 11, 14, 11);
        inner.Dock = DockStyle.Fill;
        Controls.Add(inner);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        base.OnPaintBackground(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var bounds = ClientRectangle;
        bounds.Width--;
        bounds.Height--;
        var rad = Theme.ClampCornerRadius(bounds, _radius);

        using var path = Theme.RoundedRect(bounds, rad);
        using var brush = new SolidBrush(Theme.AuthFieldFill);
        g.FillPath(brush, path);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var bounds = ClientRectangle;
        bounds.Width--;
        bounds.Height--;
        var rad = Theme.ClampCornerRadius(bounds, _radius);

        using var path = Theme.RoundedRect(bounds, rad);
        using var pen = new Pen(Theme.AuthFieldBorder, 1f);
        g.DrawPath(pen, path);
    }
}
