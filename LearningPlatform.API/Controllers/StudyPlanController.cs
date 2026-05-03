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
public class StudyPlanController(AppDbContext db, StudyPlanService studyPlanService) : ControllerBase
{
    private static readonly string[] Days = ["Pazartesi", "Salı", "Çarşamba", "Perşembe", "Cuma", "Cumartesi", "Pazar"];

    [HttpPost("generate")]
    public async Task<IActionResult> Generate()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var userName = User.FindFirstValue(ClaimTypes.Email)!;

        var approvedCourses = await db.Enrollments
            .Include(e => e.Course)
            .Where(e => e.UserId == userId && e.Status == EnrollmentStatus.Approved)
            .Select(e => e.Course)
            .ToListAsync();

        if (approvedCourses.Count == 0)
            return BadRequest("Onaylanmış kurs kaydınız bulunmuyor.");

        // tamamlama yüzdesi düşük olan önce
        var completedModulesByCourse = await db.ModuleCompletions
            .Where(mc => mc.UserId == userId)
            .GroupBy(mc => mc.Module.CourseId)
            .Select(g => new { CourseId = g.Key, CompletedCount = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.CompletedCount);

        var moduleCounts = await db.CourseModules
            .Where(m => approvedCourses.Select(c => c.Id).Contains(m.CourseId))
            .GroupBy(m => m.CourseId)
            .Select(g => new { CourseId = g.Key, Total = g.Count() })
            .ToDictionaryAsync(x => x.CourseId, x => x.Total);

        var orderedCourses = approvedCourses
            .OrderBy(c =>
            {
                int total = moduleCounts.GetValueOrDefault(c.Id, 1);
                int done = completedModulesByCourse.GetValueOrDefault(c.Id, 0);
                return (double)done / total;
            })
            .ThenBy(c => (int)c.Difficulty)
            .ToList();

        // önceki plan varsa temizle
        var existing = await db.StudyPlans
            .Include(sp => sp.Items)
            .FirstOrDefaultAsync(sp => sp.UserId == userId);

        if (existing is not null)
        {
            db.StudyPlanItems.RemoveRange(existing.Items);
            db.StudyPlans.Remove(existing);
        }

        var plan = new StudyPlan { UserId = userId };
        db.StudyPlans.Add(plan);
        await db.SaveChangesAsync();

        // kursa haftada bir gün ve önerilen saate göre ata
        var items = orderedCourses.Select((course, index) => new StudyPlanItem
        {
            StudyPlanId = plan.Id,
            CourseId = course.Id,
            ScheduledDay = Days[index % Days.Length],
            RecommendedHours = Math.Max(1, course.EctsCredit / 2)
        }).ToList();

        db.StudyPlanItems.AddRange(items);
        await db.SaveChangesAsync();

        // tamamlama oranı düşük olan kurslar
        var recommended = orderedCourses
            .Where(c =>
            {
                int total = moduleCounts.GetValueOrDefault(c.Id, 1);
                int done = completedModulesByCourse.GetValueOrDefault(c.Id, 0);
                return total == 0 || (double)done / total < 0.5;
            })
            .Select(c => c.Title);

        studyPlanService.NotifyGenerated(userName, recommended);

        var dto = new StudyPlanDto(
            plan.Id,
            plan.GeneratedAt,
            items.Select(i => new StudyPlanItemDto(
                i.CourseId,
                orderedCourses.First(c => c.Id == i.CourseId).Title,
                orderedCourses.First(c => c.Id == i.CourseId).Category,
                i.ScheduledDay,
                i.RecommendedHours
            ))
        );

        return Ok(dto);
    }

    [HttpGet]
    public async Task<IActionResult> GetPlan()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var plan = await db.StudyPlans
            .Include(sp => sp.Items)
                .ThenInclude(i => i.Course)
            .FirstOrDefaultAsync(sp => sp.UserId == userId);

        if (plan is null)
            return NotFound("Henüz bir çalışma planınız yok. /generate ile oluşturun.");

        var dto = new StudyPlanDto(
            plan.Id,
            plan.GeneratedAt,
            plan.Items.Select(i => new StudyPlanItemDto(
                i.CourseId,
                i.Course.Title,
                i.Course.Category,
                i.ScheduledDay,
                i.RecommendedHours
            ))
        );

        return Ok(dto);
    }

    [HttpDelete]
    public async Task<IActionResult> DeletePlan()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var plan = await db.StudyPlans
            .Include(sp => sp.Items)
            .FirstOrDefaultAsync(sp => sp.UserId == userId);

        if (plan is null)
            return NotFound("Silinecek plan bulunamadı.");

        db.StudyPlanItems.RemoveRange(plan.Items);
        db.StudyPlans.Remove(plan);
        await db.SaveChangesAsync();

        return Ok(new { message = "Çalışma planı silindi." });
    }
}
