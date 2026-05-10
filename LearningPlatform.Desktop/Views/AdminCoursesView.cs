using LearningPlatform.Desktop.Forms;
using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class AdminCoursesView : UserControl
{
    private readonly ApiClient _api;

    public AdminCoursesView(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        var title = new Label { Text = "Manage Courses", Font = Theme.UiFont(18f, FontStyle.Bold), Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 0, 0, 12) };

        var toolbar = new GlassCardPanel { Dock = DockStyle.Top, Height = 60, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 12) };
        var category = new TextBox { PlaceholderText = "Category", Left = 8, Top = 14, Width = 160 };
        var difficulty = new ComboBox { Left = 176, Top = 14, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
        difficulty.Items.Add("All");
        foreach (var d in Enum.GetNames<DifficultyLevel>()) difficulty.Items.Add(d);
        difficulty.SelectedIndex = 0;
        var load = new Button { Text = "Load", Left = 324, Top = 12, Width = 88 };
        Theme.StyleOutlineButton(load);
        var add = new Button { Text = "Add", Left = 418, Top = 12, Width = 88 };
        Theme.StylePrimaryButton(add);
        var edit = new Button { Text = "Edit", Left = 512, Top = 12, Width = 88 };
        Theme.StyleOutlineButton(edit);
        var del = new Button { Text = "Delete", Left = 606, Top = 12, Width = 88 };
        Theme.StyleDangerOutline(del);

        toolbar.Controls.AddRange([category, difficulty, load, add, edit, del]);

        var gridHost = new GlassCardPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, BackgroundColor = Theme.CardBackground, BorderStyle = BorderStyle.None };
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.PageBackground;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
        grid.ColumnHeadersDefaultCellStyle.Font = Theme.UiFont(9f, FontStyle.Bold);
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridHost.Controls.Add(grid);

        List<CourseListDto> cache = [];

        async Task RefreshCourses()
        {
            try
            {
                var query = new List<string>();
                if (!string.IsNullOrWhiteSpace(category.Text)) query.Add($"category={Uri.EscapeDataString(category.Text.Trim())}");
                if (difficulty.SelectedIndex > 0) query.Add($"difficulty={difficulty.SelectedItem}");
                var endpoint = "/api/courses" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
                cache = await _api.GetAsync<List<CourseListDto>>(endpoint) ?? [];
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
        add.Click += async (_, _) =>
        {
            using var form = new CourseEditForm();
            if (form.ShowDialog(FindForm()) == DialogResult.OK && form.CreateDto is not null)
            {
                await _api.PostAsync("/api/courses", form.CreateDto);
                await RefreshCourses();
            }
        };
        edit.Click += async (_, _) =>
        {
            var id = SelectedCourseId();
            if (id is null) return;
            var existing = await _api.GetAsync<CourseDetailDto>($"/api/courses/{id.Value}");
            if (existing is null) return;
            using var form = new CourseEditForm(existing);
            if (form.ShowDialog(FindForm()) == DialogResult.OK && form.UpdateDto is not null)
            {
                await _api.PutAsync($"/api/courses/{id.Value}", form.UpdateDto);
                await RefreshCourses();
            }
        };
        del.Click += async (_, _) =>
        {
            var id = SelectedCourseId();
            if (id is null) return;
            if (MessageBox.Show("Delete selected course?", "Confirm", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await _api.DeleteAsync($"/api/courses/{id.Value}");
            await RefreshCourses();
        };

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.Controls.Add(title, 0, 0);
        root.Controls.Add(toolbar, 0, 1);
        root.Controls.Add(gridHost, 0, 2);

        Controls.Add(root);

        Load += async (_, _) => await RefreshCourses();
    }
}
