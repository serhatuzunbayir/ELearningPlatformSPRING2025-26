namespace LearningPlatform.API.DTOs;

public record CompleteModuleDto(int ModuleId);

public record ModuleProgressDto(
    int ModuleId,
    string Title,
    int Order,
    bool IsCompleted,
    DateTime? CompletedAt
);

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
