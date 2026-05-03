namespace LearningPlatform.API.Services;

public delegate void StudyPlanGeneratedHandler(string studentName, IEnumerable<string> recommendedCourseTitles);

public class StudyPlanService
{
    public event StudyPlanGeneratedHandler? OnPlanGenerated;

    public StudyPlanService()
    {
        OnPlanGenerated += (studentName, courses) =>
        {
            var list = string.Join(", ", courses.Select(c => $"'{c}'"));
            Console.WriteLine($"[Öneri] {studentName} için çalışma planı oluşturuldu. Önerilen kurslar: {list}");
        };
    }

    public void NotifyGenerated(string studentName, IEnumerable<string> recommendedCourseTitles) =>
        OnPlanGenerated?.Invoke(studentName, recommendedCourseTitles);
}
