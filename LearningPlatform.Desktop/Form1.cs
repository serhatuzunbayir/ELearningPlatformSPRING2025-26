using LearningPlatform.Desktop.Forms;
using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;
using LearningPlatform.Desktop.Views;

namespace LearningPlatform.Desktop;

public partial class Form1 : Form
{
    private enum ShellNav
    {
        StudentDashboard,
        StudentCourses,
        StudentStudyPlan,
        AdminDashboard,
        AdminCourses,
        AdminEnrollments,
        AdminStudentProgress,
        AdminAnalytics
    }

    private readonly ApiClient _apiClient;
    private readonly SessionStore _sessionStore;
    private readonly UserSession _session;
    private readonly bool _isAdmin;
    private readonly GradientPanel _sidebar = new() { Kind = GradientPanel.GradientKind.Sidebar, Dock = DockStyle.Left, Width = 236 };
    private readonly Panel _header = new() { Dock = DockStyle.Top, Height = 58, BackColor = Theme.HeaderBar };
    private readonly Panel _contentHost = new() { Dock = DockStyle.Fill, BackColor = Theme.PageBackground };
    private readonly List<Button> _navButtons = [];

    public bool ShouldReturnToLogin { get; private set; }

    public Form1(ApiClient apiClient, SessionStore sessionStore, UserSession session)
    {
        _apiClient = apiClient;
        _sessionStore = sessionStore;
        _session = session;
        _isAdmin = _session.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        InitializeComponent();
        Text = $"learning_platform — {_session.Role}";
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(960, 640);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.PageBackground;

        BuildChrome();

        Controls.Add(_contentHost);
        Controls.Add(_sidebar);
        Controls.Add(_header);

        if (_isAdmin)
            ShowAdminDashboard();
        else
            ShowStudentDashboard();
    }

    private void BuildChrome()
    {
        _header.Padding = new Padding(20, 0, 20, 0);
        _header.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(230, 230, 230), 1);
            e.Graphics.DrawLine(pen, 0, _header.Height - 1, _header.Width, _header.Height - 1);
        };

        var brand = new Label
        {
            Text = "learning_platform",
            Font = Theme.UiFont(12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            AutoSize = true,
            Anchor = AnchorStyles.Left | AnchorStyles.Top,
            Location = new Point(20, 18)
        };

        var userChip = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Anchor = AnchorStyles.Right | AnchorStyles.Top };
        userChip.Location = new Point(_header.Width - 320, 14);
        userChip.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        var userName = new Label
        {
            Text = _session.Name,
            Font = Theme.UiFont(10f, FontStyle.Bold),
            ForeColor = Theme.Primary,
            AutoSize = true,
            Padding = new Padding(0, 6, 8, 0)
        };
        userChip.Controls.Add(userName);

        if (_isAdmin)
            userChip.Controls.Add(Theme.MakeBadge("Admin", Theme.Danger));

        var signOut = new Button { Text = "Sign Out", AutoSize = true };
        Theme.StyleDangerOutline(signOut);
        signOut.Margin = new Padding(12, 2, 0, 0);
        signOut.Click += (_, _) =>
        {
            _sessionStore.Clear();
            ShouldReturnToLogin = true;
            Close();
        };
        userChip.Controls.Add(signOut);

        _header.Layout += (_, _) => { userChip.Left = _header.Width - userChip.PreferredSize.Width - 24; };

        _header.Controls.Add(userChip);
        _header.Controls.Add(brand);

        var navHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 12, 8, 12) };
        var navFlow = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        navHost.Controls.Add(navFlow);

        _sidebar.Controls.Add(navHost);

        if (_isAdmin)
            BuildAdminNav(navFlow);
        else
            BuildStudentNav(navFlow);

        var sidebarFooter = new Label
        {
            Text = _session.Email,
            Dock = DockStyle.Bottom,
            Height = 44,
            ForeColor = Color.FromArgb(200, 220, 230),
            Font = Theme.UiFont(8.5f),
            Padding = new Padding(16, 12, 16, 8),
            AutoEllipsis = true
        };
        _sidebar.Controls.Add(sidebarFooter);
    }

    private void BuildStudentNav(FlowLayoutPanel navFlow)
    {
        navFlow.Controls.Add(MakeNavHeading("Student"));
        navFlow.Controls.Add(NavButton("Dashboard", ShellNav.StudentDashboard, ShowStudentDashboard));
        navFlow.Controls.Add(NavButton("Courses", ShellNav.StudentCourses, ShowStudentCourses));
        navFlow.Controls.Add(NavButton("Study Plan", ShellNav.StudentStudyPlan, ShowStudentStudyPlan));
    }

    private void BuildAdminNav(FlowLayoutPanel navFlow)
    {
        navFlow.Controls.Add(MakeNavHeading("Admin"));
        navFlow.Controls.Add(NavButton("Admin Dashboard", ShellNav.AdminDashboard, ShowAdminDashboard));
        navFlow.Controls.Add(NavButton("Manage Courses", ShellNav.AdminCourses, ShowAdminCourses));
        navFlow.Controls.Add(NavButton("Pending Enrollments", ShellNav.AdminEnrollments, ShowAdminEnrollments));
        navFlow.Controls.Add(NavButton("Student Progress", ShellNav.AdminStudentProgress, ShowAdminStudentProgress));
        navFlow.Controls.Add(NavButton("Analytics", ShellNav.AdminAnalytics, ShowAdminAnalytics));
    }

    private static Label MakeNavHeading(string text) =>
        new()
        {
            Text = text.ToUpperInvariant(),
            Font = Theme.UiFont(8f, FontStyle.Bold),
            ForeColor = Color.FromArgb(160, 190, 200),
            AutoSize = true,
            Margin = new Padding(12, 8, 0, 6),
            Width = 200
        };

    private Button NavButton(string text, ShellNav id, Action handler)
    {
        var b = new Button { Text = text, Width = 212 };
        Theme.StyleSidebarButton(b, false);
        b.Tag = id;
        b.Click += (_, _) => handler();
        _navButtons.Add(b);
        return b;
    }

    private void HighlightNav(ShellNav id)
    {
        // Highlight active menu item.
        foreach (var b in _navButtons)
            Theme.StyleSidebarButton(b, b.Tag is ShellNav nid && nid == id);
    }

    private void SetHost(Control view)
    {
        _contentHost.Controls.Clear();
        view.Dock = DockStyle.Fill;
        _contentHost.Controls.Add(view);
    }

    private void ShowStudentDashboard()
    {
        HighlightNav(ShellNav.StudentDashboard);
        var v = new StudentDashboardView(
            _apiClient,
            _session.Name,
            id =>
            {
                HighlightNav(ShellNav.StudentDashboard);
                SetHost(new CourseProgressView(_apiClient, id, ShowStudentDashboard));
            },
            ShowStudentCourses);
        SetHost(v);
    }

    private void ShowStudentCourses()
    {
        HighlightNav(ShellNav.StudentCourses);
        SetHost(new CoursesView(_apiClient));
    }

    private void ShowStudentStudyPlan()
    {
        HighlightNav(ShellNav.StudentStudyPlan);
        SetHost(new StudyPlanView(_apiClient));
    }

    private async void ShowAdminDashboard()
    {
        HighlightNav(ShellNav.AdminDashboard);
        var v = new AdminDashboardView(_apiClient, ShowAdminCourses, ShowAdminEnrollments, OnAdminCreateCourse);
        SetHost(v);
        await v.RefreshAsync(_session.Name);
    }

    private void ShowAdminCourses()
    {
        HighlightNav(ShellNav.AdminCourses);
        SetHost(new AdminCoursesView(_apiClient));
    }

    private void ShowAdminEnrollments()
    {
        HighlightNav(ShellNav.AdminEnrollments);
        SetHost(new AdminEnrollmentsView(_apiClient));
    }

    private void ShowAdminStudentProgress()
    {
        HighlightNav(ShellNav.AdminStudentProgress);
        SetHost(new AdminStudentProgressView(_apiClient));
    }

    private void ShowAdminAnalytics()
    {
        HighlightNav(ShellNav.AdminAnalytics);
        SetHost(new AdminAnalyticsView(_apiClient));
    }

    private async void OnAdminCreateCourse()
    {
        using var form = new CourseEditForm();
        if (form.ShowDialog(this) == DialogResult.OK && form.CreateDto is not null)
        {
            await _apiClient.PostAsync("/api/courses", form.CreateDto);
            ShowAdminCourses();
        }
    }
}
