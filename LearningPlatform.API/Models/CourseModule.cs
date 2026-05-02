namespace LearningPlatform.API.Models;

public class CourseModule
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Order { get; set; }

    public Course Course { get; set; } = null!;
    public ICollection<ModuleCompletion> Completions { get; set; } = [];
}
