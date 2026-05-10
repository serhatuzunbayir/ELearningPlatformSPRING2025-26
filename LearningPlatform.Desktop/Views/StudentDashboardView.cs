using System.Drawing.Drawing2D;
using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class StudentDashboardView : UserControl
{
    private readonly ApiClient _api;
    private readonly string _displayName;
    private readonly Action<int> _onContinueCourse;
    private readonly Action _onBrowseCourses;
    private readonly FlowLayoutPanel _kpiRow = new() { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false, Margin = new Padding(0, 16, 0, 8) };
    private readonly FlowLayoutPanel _courseFlow = new() { AutoScroll = true, Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(8) };

    public StudentDashboardView(ApiClient api, string displayName, Action<int> onContinueCourse, Action onBrowseCourses)
    {
        _api = api;
        _displayName = displayName;
        _onContinueCourse = onContinueCourse;
        _onBrowseCourses = onBrowseCourses;
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 4 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        var welcome = new Label
        {
            Text = $"Welcome back, {_displayName}!",
            Font = Theme.UiFont(18f, FontStyle.Bold),
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 4)
        };

        var titleCourses = new Label
        {
            Text = "Your Active Courses",
            Font = Theme.UiFont(13f, FontStyle.Bold),
            Dock = DockStyle.Top,
            AutoSize = true,
            Margin = new Padding(0, 8, 0, 8)
        };

        root.Controls.Add(welcome, 0, 0);
        root.Controls.Add(_kpiRow, 0, 1);
        root.Controls.Add(titleCourses, 0, 2);
        root.Controls.Add(_courseFlow, 0, 3);

        Controls.Add(root);
        Load += async (_, _) => await RefreshAsync();
    }

    private static Panel BuildKpiCard(string title, string valueText, Color bg)
    {
        var card = new Panel { Width = 220, Height = 110, Margin = new Padding(0, 0, 16, 0), BackColor = bg, Padding = new Padding(16) };
        card.Paint += (_, e) =>
        {
            var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            using var path = Theme.RoundedRect(r, 14);
            using var pen = new Pen(Color.FromArgb(50, Color.White), 1);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.DrawPath(pen, path);
        };

        var t = new Label { Text = title, ForeColor = Color.White, Font = Theme.UiFont(10f, FontStyle.Bold), Dock = DockStyle.Top, AutoSize = true };
        var v = new Label { Text = valueText, ForeColor = Color.White, Font = Theme.UiFont(26f, FontStyle.Bold), Dock = DockStyle.Bottom, AutoSize = true };
        card.Controls.Add(v);
        card.Controls.Add(t);
        return card;
    }

    public async Task RefreshAsync()
    {
        try
        {
            var summary = await _api.GetAsync<ProgressSummaryDto>("/api/progress/summary");
            Render(summary);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Dashboard");
        }
    }

    private void Render(ProgressSummaryDto? summary)
    {
        _kpiRow.Controls.Clear();
        if (summary is not null)
        {
            _kpiRow.Controls.Add(BuildKpiCard("Enrolled Courses", summary.TotalEnrolledCourses.ToString(), Theme.Primary));
            _kpiRow.Controls.Add(BuildKpiCard("Completed Courses", summary.FullyCompletedCourses.ToString(), Theme.Success));
            _kpiRow.Controls.Add(BuildKpiCard("Overall Progress", $"{summary.OverallCompletionPercentage:0.0}%", Theme.Info));
        }

        _courseFlow.Controls.Clear();
        if (summary?.Courses is null || !summary.Courses.Any())
        {
            var empty = new GlassCardPanel { Width = 640, Height = 240, Margin = new Padding(8), Padding = new Padding(24) };
            var stack = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            stack.Controls.Add(new Label
            {
                Text = "No Active Courses",
                Font = Theme.UiFont(14f, FontStyle.Bold),
                AutoSize = true,
                Padding = new Padding(0, 8, 0, 8)
            }, 0, 0);

            stack.Controls.Add(new Label
            {
                Text = "You are not enrolled in any courses yet, or your enrollments are pending approval.",
                Font = Theme.UiFont(10f),
                ForeColor = Theme.TextMuted,
                AutoSize = true,
                MaximumSize = new Size(560, 0),
                Padding = new Padding(0, 0, 0, 16)
            }, 0, 1);

            var browse = new Button { Text = "Browse Courses", AutoSize = true };
            Theme.StylePrimaryButton(browse);
            browse.Click += (_, _) => _onBrowseCourses();
            var wrap = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true };
            wrap.Controls.Add(browse);
            stack.Controls.Add(wrap, 0, 2);

            empty.Controls.Add(stack);
            _courseFlow.Controls.Add(empty);
            return;
        }

        foreach (var course in summary.Courses)
            _courseFlow.Controls.Add(BuildCourseCard(course));
    }

    private Panel BuildCourseCard(CourseProgressDto course)
    {
        var card = new GlassCardPanel { Width = 340, Height = 190, Margin = new Padding(12), Padding = new Padding(16) };

        var title = new Label { Text = course.CourseTitle, Font = Theme.UiFont(12f, FontStyle.Bold), AutoSize = false, Width = 300, Height = 44 };
        var sub = new Label { Text = $"Modules completed: {course.CompletedModules} / {course.TotalModules}", Font = Theme.UiFont(9f), ForeColor = Theme.TextMuted, AutoSize = true };

        var barOuter = new Panel { Height = 12, Width = 300, BackColor = Color.FromArgb(233, 236, 239), Margin = new Padding(0, 10, 0, 12) };
        var fillW = (int)(300 * Math.Clamp(course.CompletionPercentage / 100.0, 0, 1));
        var barInner = new Panel { Height = 12, Width = Math.Max(0, fillW), BackColor = Theme.Success, Dock = DockStyle.Left };
        barOuter.Controls.Add(barInner);

        var btn = new Button { Text = "Continue Course →", AutoSize = true };
        Theme.StylePrimaryButton(btn);
        btn.Click += (_, _) => _onContinueCourse(course.CourseId);

        var footer = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
        footer.Controls.Add(btn);

        var stack = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.Controls.Add(title, 0, 0);
        stack.Controls.Add(sub, 0, 1);
        stack.Controls.Add(barOuter, 0, 2);

        card.Controls.Add(footer);
        card.Controls.Add(stack);

        return card;
    }
}
