using LearningPlatform.API.Models;

namespace LearningPlatform.API.DTOs;

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
