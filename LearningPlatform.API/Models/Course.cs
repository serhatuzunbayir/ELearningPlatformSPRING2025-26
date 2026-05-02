namespace LearningPlatform.API.Models;

public class Course
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DifficultyLevel Difficulty { get; set; }
    public int EctsCredit { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CourseModule> Modules { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}
