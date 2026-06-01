using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class AdminStudentProgressView : UserControl
{
    public AdminStudentProgressView(ApiClient api)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        var title = new Label { Text = "Student Progress", Font = Theme.UiFont(18f, FontStyle.Bold), Dock = DockStyle.Top, AutoSize = true };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };

        Controls.Add(grid);
        Controls.Add(title);

        Load += async (_, _) =>
        {
            try
            {
                var rows = await api.GetAsync<List<StudentCourseProgressDto>>("/api/admin/students/progress") ?? [];
                grid.DataSource = rows;
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Progress"); }
        };
    }
}
