namespace LearningPlatform.API.Models;

public class ModuleCompletion
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ModuleId { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public CourseModule Module { get; set; } = null!;
}
