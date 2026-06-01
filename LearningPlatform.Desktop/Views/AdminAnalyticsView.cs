using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class AdminAnalyticsView : UserControl
{
    public AdminAnalyticsView(ApiClient api)
    {
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        var title = new Label { Text = "Course Analytics", Font = Theme.UiFont(18f, FontStyle.Bold), Dock = DockStyle.Top, AutoSize = true };
        var summary = new Label { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 12, 0, 12) };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true };

        Controls.Add(grid);
        Controls.Add(summary);
        Controls.Add(title);

        Load += async (_, _) =>
        {
            try
            {
                var data = await api.GetAsync<AdminAnalyticsDto>("/api/admin/analytics");
                if (data is null) return;
                summary.Text = $"Courses: {data.TotalCourses}   Students: {data.TotalStudents}   Pending enrollments: {data.PendingEnrollmentRequests}";
                grid.DataSource = data.Courses.ToList();
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Analytics"); }
        };
    }
}
