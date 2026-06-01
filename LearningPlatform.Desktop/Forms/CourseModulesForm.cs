using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Forms;

public class CourseModulesForm : Form
{
    private readonly ApiClient _api;
    private readonly int _courseId;
    private readonly string _courseTitle;
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };

    public CourseModulesForm(ApiClient api, int courseId, string courseTitle)
    {
        _api = api;
        _courseId = courseId;
        _courseTitle = courseTitle;
        Text = $"Modules — {courseTitle}";
        ClientSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterParent;

        var titleBox = new TextBox { PlaceholderText = "Title", Left = 12, Top = 12, Width = 160 };
        var contentBox = new TextBox { PlaceholderText = "Content", Left = 180, Top = 12, Width = 200 };
        var orderBox = new NumericUpDown { Left = 388, Top = 12, Width = 60, Minimum = 1, Maximum = 99, Value = 1 };
        var addBtn = new Button { Text = "Add", Left = 456, Top = 10, Width = 80 };
        Theme.StylePrimaryButton(addBtn);
        var refreshBtn = new Button { Text = "Refresh", Left = 544, Top = 10, Width = 80 };
        Theme.StyleOutlineButton(refreshBtn);

        var top = new Panel { Dock = DockStyle.Top, Height = 48 };
        top.Controls.AddRange([titleBox, contentBox, orderBox, addBtn, refreshBtn]);

        var host = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12) };
        host.Controls.Add(_grid);

        Controls.Add(host);
        Controls.Add(top);

        addBtn.Click += async (_, _) =>
        {
            try
            {
                var dto = new CreateModuleDto(titleBox.Text.Trim(), contentBox.Text.Trim(), (int)orderBox.Value);
                await _api.PostAsync($"/api/courses/{_courseId}/modules", dto);
                await LoadModulesAsync();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        };

        refreshBtn.Click += async (_, _) => await LoadModulesAsync();
        Load += async (_, _) => await LoadModulesAsync();
    }

    private async Task LoadModulesAsync()
    {
        var modules = await _api.GetAsync<List<CourseModuleDetailDto>>($"/api/courses/{_courseId}/modules") ?? [];
        _grid.DataSource = modules.OrderBy(m => m.Order).ToList();
    }
}
