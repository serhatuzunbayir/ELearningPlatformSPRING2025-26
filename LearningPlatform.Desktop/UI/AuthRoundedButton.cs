using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace LearningPlatform.Desktop.UI;

public enum AuthRoundedButtonVisualStyle
{
    Primary,
    Outline,
}

/// <summary>Smooth rounded Sign In / Create account buttons (no Region clipping).</summary>
public sealed class AuthRoundedButton : Button
{
    private AuthRoundedButtonVisualStyle _visualStyle = AuthRoundedButtonVisualStyle.Primary;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public AuthRoundedButtonVisualStyle VisualStyle
    {
        get => _visualStyle;
        set
        {
            _visualStyle = value;
            Invalidate();
        }
    }

    public AuthRoundedButton()
    {
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.UserPaint |
            ControlStyles.ResizeRedraw |
            ControlStyles.Selectable |
            ControlStyles.SupportsTransparentBackColor,
            true);

        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        Cursor = Cursors.Hand;
        TabStop = true;
        ForeColor = Color.White;
        Font = Theme.UiFont(10.5f, FontStyle.Bold);
        BackColor = Color.Transparent;
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
        base.OnPaintBackground(pevent);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.HighQuality;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        var r = ClientRectangle;
        r.Inflate(-1, -1);
        var rad = Theme.ClampCornerRadius(r, Theme.AuthButtonCornerRadius);
        using var path = Theme.RoundedRect(r, rad);

        if (_visualStyle == AuthRoundedButtonVisualStyle.Primary)
        {
            var fill = Enabled ? Theme.AccentCyan : Color.FromArgb(120, 120, 130);
            using var brush = new SolidBrush(fill);
            g.FillPath(brush, path);
        }
        else
        {
            using var brush = new SolidBrush(Color.FromArgb(42, 35, 55, 72));
            g.FillPath(brush, path);
            using var pen = new Pen(Theme.AccentMint, 2f);
            g.DrawPath(pen, path);
        }

        using var textBrush = new SolidBrush(Enabled ? ForeColor : Color.FromArgb(180, 180, 180));
        var fmt = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
        };
        g.DrawString(Text, Font, textBrush, ClientRectangle, fmt);

    }
}
