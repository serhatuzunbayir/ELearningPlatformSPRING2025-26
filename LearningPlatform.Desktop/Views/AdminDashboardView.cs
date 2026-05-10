using System.Drawing.Drawing2D;
using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class AdminDashboardView : UserControl
{
    private readonly ApiClient _api;
    private readonly Action _navCourses;
    private readonly Action _navEnrollments;
    private readonly Action _createCourse;
    private readonly FlowLayoutPanel _kpiRow = new() { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false, Margin = new Padding(0, 8, 0, 24) };
    private readonly Label _nameLbl = new() { Font = Theme.UiFont(18f, FontStyle.Bold), ForeColor = Theme.Primary, AutoSize = true };
    private readonly GlassCardPanel _recentCard = new() { Dock = DockStyle.Fill, Padding = new Padding(20) };

    public AdminDashboardView(ApiClient api, Action navCourses, Action navEnrollments, Action createCourse)
    {
        _api = api;
        _navCourses = navCourses;
        _navEnrollments = navEnrollments;
        _createCourse = createCourse;
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        var welcomeRow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, WrapContents = false };
        welcomeRow.Controls.Add(new Label { Text = "Welcome,", Font = Theme.UiFont(18f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 8, 0) });
        welcomeRow.Controls.Add(_nameLbl);

        var actionsCard = new GlassCardPanel { Dock = DockStyle.Fill, Padding = new Padding(20) };
        FillQuickActions(actionsCard);

        var split = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
        split.Controls.Add(actionsCard, 0, 0);
        split.Controls.Add(_recentCard, 1, 0);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.Controls.Add(welcomeRow, 0, 0);
        root.Controls.Add(_kpiRow, 0, 1);
        root.Controls.Add(split, 0, 2);

        Controls.Add(root);
    }

    private void FillQuickActions(GlassCardPanel actionsCard)
    {
        var stack = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1 };
        stack.Controls.Add(new Label { Text = "Quick Actions", Font = Theme.UiFont(13f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 16) }, 0, 0);

        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, RowCount = 3 };
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var b1 = new Button { Text = "Manage Courses", Dock = DockStyle.Fill, Height = 44, Margin = new Padding(0, 0, 0, 10) };
        Theme.StylePrimaryButton(b1);
        b1.Click += (_, _) => _navCourses();

        var b2 = new Button { Text = "Review Pending Enrollments", Dock = DockStyle.Fill, Height = 44, Margin = new Padding(0, 0, 0, 10) };
        b2.FlatStyle = FlatStyle.Flat;
        b2.FlatAppearance.BorderSize = 0;
        b2.BackColor = Theme.Warning;
        b2.ForeColor = Color.White;
        b2.Font = Theme.UiFont(10f, FontStyle.Bold);
        b2.Cursor = Cursors.Hand;
        b2.Click += (_, _) => _navEnrollments();

        var b3 = new Button { Text = "Add New Course", Dock = DockStyle.Fill, Height = 44 };
        Theme.StylePrimaryButton(b3);
        b3.BackColor = Theme.Success;
        b3.Click += (_, _) => _createCourse();

        grid.Controls.Add(b1, 0, 0);
        grid.Controls.Add(b2, 0, 1);
        grid.Controls.Add(b3, 0, 2);

        stack.Controls.Add(grid, 0, 1);
        actionsCard.Controls.Add(stack);
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

    public async Task RefreshAsync(string adminDisplayName)
    {
        if (!string.IsNullOrEmpty(adminDisplayName))
            _nameLbl.Text = adminDisplayName;

        try
        {
            var courses = await _api.GetAsync<List<CourseListDto>>("/api/courses") ?? [];
            var pending = await _api.GetAsync<List<EnrollmentDto>>("/api/enrollments/pending") ?? [];
            var approvedTotal = courses.Sum(c => c.EnrollmentCount);

            _kpiRow.Controls.Clear();
            _kpiRow.Controls.Add(BuildKpiCard("Total Courses", courses.Count.ToString(), Theme.Primary));
            _kpiRow.Controls.Add(BuildKpiCard("Pending Enrollments", pending.Count.ToString(), Theme.Warning));
            _kpiRow.Controls.Add(BuildKpiCard("Approved Enrollments", approvedTotal.ToString(), Theme.Success));

            RenderRecent(pending.Take(5).ToList());
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Admin Dashboard");
        }
    }

    private void RenderRecent(List<EnrollmentDto> recent)
    {
        _recentCard.Controls.Clear();

        var stack = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, AutoSize = true };
        stack.Controls.Add(new Label { Text = "Latest Pending Requests", Font = Theme.UiFont(13f, FontStyle.Bold), AutoSize = true, Margin = new Padding(0, 0, 0, 16) }, 0, 0);

        if (!recent.Any())
        {
            stack.Controls.Add(new Label { Text = "No pending enrollment requests right now.", ForeColor = Theme.TextMuted, AutoSize = true }, 0, 1);
            _recentCard.Controls.Add(stack);
            return;
        }

        var list = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false };
        foreach (var enr in recent)
        {
            var row = new Panel { Width = Math.Max(320, _recentCard.ClientSize.Width - 56), Height = 52, Margin = new Padding(0, 0, 0, 8), BackColor = Theme.PageBackground, Padding = new Padding(12, 8, 12, 8) };
            var inner = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75f));
            inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var left = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            left.Controls.Add(new Label { Text = enr.StudentName, Font = Theme.UiFont(10f, FontStyle.Bold), AutoSize = true }, 0, 0);
            left.Controls.Add(new Label { Text = enr.CourseTitle, Font = Theme.UiFont(8.5f), ForeColor = Theme.TextMuted, AutoSize = true }, 0, 1);

            var badge = new Label
            {
                Text = "Pending",
                AutoSize = true,
                BackColor = Theme.Warning,
                ForeColor = Color.FromArgb(33, 37, 41),
                Padding = new Padding(10, 4, 10, 4),
                Font = Theme.UiFont(8.5f, FontStyle.Bold),
                Margin = new Padding(8)
            };
            var badgeWrap = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            badgeWrap.Controls.Add(badge);

            inner.Controls.Add(left, 0, 0);
            inner.Controls.Add(badgeWrap, 1, 0);
            row.Controls.Add(inner);
            list.Controls.Add(row);
        }

        stack.Controls.Add(list, 0, 1);

        var seeAll = new LinkLabel { Text = "See All →", AutoSize = true, LinkColor = Theme.Primary, Margin = new Padding(0, 12, 0, 0) };
        seeAll.Click += (_, _) => _navEnrollments();
        stack.Controls.Add(seeAll, 0, 2);

        _recentCard.Controls.Add(stack);
    }
}
