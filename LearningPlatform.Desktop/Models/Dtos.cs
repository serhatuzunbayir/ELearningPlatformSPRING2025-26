namespace LearningPlatform.Desktop.Models;

public record RegisterDto(string Name, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponseDto(string Token, string Name, string Email, string Role);

public enum DifficultyLevel
{
    Beginner,
    Intermediate,
    Advanced
}

public record CourseListDto(
    int Id,
    string Title,
    string Category,
    DifficultyLevel Difficulty,
    int EctsCredit,
    int ModuleCount,
    int EnrollmentCount
);

public record CourseDetailDto(
    int Id,
    string Title,
    string Description,
    string Category,
    DifficultyLevel Difficulty,
    int EctsCredit,
    List<CourseModuleDto> Modules
);

public record CourseModuleDto(int Id, string Title, int Order);
public record CreateCourseDto(string Title, string Description, string Category, DifficultyLevel Difficulty, int EctsCredit);
public record UpdateCourseDto(string Title, string Description, string Category, DifficultyLevel Difficulty, int EctsCredit);

public enum EnrollmentStatus
{
    Pending,
    Approved,
    Rejected
}

public record EnrollmentRequestDto(int CourseId);
public record EnrollmentDto(
    int Id,
    string StudentName,
    string StudentEmail,
    string CourseTitle,
    EnrollmentStatus Status,
    DateTime RequestedAt,
    DateTime? ApprovedAt
);

public record CompleteModuleDto(int ModuleId);
public record ModuleProgressDto(int ModuleId, string Title, int Order, bool IsCompleted, DateTime? CompletedAt);
public record CourseProgressDto(
    int CourseId,
    string CourseTitle,
    int TotalModules,
    int CompletedModules,
    double CompletionPercentage,
    IEnumerable<ModuleProgressDto> Modules
);
public record ProgressSummaryDto(
    int TotalEnrolledCourses,
    int FullyCompletedCourses,
    double OverallCompletionPercentage,
    IEnumerable<CourseProgressDto> Courses
);

public record StudyPlanItemDto(int CourseId, string CourseTitle, string Category, string ScheduledDay, int RecommendedHours);
public record StudyPlanDto(int Id, DateTime GeneratedAt, IEnumerable<StudyPlanItemDto> Items);

public record UserSession(string Token, string Name, string Email, string Role);
