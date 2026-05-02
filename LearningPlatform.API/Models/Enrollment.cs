namespace LearningPlatform.API.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Pending;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; set; }

    public User User { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

public enum EnrollmentStatus
{
    Pending,
    Approved,
    Rejected
}
