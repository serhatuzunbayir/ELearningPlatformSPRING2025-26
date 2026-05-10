using System.Drawing.Drawing2D;
using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class CourseProgressView : UserControl
{
    private readonly ApiClient _api;
    private readonly int _courseId;
    private readonly Action _onBack;
    private readonly FlowLayoutPanel _modulesFlow = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        Padding = new Padding(8)
    };

    private Panel _heroSlot = null!;

    public CourseProgressView(ApiClient api, int courseId, Action onBack)
    {
        _api = api;
        _courseId = courseId;
        _onBack = onBack;
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 150f));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var back = new LinkLabel { Text = "← Back to Dashboard", AutoSize = true, LinkColor = Theme.Primary, Margin = new Padding(0, 0, 0, 12) };
        back.LinkClicked += (_, _) => _onBack();

        _heroSlot = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 16) };

        root.Controls.Add(back, 0, 0);
        root.Controls.Add(_heroSlot, 0, 1);
        root.Controls.Add(_modulesFlow, 0, 2);

        Controls.Add(root);

        Load += async (_, _) => await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            var model = await _api.GetAsync<CourseProgressDto>($"/api/progress/course/{_courseId}");
            if (model is null) return;

            _heroSlot.Controls.Clear();
            var fillFrac = Math.Clamp(model.CompletionPercentage / 100.0, 0, 1);

            var hero = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Primary, Padding = new Padding(20, 16, 20, 12) };
            hero.Paint += (_, e) =>
            {
                var r = hero.ClientRectangle;
                r.Inflate(-1, -1);
                using var path = Theme.RoundedRect(r, 14);
                using var b = new SolidBrush(Theme.Primary);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                e.Graphics.FillPath(b, path);
            };

            var barOuter = new Panel { Height = 14, Dock = DockStyle.Bottom };
            barOuter.Paint += (_, e) =>
            {
                using var track = new SolidBrush(Color.FromArgb(90, 255, 255, 255));
                e.Graphics.FillRectangle(track, barOuter.ClientRectangle);
                var w = (int)(barOuter.Width * fillFrac);
                if (w > 0)
                {
                    using var fg = new SolidBrush(Theme.Success);
                    e.Graphics.FillRectangle(fg, 0, 0, w, barOuter.Height);
                }
            };

            var inner = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(4), Margin = new Padding(0, 0, 0, 10) };
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72f));
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));

            var title = new Label
            {
                Text = model.CourseTitle,
                Font = Theme.UiFont(15f, FontStyle.Bold),
                ForeColor = Color.White,
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            var sub = new Label
            {
                Text = $"You have completed {model.CompletedModules} out of {model.TotalModules} modules.",
                Font = Theme.UiFont(9.5f),
                ForeColor = Color.FromArgb(235, 235, 235),
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            var pct = new Label
            {
                Text = $"{model.CompletionPercentage:0.0}%",
                Font = Theme.UiFont(26f, FontStyle.Bold),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.TopRight,
                Dock = DockStyle.Fill,
                AutoSize = true
            };

            inner.Controls.Add(title, 0, 0);
            inner.Controls.Add(pct, 1, 0);
            inner.Controls.Add(sub, 0, 1);
            inner.SetColumnSpan(sub, 2);

            hero.Controls.Add(inner);
            hero.Controls.Add(barOuter);
            _heroSlot.Controls.Add(hero);

            _modulesFlow.Controls.Clear();
            _modulesFlow.Controls.Add(new Label { Text = "Course Modules", Font = Theme.UiFont(12f, FontStyle.Bold), AutoSize = true, Margin = new Padding(8, 8, 8, 12) });

            foreach (var module in model.Modules.OrderBy(m => m.Order))
                _modulesFlow.Controls.Add(BuildModuleRow(module));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Course Progress");
        }
    }

    private Panel BuildModuleRow(ModuleProgressDto module)
    {
        var row = new GlassCardPanel
        {
            Margin = new Padding(8, 4, 8, 4),
            Padding = new Padding(16, 12, 16, 12),
            MinimumSize = new Size(_modulesFlow.ClientSize.Width > 0 ? _modulesFlow.ClientSize.Width - 36 : 600, 72),
            Width = _modulesFlow.ClientSize.Width > 0 ? _modulesFlow.ClientSize.Width - 36 : 700,
            Height = 72
        };

        var status = new Label
        {
            Text = module.IsCompleted ? "\u2713" : "\u25CB",
            Font = Theme.UiFont(14f, FontStyle.Bold),
            ForeColor = module.IsCompleted ? Theme.Success : Theme.Secondary,
            Width = 28,
            Dock = DockStyle.Left,
            TextAlign = ContentAlignment.MiddleCenter
        };

        var textCol = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        textCol.RowStyles.Add(new RowStyle(SizeType.Percent, 55f));
        textCol.RowStyles.Add(new RowStyle(SizeType.Percent, 45f));

        var title = new Label
        {
            Text = $"Module {module.Order}: {module.Title}",
            Font = Theme.UiFont(10f, FontStyle.Bold),
            Dock = DockStyle.Fill,
            AutoEllipsis = true
        };
        var meta = new Label
        {
            Text = module.IsCompleted && module.CompletedAt.HasValue
                ? $"Completed on {module.CompletedAt.Value:g}"
                : "Not yet completed",
            Font = Theme.UiFont(8.5f),
            ForeColor = Theme.TextMuted,
            Dock = DockStyle.Fill
        };
        textCol.Controls.Add(title, 0, 0);
        textCol.Controls.Add(meta, 0, 1);

        FlowLayoutPanel actionCol;
        if (!module.IsCompleted)
        {
            var btn = new Button { Text = "Mark as Complete", AutoSize = true };
            Theme.StyleOutlineButton(btn);
            btn.Margin = new Padding(8, 0, 0, 0);
            btn.Click += async (_, _) =>
            {
                var ok = await _api.PostAsync("/api/progress/complete", new CompleteModuleDto(module.ModuleId));
                MessageBox.Show(ok ? "Module completed." : "Could not complete module.", "Progress");
                await LoadAsync();
            };
            actionCol = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Right, WrapContents = false };
            actionCol.Controls.Add(btn);
        }
        else
        {
            var done = new Button { Text = "Completed", AutoSize = true, Enabled = false };
            done.FlatStyle = FlatStyle.Flat;
            done.FlatAppearance.BorderSize = 0;
            done.BackColor = Theme.Success;
            done.ForeColor = Color.White;
            actionCol = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, AutoSize = true, Dock = DockStyle.Right, WrapContents = false };
            actionCol.Controls.Add(done);
        }

        var wrap = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36));
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        wrap.Controls.Add(status, 0, 0);
        wrap.Controls.Add(textCol, 1, 0);
        wrap.Controls.Add(actionCol, 2, 0);

        row.Controls.Add(wrap);
        return row;
    }
}
