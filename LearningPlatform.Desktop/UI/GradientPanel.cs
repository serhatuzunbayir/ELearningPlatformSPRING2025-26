using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace LearningPlatform.Desktop.UI;

public class GradientPanel : Panel
{
    public enum GradientKind
    {
        Sidebar,
        AuthBackground,
        LightFill
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public GradientKind Kind { get; set; } = GradientKind.LightFill;

    public GradientPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        var rect = ClientRectangle;

        switch (Kind)
        {
            case GradientKind.Sidebar:
                Theme.PaintVerticalGradient(g, rect, Theme.SidebarDark1, Theme.SidebarDark3);
                break;
            case GradientKind.AuthBackground:
                Theme.PaintDiagonalGradient(g, rect, Theme.AuthGradient1, Theme.AuthGradient2, Theme.AuthGradient3);
                break;
            case GradientKind.LightFill:
                using (var b = new SolidBrush(Theme.PageBackground))
                    g.FillRectangle(b, rect);
                break;
        }
    }
}
