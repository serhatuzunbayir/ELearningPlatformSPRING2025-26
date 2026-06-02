using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.Services;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Views;

public class StudyPlanView : UserControl
{
    private readonly ApiClient _api;
    private readonly TableLayoutPanel _body = new() { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };

    public StudyPlanView(ApiClient api)
    {
        _api = api;
        Dock = DockStyle.Fill;
        BackColor = Theme.PageBackground;
        Padding = new Padding(24);

        var topStrip = new Panel { Dock = DockStyle.Top, Height = 32, Padding = new Padding(8, 6, 8, 0) };
        var advLink = new LinkLabel { Text = "Advanced", AutoSize = true, LinkColor = Theme.Primary };
        var ctx = new ContextMenuStrip();
        ctx.Items.Add("Delete study plan…", null, (_, _) => OnAdvancedDelete());
        advLink.ContextMenuStrip = ctx;
        advLink.Click += (_, _) => ctx.Show(advLink, new Point(0, advLink.Height));
        topStrip.Controls.Add(advLink);

        _body.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _body.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

        Controls.Add(_body);
        Controls.Add(topStrip);

        Load += async (_, _) => await RefreshAsync();
    }

    private async void OnAdvancedDelete()
    {
        var first = MessageBox.Show(
            "Delete your study plan? This cannot be undone from the desktop app.",
            "Advanced",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (first != DialogResult.Yes) return;

        var second = MessageBox.Show(
            "Please confirm again: permanently delete the current study plan?",
            "Confirm delete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (second != DialogResult.Yes) return;

        try
        {
            await _api.DeleteAsync("/api/studyplan");
            await RefreshAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Study plan");
        }
    }

    private async Task GenerateAsync()
    {
        try
        {
            await _api.PostAsync<object, StudyPlanDto>("/api/studyplan/generate", new { });
            await RefreshAsync();
            MessageBox.Show("Study plan generated successfully based on your approved enrollments!", "Study Plan");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Study plan");
        }
    }

    private async Task RefreshAsync()
    {
        StudyPlanDto? plan = null;
        try
        {
            plan = await _api.GetAsync<StudyPlanDto>("/api/studyplan");
        }
        catch (Exception ex)
        {
            if (!ex.Message.Contains("404"))
            {
                MessageBox.Show(ex.Message, "Study plan");
                return;
            }
        }

        Render(plan);
    }

    private void Render(StudyPlanDto? plan)
    {
        while (_body.Controls.Count > 0)
        {
            var c = _body.Controls[0];
            _body.Controls.Remove(c);
            c.Dispose();
        }

        var headerBar = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, Padding = new Padding(0, 0, 0, 12) };
        headerBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70f));
        headerBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));

        var title = new Label { Text = "Your Study Plan", Font = Theme.UiFont(18f, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Fill };

        if (plan is null)
        {
            var btnGenerateTop = new Button { Text = "Generate Plan", AutoSize = true, Anchor = AnchorStyles.Right };
            Theme.StylePrimaryButton(btnGenerateTop);
            btnGenerateTop.Click += async (_, _) => await GenerateAsync();
            headerBar.Controls.Add(title, 0, 0);
            headerBar.Controls.Add(btnGenerateTop, 1, 0);
        }
        else
        {
            headerBar.Controls.Add(title, 0, 0);
            headerBar.SetColumnSpan(title, 2);
        }

        var scrollHost = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(4) };

        if (plan is null)
        {
            var empty = new GlassCardPanel { Width = Math.Max(600, ClientSize.Width - 80), Height = 260, Margin = new Padding(8), Padding = new Padding(32) };
            empty.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            empty.Controls.Add(new Label
            {
                Text = "No Study Plan Found",
                Font = Theme.UiFont(15f, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 16, 0, 12)
            });
            empty.Controls.Add(new Label
            {
                Text = "You don't have a study plan yet. Click \"Generate Plan\" to let the system generate a personalized study plan based on your progress, enrolled courses, and ECTS credit load.",
                Font = Theme.UiFont(10f),
                ForeColor = Theme.TextMuted,
                Dock = DockStyle.Top,
                MaximumSize = new Size(640, 0),
                AutoSize = true
            });
            var btn = new Button { Text = "Generate Plan", AutoSize = true, Margin = new Padding(0, 24, 0, 0) };
            Theme.StylePrimaryButton(btn);
            btn.Click += async (_, _) => await GenerateAsync();
            empty.Controls.Add(btn);
            scrollHost.Controls.Add(empty);
        }
        else
        {
            var info = new Panel { Width = scrollHost.ClientSize.Width - 24, Height = 44, Margin = new Padding(8, 0, 8, 12), BackColor = Theme.Info, Padding = new Padding(16, 12, 16, 12) };
            info.Controls.Add(new Label
            {
                Text = $"This plan was generated on {plan.GeneratedAt:g}.",
                Dock = DockStyle.Fill,
                ForeColor = Color.FromArgb(8, 66, 74),
                Font = Theme.UiFont(9.5f)
            });
            scrollHost.Controls.Add(info);

            var grid = new FlowLayoutPanel
            {
                AutoSize = true,
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(4),
                MaximumSize = new Size(scrollHost.ClientSize.Width, 0),
                Width = scrollHost.ClientSize.Width - 8
            };

            // One card per day.
            foreach (var dayGroup in plan.Items.GroupBy(i => i.ScheduledDay).OrderBy(g => g.Key))
            {
                var card = new GlassCardPanel { Width = 280, Margin = new Padding(12), Padding = new Padding(0) };

                var head = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Theme.Primary, Padding = new Padding(12, 10, 12, 10) };
                head.Controls.Add(new Label { Text = dayGroup.Key, ForeColor = Color.White, Font = Theme.UiFont(10f, FontStyle.Bold), Dock = DockStyle.Fill });

                var listHost = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, AutoSize = true, Padding = new Padding(8), WrapContents = false };

                // Add course rows for this day.
                foreach (var item in dayGroup)
                {
                    var row = new Panel { Width = 252, AutoSize = true, Margin = new Padding(0, 6, 0, 6), Padding = new Padding(8), BackColor = Theme.PageBackground };
                    var badges = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Dock = DockStyle.Top };
                    badges.Controls.Add(Theme.MakeBadge(item.Category, Theme.Secondary));
                    badges.Controls.Add(new Label
                    {
                        Text = $"{item.RecommendedHours} h",
                        ForeColor = Theme.Primary,
                        Font = Theme.UiFont(9f, FontStyle.Bold),
                        AutoSize = true,
                        Margin = new Padding(8, 4, 0, 0)
                    });
                    row.Controls.Add(badges);
                    row.Controls.Add(new Label { Text = item.CourseTitle, Font = Theme.UiFont(10f, FontStyle.Bold), AutoSize = true, Dock = DockStyle.Bottom });
                    listHost.Controls.Add(row);
                }

                card.Controls.Add(listHost);
                card.Controls.Add(head);
                listHost.PerformLayout();
                card.Height = head.Height + listHost.PreferredSize.Height + 24;
                grid.Controls.Add(card);
            }

            scrollHost.Controls.Add(grid);

            var regenWrap = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8, 16, 8, 8) };
            var regen = new Button { Text = "Regenerate Plan", AutoSize = true };
            Theme.StyleOutlineButton(regen);
            regen.Click += async (_, _) => await GenerateAsync();
            regenWrap.Controls.Add(regen);
            scrollHost.Controls.Add(regenWrap);
        }

        _body.Controls.Add(headerBar, 0, 0);
        _body.Controls.Add(scrollHost, 0, 1);
    }
}
