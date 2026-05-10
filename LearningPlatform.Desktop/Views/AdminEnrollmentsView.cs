using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class AdminEnrollmentsView : UserControl
{
    private readonly ApiClient _api;

    public AdminEnrollmentsView(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        var title = new Label { Text = "Pending Enrollments", Font = Theme.UiFont(18f, FontStyle.Bold), Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 0, 0, 12) };

        var toolbar = new GlassCardPanel { Dock = DockStyle.Top, Height = 56, Padding = new Padding(12), Margin = new Padding(0, 0, 0, 12) };
        var load = new Button { Text = "Load Pending", Left = 8, Top = 12, Width = 120 };
        Theme.StyleOutlineButton(load);
        var approve = new Button { Text = "Approve", Left = 136, Top = 12, Width = 96 };
        Theme.StylePrimaryButton(approve);
        approve.BackColor = Theme.Success;
        var reject = new Button { Text = "Reject", Left = 240, Top = 12, Width = 96 };
        Theme.StyleDangerOutline(reject);
        toolbar.Controls.AddRange([load, approve, reject]);

        var gridHost = new GlassCardPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
        var grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, BackgroundColor = Theme.CardBackground, BorderStyle = BorderStyle.None };
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.PageBackground;
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(33, 37, 41);
        grid.ColumnHeadersDefaultCellStyle.Font = Theme.UiFont(9f, FontStyle.Bold);
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridHost.Controls.Add(grid);

        List<EnrollmentDto> cache = [];

        async Task LoadPending()
        {
            cache = await _api.GetAsync<List<EnrollmentDto>>("/api/enrollments/pending") ?? [];
            grid.DataSource = cache;
        }

        int? SelectedId() => grid.CurrentRow?.DataBoundItem is EnrollmentDto e ? e.Id : null;

        load.Click += async (_, _) => await LoadPending();
        approve.Click += async (_, _) =>
        {
            var id = SelectedId();
            if (id is null) return;
            await _api.PutAsync<object>($"/api/enrollments/{id.Value}/approve");
            await LoadPending();
        };
        reject.Click += async (_, _) =>
        {
            var id = SelectedId();
            if (id is null) return;
            await _api.PutAsync<object>($"/api/enrollments/{id.Value}/reject");
            await LoadPending();
        };

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
        root.Controls.Add(title, 0, 0);
        root.Controls.Add(toolbar, 0, 1);
        root.Controls.Add(gridHost, 0, 2);

        Controls.Add(root);

        Load += async (_, _) => await LoadPending();
    }
}
