using LearningPlatform.Desktop.Forms;
using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class CoursesView : UserControl
{
    private readonly ApiClient _api;
    private readonly FlowLayoutPanel _cards = new() { AutoScroll = true, Dock = DockStyle.Fill, WrapContents = true, Padding = new Padding(8) };
    private readonly TextBox _category = new() { PlaceholderText = "Search by Category (e.g. Programming)", Width = 260 };
    private readonly ComboBox _difficulty = new() { Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };

    public CoursesView(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        _difficulty.Items.Add("All Difficulties");
        foreach (var d in Enum.GetNames<DifficultyLevel>())
            _difficulty.Items.Add(d);
        _difficulty.SelectedIndex = 0;

        var filterCard = new GlassCardPanel { Dock = DockStyle.Fill, Padding = new Padding(16) };
        var filterInner = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Dock = DockStyle.Fill, WrapContents = false };

        var btnFilter = new Button { Text = "Filter", AutoSize = true };
        Theme.StylePrimaryButton(btnFilter);
        btnFilter.Click += async (_, _) => await LoadCoursesAsync();

        var btnClear = new Button { Text = "Clear", AutoSize = true };
        Theme.StyleOutlineButton(btnClear);
        btnClear.Margin = new Padding(12, 0, 0, 0);
        btnClear.Click += (_, _) =>
        {
            _category.Clear();
            _difficulty.SelectedIndex = 0;
            _ = LoadCoursesAsync();
        };

        filterInner.Controls.Add(_category);
        filterInner.Controls.Add(_difficulty);
        filterInner.Controls.Add(btnFilter);
        filterInner.Controls.Add(btnClear);
        filterCard.Controls.Add(filterInner);

        var title = new Label { Text = "Available Courses", Font = Theme.UiFont(18f, FontStyle.Bold), Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 8, 0, 12) };

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.Controls.Add(filterCard, 0, 0);
        root.Controls.Add(title, 0, 1);
        root.Controls.Add(_cards, 0, 2);

        Controls.Add(root);

        Load += async (_, _) => await LoadCoursesAsync();
    }

    private async Task LoadCoursesAsync()
    {
        try
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(_category.Text))
                query.Add($"category={Uri.EscapeDataString(_category.Text.Trim())}");
            if (_difficulty.SelectedIndex > 0)
                query.Add($"difficulty={_difficulty.SelectedItem}");
            var endpoint = "/api/courses" + (query.Count > 0 ? "?" + string.Join("&", query) : string.Empty);
            var list = await _api.GetAsync<List<CourseListDto>>(endpoint) ?? [];

            _cards.Controls.Clear();
            foreach (var c in list)
            {
                var card = new CourseCardControl(c, async () =>
                {
                    var detail = await _api.GetAsync<CourseDetailDto>($"/api/courses/{c.Id}");
                    if (detail is null) return;
                    using var dlg = new CourseDetailForm(_api, detail);
                    dlg.ShowDialog(FindForm());
                });
                _cards.Controls.Add(card);
            }

            if (!list.Any())
            {
                _cards.Controls.Add(new Label
                {
                    Text = "No courses available right now. Check back later!",
                    Font = Theme.UiFont(11f),
                    ForeColor = Theme.TextMuted,
                    Margin = new Padding(24),
                    AutoSize = true
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Courses");
        }
    }
}
