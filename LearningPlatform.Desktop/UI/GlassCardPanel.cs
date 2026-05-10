using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace LearningPlatform.Desktop.UI;

/// <summary>Solid card mimicking web rounded cards (WinForms has no real backdrop blur).</summary>
public class GlassCardPanel : Panel
{
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int CornerRadius { get; set; } = 14;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color StrokeColor { get; set; } = Color.FromArgb(230, 230, 230);

    public GlassCardPanel()
    {
        DoubleBuffered = true;
        BackColor = Theme.CardBackground;
        Padding = new Padding(16);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = ClientRectangle;
        rect.Width--;
        rect.Height--;

        using var path = Theme.RoundedRect(rect, CornerRadius);
        using var fill = new SolidBrush(BackColor);
        g.FillPath(fill, path);

        using var pen = new Pen(StrokeColor, 1);
        g.DrawPath(pen, path);
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Painted in OnPaint
    }
}
