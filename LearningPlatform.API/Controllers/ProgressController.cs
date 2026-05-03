using System.Security.Claims;
using LearningPlatform.API.Data;
using LearningPlatform.API.DTOs;
using LearningPlatform.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class ProgressController(AppDbContext db) : ControllerBase
{
    [HttpPost("complete")]
    public async Task<IActionResult> CompleteModule(CompleteModuleDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var module = await db.CourseModules
            .Include(m => m.Course)
            .FirstOrDefaultAsync(m => m.Id == dto.ModuleId);

        if (module is null)
            return NotFound("Modül bulunamadı.");

        var isEnrolled = await db.Enrollments
            .AnyAsync(e => e.UserId == userId
                        && e.CourseId == module.CourseId
                        && e.Status == EnrollmentStatus.Approved);

        if (!isEnrolled)
            return Forbid();

        var alreadyCompleted = await db.ModuleCompletions
            .AnyAsync(mc => mc.UserId == userId && mc.ModuleId == dto.ModuleId);

        if (alreadyCompleted)
            return BadRequest("Bu modülü zaten tamamladınız.");

        db.ModuleCompletions.Add(new ModuleCompletion
        {
            UserId = userId,
            ModuleId = dto.ModuleId
        });

        await db.SaveChangesAsync();

        return Ok(new { message = $"'{module.Title}' modülü tamamlandı." });
    }

    [HttpGet("course/{courseId}")]
    public async Task<IActionResult> GetCourseProgress(int courseId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var course = await db.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound("Kurs bulunamadı.");

        var isEnrolled = await db.Enrollments
            .AnyAsync(e => e.UserId == userId
                        && e.CourseId == courseId
                        && e.Status == EnrollmentStatus.Approved);

        if (!isEnrolled)
            return Forbid();

        var modules = await db.CourseModules
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.Order)
            .ToListAsync();

        var completedModuleIds = await db.ModuleCompletions
            .Where(mc => mc.UserId == userId && mc.Module.CourseId == courseId)
            .ToDictionaryAsync(mc => mc.ModuleId, mc => mc.CompletedAt);

        var moduleDtos = modules.Select(m => new ModuleProgressDto(
            m.Id,
            m.Title,
            m.Order,
            completedModuleIds.ContainsKey(m.Id),
            completedModuleIds.TryGetValue(m.Id, out var d) ? d : null
        )).ToList();

        int total = modules.Count;
        int completed = moduleDtos.Count(m => m.IsCompleted);
        double percentage = total == 0 ? 0 : Math.Round((double)completed / total * 100, 1);

        return Ok(new CourseProgressDto(courseId, course.Title, total, completed, percentage, moduleDtos));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrolledCourses = await db.Enrollments
            .Include(e => e.Course)
                .ThenInclude(c => c.Modules)
            .Where(e => e.UserId == userId && e.Status == EnrollmentStatus.Approved)
            .ToListAsync();

        var completedModuleIds = await db.ModuleCompletions
            .Where(mc => mc.UserId == userId)
            .ToDictionaryAsync(mc => mc.ModuleId, mc => mc.CompletedAt);

        var courseProgresses = enrolledCourses.Select(e =>
        {
            var modules = e.Course.Modules.OrderBy(m => m.Order).ToList();
            int total = modules.Count;
            int completed = modules.Count(m => completedModuleIds.ContainsKey(m.Id));
            double percentage = total == 0 ? 0 : Math.Round((double)completed / total * 100, 1);

            var moduleDtos = modules.Select(m => new ModuleProgressDto(
                m.Id,
                m.Title,
                m.Order,
                completedModuleIds.ContainsKey(m.Id),
                completedModuleIds.TryGetValue(m.Id, out var d) ? d : null
            ));

            return new CourseProgressDto(e.CourseId, e.Course.Title, total, completed, percentage, moduleDtos);
        }).ToList();

        int totalEnrolled = courseProgresses.Count;
        int fullyCompleted = courseProgresses.Count(c => c.TotalModules > 0 && c.CompletedModules == c.TotalModules);
        double overall = totalEnrolled == 0 ? 0
            : Math.Round(courseProgresses.Average(c => c.CompletionPercentage), 1);

        return Ok(new ProgressSummaryDto(totalEnrolled, fullyCompleted, overall, courseProgresses));
    }
}
