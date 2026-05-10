using LearningPlatform.Desktop.Models;

namespace LearningPlatform.Desktop.UI;

public class CourseCardControl : GlassCardPanel
{
    public CourseCardControl(CourseListDto course, Action onViewDetails)
    {
        Width = 280;
        Height = 230;
        Margin = new Padding(12);
        Padding = new Padding(16, 14, 16, 14);

        var badgeRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };
        badgeRow.Controls.Add(Theme.MakeBadge(course.Category, Theme.Primary));
        badgeRow.Controls.Add(Theme.MakeBadge(course.Difficulty.ToString(), Theme.Secondary));

        var title = new Label
        {
            Text = course.Title,
            Font = Theme.UiFont(12f, FontStyle.Bold),
            ForeColor = Color.FromArgb(33, 37, 41),
            AutoSize = false,
            Width = 240,
            Height = 52
        };

        var meta = new Label
        {
            Text = $"{course.ModuleCount} Modules • {course.EctsCredit} ECTS",
            Font = Theme.UiFont(9f),
            ForeColor = Theme.TextMuted,
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 12)
        };

        var btn = new Button { Text = "View Details →", AutoSize = true };
        Theme.StyleOutlineButton(btn);
        btn.Click += (_, _) => onViewDetails();

        var btnRow = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = false,
            Dock = DockStyle.Bottom,
            Padding = new Padding(0, 8, 0, 0)
        };
        btnRow.Controls.Add(btn);

        var stack = new TableLayoutPanel
        {
            ColumnCount = 1,
            Dock = DockStyle.Fill,
            RowCount = 3,
            Padding = new Padding(0)
        };
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.Controls.Add(badgeRow, 0, 0);
        stack.Controls.Add(title, 0, 1);
        stack.Controls.Add(meta, 0, 2);

        Controls.Add(btnRow);
        Controls.Add(stack);
    }
}
