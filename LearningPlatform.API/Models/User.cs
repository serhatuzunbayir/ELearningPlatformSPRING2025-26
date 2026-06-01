namespace LearningPlatform.API.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Student learning preference used for course recommendations (FR8).</summary>
    public string? PreferredCategory { get; set; }

    public bool TwoFactorEnabled { get; set; }

    public string? TwoFactorCode { get; set; }

    public DateTime? TwoFactorCodeExpiry { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public StudyPlan? StudyPlan { get; set; }
}

public enum UserRole
{
    Student,
    Admin
}
