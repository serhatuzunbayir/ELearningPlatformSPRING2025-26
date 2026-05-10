using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Forms;

public class CourseDetailForm : Form
{
    private readonly ApiClient _apiClient;
    private readonly CourseDetailDto _detail;

    public CourseDetailForm(ApiClient apiClient, CourseDetailDto detail)
    {
        _apiClient = apiClient;
        _detail = detail;
        Text = detail.Title;
        Size = new Size(680, 680);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.PageBackground;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(24, 24, 24, 32) };

        var card = new GlassCardPanel { Dock = DockStyle.Top, Padding = new Padding(28, 28, 28, 32), Width = 620 };
        scroll.Controls.Add(card);
        scroll.Resize += (_, _) => card.Width = Math.Max(480, scroll.ClientSize.Width - scroll.Padding.Left - scroll.Padding.Right);

        var backBtn = new LinkLabel { Text = "← Back to courses", AutoSize = true, LinkColor = Theme.Primary, Margin = new Padding(0, 0, 0, 16) };
        backBtn.LinkClicked += (_, _) => Close();

        var title = new Label
        {
            Text = detail.Title,
            Font = Theme.UiFont(18f, FontStyle.Bold),
            AutoSize = false,
            Width = 560,
            Height = 52,
            Margin = new Padding(0, 0, 0, 12)
        };

        var badgeRow = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 0, 0, 16) };
        badgeRow.Controls.Add(Theme.MakeBadge(detail.Category, Theme.Primary));
        badgeRow.Controls.Add(Theme.MakeBadge(detail.Difficulty.ToString(), Theme.Secondary));
        badgeRow.Controls.Add(new Label
        {
            Text = $"{detail.EctsCredit} ECTS",
            Font = Theme.UiFont(11f, FontStyle.Bold),
            ForeColor = Theme.Success,
            AutoSize = true,
            Margin = new Padding(16, 6, 0, 0)
        });

        var desc = new TextBox
        {
            Multiline = true,
            ReadOnly = true,
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Theme.CardBackground,
            Text = detail.Description,
            Font = Theme.UiFont(10f),
            ForeColor = Theme.TextMuted,
            Width = 560,
            Height = 140,
            ScrollBars = ScrollBars.Vertical,
            Margin = new Padding(0, 0, 0, 20)
        };

        var modulesTitle = new Label
        {
            Text = $"Course Modules ({detail.Modules.Count})",
            Font = Theme.UiFont(11f, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 12)
        };

        var modulesFlow = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false, Margin = new Padding(0, 0, 0, 24) };

        foreach (var m in detail.Modules.OrderBy(x => x.Order))
        {
            var row = new Panel { Width = 560, Height = 46, Margin = new Padding(0, 0, 0, 6), BackColor = Theme.PageBackground, Padding = new Padding(12, 12, 12, 12) };
            row.Controls.Add(new Label
            {
                Text = $"Module {m.Order}: {m.Title}",
                Font = Theme.UiFont(9.5f, FontStyle.Bold),
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(33, 37, 41)
            });
            modulesFlow.Controls.Add(row);
        }

        if (!detail.Modules.Any())
        {
            modulesFlow.Controls.Add(new Label { Text = "No modules have been added to this course yet.", ForeColor = Theme.TextMuted, AutoSize = true });
        }

        var enrollBtn = new Button { Text = "Send Enrollment Request", Width = 280, Height = 42 };
        Theme.StylePrimaryButton(enrollBtn);
        enrollBtn.Margin = new Padding(0, 8, 0, 8);
        enrollBtn.Click += async (_, _) =>
        {
            try
            {
                var ok = await _apiClient.PostAsync("/api/enrollments", new EnrollmentRequestDto(_detail.Id));
                MessageBox.Show(ok ? "Enrollment request sent." : "Enrollment request failed.", Text);
                if (ok) Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text);
            }
        };

        var enrollNote = new Label
        {
            Text = "By requesting enrollment, your request will be sent to an administrator for approval.",
            Font = Theme.UiFont(8.5f),
            ForeColor = Theme.TextMuted,
            MaximumSize = new Size(560, 0),
            AutoSize = true
        };

        var stack = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, AutoSize = true, WrapContents = false };
        stack.Controls.Add(backBtn);
        stack.Controls.Add(title);
        stack.Controls.Add(badgeRow);
        stack.Controls.Add(desc);
        stack.Controls.Add(modulesTitle);
        stack.Controls.Add(modulesFlow);
        stack.Controls.Add(enrollBtn);
        stack.Controls.Add(enrollNote);

        card.Controls.Add(stack);
        Controls.Add(scroll);
    }
}
