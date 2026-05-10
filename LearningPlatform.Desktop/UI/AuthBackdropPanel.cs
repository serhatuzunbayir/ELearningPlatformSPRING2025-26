using System.Drawing.Drawing2D;

namespace LearningPlatform.Desktop.UI;

/// <summary>Full-client diagonal gradient plus soft cyan glow orbs (matches web login backdrop).</summary>
public class AuthBackdropPanel : Panel
{
    public AuthBackdropPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        Dock = DockStyle.Fill;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        var rect = ClientRectangle;

        Theme.PaintDiagonalGradient(g, rect, Theme.AuthGradient1, Theme.AuthGradient2, Theme.AuthGradient3);

        using (var glow = new SolidBrush(Color.FromArgb(50, 79, 172, 254)))
            g.FillEllipse(glow, -140, -140, 460, 460);

        using (var glow2 = new SolidBrush(Color.FromArgb(40, 0, 242, 254)))
            g.FillEllipse(glow2, Width - 300, Height - 320, 380, 380);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        // Gradient lives in OnPaintBackground so transparent auth controls resolve correct backdrop pixels.
    }
}
