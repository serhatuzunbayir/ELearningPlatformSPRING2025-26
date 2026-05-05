namespace learning_platform.Models;

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

public record CreateCourseDto(
    string Title,
    string Description,
    string Category,
    DifficultyLevel Difficulty,
    int EctsCredit
);

public record UpdateCourseDto(
    string Title,
    string Description,
    string Category,
    DifficultyLevel Difficulty,
    int EctsCredit
);
