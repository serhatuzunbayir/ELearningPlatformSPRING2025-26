namespace LearningPlatform.API.DTOs;

public record CreateModuleDto(string Title, string Content, int Order);

public record UpdateModuleDto(string Title, string Content, int Order);

public record CourseModuleDetailDto(int Id, string Title, string Content, int Order);
