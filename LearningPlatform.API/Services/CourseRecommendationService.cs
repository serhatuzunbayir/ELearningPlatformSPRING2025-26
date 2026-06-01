namespace LearningPlatform.API.Services;

public delegate void CoursesRecommendedHandler(string studentEmail, IEnumerable<string> recommendedCourseTitles);

/// <summary>
/// FR8 — recommends catalog courses using LINQ and raises a delegate when recommendations are produced.
/// </summary>
public class CourseRecommendationService
{
    public event CoursesRecommendedHandler? OnCoursesRecommended;

    public CourseRecommendationService()
    {
        OnCoursesRecommended += (email, titles) =>
        {
            var list = string.Join(", ", titles.Select(t => $"'{t}'"));
            Console.WriteLine($"[Öneri] {email} için kurs önerileri: {list}");
        };
    }

    public void NotifyRecommended(string studentEmail, IEnumerable<string> courseTitles) =>
        OnCoursesRecommended?.Invoke(studentEmail, courseTitles);
}
