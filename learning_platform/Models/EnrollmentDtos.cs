namespace learning_platform.Models;

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
