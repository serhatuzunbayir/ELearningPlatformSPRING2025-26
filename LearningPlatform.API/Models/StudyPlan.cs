namespace LearningPlatform.API.Models;

public class StudyPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public User User { get; set; } = null!;
    public ICollection<StudyPlanItem> Items { get; set; } = [];
}

public class StudyPlanItem
{
    public int Id { get; set; }
    public int StudyPlanId { get; set; }
    public int CourseId { get; set; }
    public string ScheduledDay { get; set; } = string.Empty;
    public int RecommendedHours { get; set; }

    public StudyPlan StudyPlan { get; set; } = null!;
    public Course Course { get; set; } = null!;
}
