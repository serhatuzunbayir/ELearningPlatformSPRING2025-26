using LearningPlatform.Desktop.Models;
using LearningPlatform.Desktop.UI;

namespace LearningPlatform.Desktop.Forms;

public class CourseEditForm : Form
{
    private readonly TextBox _title = new() { Width = 320, PlaceholderText = "Title" };
    private readonly TextBox _description = new() { Width = 320, PlaceholderText = "Description", Multiline = true, Height = 80 };
    private readonly TextBox _category = new() { Width = 320, PlaceholderText = "Category" };
    private readonly NumericUpDown _ects = new() { Width = 100, Minimum = 1, Maximum = 30, Value = 5 };
    private readonly ComboBox _difficulty = new() { Width = 160, DropDownStyle = ComboBoxStyle.DropDownList };
    public CreateCourseDto? CreateDto { get; private set; }
    public UpdateCourseDto? UpdateDto { get; private set; }

    public CourseEditForm(CourseDetailDto? existing = null)
    {
        Text = existing is null ? "Create Course" : "Edit Course";
        Width = 400;
        Height = 360;
        StartPosition = FormStartPosition.CenterParent;
        _difficulty.DataSource = Enum.GetValues<DifficultyLevel>();

        if (existing is not null)
        {
            _title.Text = existing.Title;
            _description.Text = existing.Description;
            _category.Text = existing.Category;
            _difficulty.SelectedItem = existing.Difficulty;
            _ects.Value = existing.EctsCredit;
        }

        var save = new Button { Text = "Save", Width = 140, Height = 36 };
        Theme.StylePrimaryButton(save);
        save.Click += (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(_title.Text) || string.IsNullOrWhiteSpace(_category.Text))
            {
                MessageBox.Show("Title and category are required.");
                return;
            }

            if (existing is null)
            {
                CreateDto = new CreateCourseDto(
                    _title.Text.Trim(), _description.Text.Trim(), _category.Text.Trim(),
                    (DifficultyLevel)_difficulty.SelectedItem!, (int)_ects.Value);
            }
            else
            {
                UpdateDto = new UpdateCourseDto(
                    _title.Text.Trim(), _description.Text.Trim(), _category.Text.Trim(),
                    (DifficultyLevel)_difficulty.SelectedItem!, (int)_ects.Value);
            }
            DialogResult = DialogResult.OK;
            Close();
        };

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            Padding = new Padding(20),
            WrapContents = false
        };
        panel.Controls.AddRange([_title, _description, _category, _difficulty, _ects, save]);
        Controls.Add(panel);
    }
}
