using LearningPlatform.API.Data;
using LearningPlatform.API.DTOs;
using LearningPlatform.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController(AppDbContext db) : ControllerBase
{
    [HttpGet("students/progress")]
    public async Task<IActionResult> GetStudentProgress()
    {
        var enrollments = await db.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
                .ThenInclude(c => c.Modules)
            .Where(e => e.Status == EnrollmentStatus.Approved)
            .ToListAsync();

        var completionLookup = await db.ModuleCompletions
            .Include(mc => mc.Module)
            .GroupBy(mc => new { mc.UserId, CourseId = mc.Module.CourseId })
            .Select(g => new { g.Key.UserId, g.Key.CourseId, Count = g.Count() })
            .ToListAsync();

        // Build one progress row per approved enrollment.
        var rows = enrollments.Select(e =>
        {
            int total = e.Course.Modules.Count;
            int completed = completionLookup
                .FirstOrDefault(x => x.UserId == e.UserId && x.CourseId == e.CourseId)?.Count ?? 0;
            double pct = total == 0 ? 0 : Math.Round((double)completed / total * 100, 1);

            return new StudentCourseProgressDto(
                e.UserId,
                e.User.Name,
                e.User.Email,
                e.CourseId,
                e.Course.Title,
                completed,
                total,
                pct);
        })
        .OrderBy(r => r.StudentName)
        .ThenBy(r => r.CourseTitle)
        .ToList();

        return Ok(rows);
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var courses = await db.Courses
            .Include(c => c.Modules)
            .Include(c => c.Enrollments)
            .ToListAsync();

        var completions = await db.ModuleCompletions
            .Include(mc => mc.Module)
            .Select(mc => new { mc.UserId, CourseId = mc.Module.CourseId })
            .ToListAsync();

        // Build analytics for each course.
        var courseAnalytics = courses.Select(c =>
        {
            var approved = c.Enrollments.Where(e => e.Status == EnrollmentStatus.Approved).ToList();
            int approvedCount = approved.Count;

            double avgCompletion = 0;
            if (approvedCount > 0 && c.Modules.Count > 0)
            {
                var percentages = approved.Select(e =>
                {
                    int done = completions.Count(x => x.UserId == e.UserId && x.CourseId == c.Id);
                    return (double)done / c.Modules.Count * 100;
                });
                avgCompletion = Math.Round(percentages.Average(), 1);
            }

            return new CourseAnalyticsDto(
                c.Id,
                c.Title,
                c.Enrollments.Count,
                approvedCount,
                c.Enrollments.Count(e => e.Status == EnrollmentStatus.Pending),
                avgCompletion);
        }).ToList();

        var dto = new AdminAnalyticsDto(
            courses.Count,
            await db.Users.CountAsync(u => u.Role == UserRole.Student),
            await db.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Pending),
            courseAnalytics);

        return Ok(dto);
    }
}
