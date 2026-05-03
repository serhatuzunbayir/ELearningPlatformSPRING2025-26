namespace LearningPlatform.API.DTOs;

public record StudyPlanItemDto(
    int CourseId,
    string CourseTitle,
    string Category,
    string ScheduledDay,
    int RecommendedHours
);

public record StudyPlanDto(
    int Id,
    DateTime GeneratedAt,
    IEnumerable<StudyPlanItemDto> Items
);
