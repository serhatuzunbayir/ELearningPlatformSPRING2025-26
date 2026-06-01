using System.Security.Claims;
using LearningPlatform.API.Data;
using LearningPlatform.API.DTOs;
using LearningPlatform.API.Models;
using LearningPlatform.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class RecommendationsController(AppDbContext db, CourseRecommendationService recommendationService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetForCurrentUser()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var email = User.FindFirstValue(ClaimTypes.Email)!;

        var user = await db.Users.FindAsync(userId);
        if (user is null)
            return Unauthorized();

        var enrolledCourseIds = await db.Enrollments
            .Where(e => e.UserId == userId)
            .Select(e => e.CourseId)
            .ToListAsync();

        var pastCategories = await db.Enrollments
            .Where(e => e.UserId == userId && e.Status == EnrollmentStatus.Approved)
            .Select(e => e.Course.Category)
            .Distinct()
            .ToListAsync();

        var preferred = user.PreferredCategory ?? pastCategories.FirstOrDefault() ?? "Programlama";

        var query = db.Courses
            .Where(c => !enrolledCourseIds.Contains(c.Id))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(preferred))
            query = query.Where(c => c.Category == preferred);

        var candidates = await query
            .OrderBy(c => c.Difficulty)
            .ThenByDescending(c => c.EctsCredit)
            .Take(5)
            .ToListAsync();

        var recommendations = candidates
            .Select(c => new RecommendedCourseDto(
                c.Id,
                c.Title,
                c.Category,
                $"Matches your preference '{preferred}' and you are not enrolled yet."))
            .ToList();

        recommendationService.NotifyRecommended(email, recommendations.Select(r => r.Title));

        return Ok(recommendations);
    }
}
