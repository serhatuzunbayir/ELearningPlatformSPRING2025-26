using System.Data;
using LearningPlatform.Desktop.Forms;
using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;

namespace LearningPlatform.Desktop;

public partial class Form1 : Form
{
    private readonly ApiClient _apiClient;
    private readonly SessionStore _sessionStore;
    private readonly UserSession _session;
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill };
    private readonly Label _header = new() { Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleLeft };
    private readonly Button _logout = new() { Text = "Logout", Dock = DockStyle.Top, Height = 32 };
    public bool ShouldReturnToLogin { get; private set; }

    public Form1(ApiClient apiClient, SessionStore sessionStore, UserSession session)
    {
        _apiClient = apiClient;
        _sessionStore = sessionStore;
        _session = session;
        InitializeComponent();
        Text = $"Learning Platform Desktop - {_session.Role}";
        Width = 1100;
        Height = 720;

        _header.Text = $"  {_session.Name} ({_session.Email}) - Role: {_session.Role}";
        _logout.Click += (_, _) =>
        {
            _sessionStore.Clear();
            ShouldReturnToLogin = true;
            Close();
        };

        Controls.Add(_tabs);
        Controls.Add(_logout);
        Controls.Add(_header);

        if (_session.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            BuildAdminTabs();
        else
            BuildStudentTabs();
    }

    private void BuildStudentTabs()
    {
        _tabs.TabPages.Add(MakeDashboardTab());
        _tabs.TabPages.Add(MakeCoursesTab(false));
        _tabs.TabPages.Add(MakeProgressTab());
        _tabs.TabPages.Add(MakeStudyPlanTab());
    }

    private void BuildAdminTabs()
    {
        _tabs.TabPages.Add(MakeAdminDashboardTab());
        _tabs.TabPages.Add(MakeCoursesTab(true));
        _tabs.TabPages.Add(MakeAdminEnrollmentsTab());
    }

    private TabPage MakeDashboardTab()
    {
        var tab = new TabPage("Dashboard");
        var text = new TextBox { Multiline = true, Dock = DockStyle.Fill, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
        var refresh = new Button { Text = "Refresh", Dock = DockStyle.Top, Height = 32 };
        refresh.Click += async (_, _) =>
        {
            try
            {
                var summary = await _apiClient.GetAsync<ProgressSummaryDto>("/api/progress/summary");
                if (summary is null) return;
                text.Text =
                    $"Total Enrolled Courses: {summary.TotalEnrolledCourses}{Environment.NewLine}" +
                    $"Fully Completed Courses: {summary.FullyCompletedCourses}{Environment.NewLine}" +
                    $"Overall Completion: {summary.OverallCompletionPercentage}%{Environment.NewLine}{Environment.NewLine}" +
                    string.Join(Environment.NewLine, summary.Courses.Select(c =>
                        $"{c.CourseTitle}: {c.CompletedModules}/{c.TotalModules} ({c.CompletionPercentage}%)"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Dashboard");
            }
        };
        tab.Controls.Add(text);
        tab.Controls.Add(refresh);
        return tab;
    }

    private TabPage MakeCoursesTab(bool adminMode)
    {
        var tab = new TabPage(adminMode ? "Courses(Admin)" : "Courses");
        var panel = new Panel { Dock = DockStyle.Top, Height = 44 };
        var category = new TextBox { PlaceholderText = "Category", Left = 8, Top = 8, Width = 160 };
        var difficulty = new ComboBox { Left = 176, Top = 8, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        difficulty.Items.Add("All");
        foreach (var d in Enum.GetNames<DifficultyLevel>()) difficulty.Items.Add(d);
        difficulty.SelectedIndex = 0;
        var load = new Button { Text = "Load", Left = 324, Top = 8, Width = 80 };
        var details = new Button { Text = "Details", Left = 410, Top = 8, Width = 80 };
        var enroll = new Button { Text = "Enroll", Left = 496, Top = 8, Width = 80, Visible = !adminMode };
        var add = new Button { Text = "Add", Left = 582, Top = 8, Width = 80, Visible = adminMode };
        var edit = new Button { Text = "Edit", Left = 668, Top = 8, Width = 80, Visible = adminMode };
        var del = new Button { Text = "Delete", Left = 754, Top = 8, Width = 80, Visible = adminMode };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        List<CourseListDto> cache = [];

        async Task RefreshCourses()
        {
            try
            {
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(category.Text)) query.Add($"category={Uri.EscapeDataString(category.Text.Trim())}");
                if (difficulty.SelectedIndex > 0) query.Add($"difficulty={difficulty.SelectedItem}");
                var endpoint = "/api/courses" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
                cache = await _apiClient.GetAsync<List<CourseListDto>>(endpoint) ?? [];
                grid.DataSource = cache;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Courses");
            }
        }

        int? SelectedCourseId()
        {
            if (grid.CurrentRow?.DataBoundItem is CourseListDto c) return c.Id;
            return null;
        }

        load.Click += async (_, _) => await RefreshCourses();
        details.Click += async (_, _) =>
        {
            var id = SelectedCourseId();
            if (id is null) return;
            var detail = await _apiClient.GetAsync<CourseDetailDto>($"/api/courses/{id.Value}");
            if (detail is null) return;
            var moduleText = string.Join(Environment.NewLine, detail.Modules.OrderBy(m => m.Order).Select(m => $"{m.Order}. {m.Title}"));
            MessageBox.Show(
                $"Title: {detail.Title}{Environment.NewLine}Category: {detail.Category}{Environment.NewLine}" +
                $"Difficulty: {detail.Difficulty}{Environment.NewLine}ECTS: {detail.EctsCredit}{Environment.NewLine}" +
                $"Description: {detail.Description}{Environment.NewLine}{Environment.NewLine}Modules:{Environment.NewLine}{moduleText}",
                "Course detail");
        };
        enroll.Click += async (_, _) =>
        {
            var id = SelectedCourseId();
            if (id is null) return;
            var ok = await _apiClient.PostAsync("/api/enrollments", new EnrollmentRequestDto(id.Value));
            MessageBox.Show(ok ? "Enrollment request sent." : "Enrollment request failed.");
        };
        add.Click += async (_, _) =>
        {
            using var form = new CourseEditForm();
            if (form.ShowDialog(this) == DialogResult.OK && form.CreateDto is not null)
            {
                await _apiClient.PostAsync("/api/courses", form.CreateDto);
                await RefreshCourses();
            }
        };
        edit.Click += async (_, _) =>
        {
            var id = SelectedCourseId();
            if (id is null) return;
            var existing = await _apiClient.GetAsync<CourseDetailDto>($"/api/courses/{id.Value}");
            if (existing is null) return;
            using var form = new CourseEditForm(existing);
            if (form.ShowDialog(this) == DialogResult.OK && form.UpdateDto is not null)
            {
                await _apiClient.PutAsync($"/api/courses/{id.Value}", form.UpdateDto);
                await RefreshCourses();
            }
        };
        del.Click += async (_, _) =>
        {
            var id = SelectedCourseId();
            if (id is null) return;
            if (MessageBox.Show("Delete selected course?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await _apiClient.DeleteAsync($"/api/courses/{id.Value}");
            await RefreshCourses();
        };

        panel.Controls.AddRange([category, difficulty, load, details, enroll, add, edit, del]);
        tab.Controls.Add(grid);
        tab.Controls.Add(panel);
        _ = RefreshCourses();
        return tab;
    }

    private TabPage MakeProgressTab()
    {
        var tab = new TabPage("Progress");
        var top = new Panel { Dock = DockStyle.Top, Height = 44 };
        var courseSelect = new ComboBox { Left = 8, Top = 8, Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
        var load = new Button { Text = "Load", Left = 356, Top = 8, Width = 80 };
        var complete = new Button { Text = "Complete module", Left = 442, Top = 8, Width = 130 };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        List<CourseProgressDto> courses = [];

        async Task LoadSummary()
        {
            var summary = await _apiClient.GetAsync<ProgressSummaryDto>("/api/progress/summary");
            courses = summary?.Courses.ToList() ?? [];
            courseSelect.DataSource = courses;
            courseSelect.DisplayMember = nameof(CourseProgressDto.CourseTitle);
            courseSelect.ValueMember = nameof(CourseProgressDto.CourseId);
        }

        async Task LoadCourseProgress()
        {
            try
            {
                if (courseSelect.SelectedItem is not CourseProgressDto selected) return;
                var progress = await _apiClient.GetAsync<CourseProgressDto>($"/api/progress/course/{selected.CourseId}");
                grid.DataSource = progress?.Modules.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Progress");
            }
        }

        load.Click += async (_, _) => await LoadCourseProgress();
        complete.Click += async (_, _) =>
        {
            if (grid.CurrentRow?.DataBoundItem is not ModuleProgressDto module || module.IsCompleted) return;
            var ok = await _apiClient.PostAsync("/api/progress/complete", new CompleteModuleDto(module.ModuleId));
            MessageBox.Show(ok ? "Module completed." : "Could not complete module.");
            await LoadCourseProgress();
        };

        top.Controls.AddRange([courseSelect, load, complete]);
        tab.Controls.Add(grid);
        tab.Controls.Add(top);
        _ = LoadSummary();
        return tab;
    }

    private TabPage MakeStudyPlanTab()
    {
        var tab = new TabPage("Study Plan");
        var top = new Panel { Dock = DockStyle.Top, Height = 44 };
        var load = new Button { Text = "Load", Left = 8, Top = 8, Width = 90 };
        var generate = new Button { Text = "Generate", Left = 104, Top = 8, Width = 90 };
        var delete = new Button { Text = "Delete", Left = 200, Top = 8, Width = 90 };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };

        async Task LoadPlan()
        {
            try
            {
                var plan = await _apiClient.GetAsync<StudyPlanDto>("/api/studyplan");
                grid.DataSource = plan?.Items.ToList();
            }
            catch (Exception ex)
            {
                grid.DataSource = null;
                MessageBox.Show(ex.Message, "Study plan");
            }
        }

        load.Click += async (_, _) => await LoadPlan();
        generate.Click += async (_, _) =>
        {
            try
            {
                await _apiClient.PostAsync<object, StudyPlanDto>("/api/studyplan/generate", new { });
                await LoadPlan();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Study plan");
            }
        };
        delete.Click += async (_, _) =>
        {
            try
            {
                await _apiClient.DeleteAsync("/api/studyplan");
                await LoadPlan();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Study plan");
            }
        };

        top.Controls.AddRange([load, generate, delete]);
        tab.Controls.Add(grid);
        tab.Controls.Add(top);
        _ = LoadPlan();
        return tab;
    }

    private TabPage MakeAdminDashboardTab()
    {
        var tab = new TabPage("Admin Dashboard");
        var text = new TextBox { Multiline = true, Dock = DockStyle.Fill, ReadOnly = true };
        var refresh = new Button { Text = "Refresh", Dock = DockStyle.Top, Height = 32 };
        refresh.Click += async (_, _) =>
        {
            try
            {
                var courses = await _apiClient.GetAsync<List<CourseListDto>>("/api/courses") ?? [];
                var pending = await _apiClient.GetAsync<List<EnrollmentDto>>("/api/enrollments/pending") ?? [];
                text.Text =
                    $"Total Courses: {courses.Count}{Environment.NewLine}" +
                    $"Pending Enrollments: {pending.Count}{Environment.NewLine}" +
                    $"Total Approved Enrollments: {courses.Sum(c => c.EnrollmentCount)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Admin dashboard");
            }
        };
        tab.Controls.Add(text);
        tab.Controls.Add(refresh);
        return tab;
    }

    private TabPage MakeAdminEnrollmentsTab()
    {
        var tab = new TabPage("Enrollments");
        var top = new Panel { Dock = DockStyle.Top, Height = 44 };
        var load = new Button { Text = "Load Pending", Left = 8, Top = 8, Width = 110 };
        var approve = new Button { Text = "Approve", Left = 124, Top = 8, Width = 90 };
        var reject = new Button { Text = "Reject", Left = 220, Top = 8, Width = 90 };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };
        List<EnrollmentDto> cache = [];

        async Task LoadPending()
        {
            cache = await _apiClient.GetAsync<List<EnrollmentDto>>("/api/enrollments/pending") ?? [];
            grid.DataSource = cache;
        }

        int? SelectedId() => grid.CurrentRow?.DataBoundItem is EnrollmentDto e ? e.Id : null;
        load.Click += async (_, _) => await LoadPending();
        approve.Click += async (_, _) =>
        {
            var id = SelectedId();
            if (id is null) return;
            await _apiClient.PutAsync<object>($"/api/enrollments/{id.Value}/approve");
            await LoadPending();
        };
        reject.Click += async (_, _) =>
        {
            var id = SelectedId();
            if (id is null) return;
            await _apiClient.PutAsync<object>($"/api/enrollments/{id.Value}/reject");
            await LoadPending();
        };

        top.Controls.AddRange([load, approve, reject]);
        tab.Controls.Add(grid);
        tab.Controls.Add(top);
        _ = LoadPending();
        return tab;
    }
}
