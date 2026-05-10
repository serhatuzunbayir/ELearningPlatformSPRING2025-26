using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace LearningPlatform.Desktop.UI;

/// <summary>Glass-style auth card with smooth AA rounded corners (web auth-container ~20px).</summary>
public sealed class AuthRoundedCardPanel : Panel
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Radius { get; set; } = Theme.AuthCardCornerRadius;

    public AuthRoundedCardPanel()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw |
            ControlStyles.SupportsTransparentBackColor,
            true);
        BackColor = Color.Transparent;
        Padding = new Padding(40, 40, 40, 32);
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
        var rad = Theme.ClampCornerRadius(bounds, Radius);

        using var path = Theme.RoundedRect(bounds, rad);
        using var brush = new SolidBrush(Theme.AuthCardFill);
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
        var rad = Theme.ClampCornerRadius(bounds, Radius);

        using var path = Theme.RoundedRect(bounds, rad);
        using var pen = new Pen(Color.FromArgb(110, 180, 210, 235), 1f);
        g.DrawPath(pen, path);
    }
}
