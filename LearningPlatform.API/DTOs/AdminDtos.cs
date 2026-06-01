namespace LearningPlatform.API.DTOs;

public record StudentCourseProgressDto(
    int StudentId,
    string StudentName,
    string StudentEmail,
    int CourseId,
    string CourseTitle,
    int CompletedModules,
    int TotalModules,
    double CompletionPercentage
);

public record CourseAnalyticsDto(
    int CourseId,
    string CourseTitle,
    int TotalEnrollments,
    int ApprovedEnrollments,
    int PendingEnrollments,
    double AverageCompletionPercentage
);

public record AdminAnalyticsDto(
    int TotalCourses,
    int TotalStudents,
    int PendingEnrollmentRequests,
    IEnumerable<CourseAnalyticsDto> Courses
);
