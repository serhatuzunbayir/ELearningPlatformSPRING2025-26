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
[Authorize]
public class EnrollmentsController(AppDbContext db, EnrollmentService enrollmentService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> RequestEnrollment(EnrollmentRequestDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var course = await db.Courses.FindAsync(dto.CourseId);
        if (course is null)
            return NotFound("Kurs bulunamadı.");

        var alreadyExists = await db.Enrollments
            .AnyAsync(e => e.UserId == userId && e.CourseId == dto.CourseId);

        if (alreadyExists)
            return BadRequest("Bu kursa zaten kayıt talebinde bulundunuz.");

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = dto.CourseId,
            Status = EnrollmentStatus.Pending
        };

        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        return Ok(new { message = "Kayıt talebiniz alındı, admin onayı bekleniyor." });
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPending()
    {
        var enrollments = await db.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .Where(e => e.Status == EnrollmentStatus.Pending)
            .OrderBy(e => e.RequestedAt)
            .Select(e => new EnrollmentDto(
                e.Id,
                e.User.Name,
                e.User.Email,
                e.Course.Title,
                e.Status,
                e.RequestedAt,
                e.ApprovedAt
            ))
            .ToListAsync();

        return Ok(enrollments);
    }

    [HttpPut("{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Approve(int id)
    {
        var enrollment = await db.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment is null)
            return NotFound("Kayıt talebi bulunamadı.");

        if (enrollment.Status != EnrollmentStatus.Pending)
            return BadRequest("Bu talep zaten işleme alınmış.");

        enrollment.Status = EnrollmentStatus.Approved;
        enrollment.ApprovedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        enrollmentService.NotifyApproved(enrollment.User.Name, enrollment.Course.Title);

        return Ok(new { message = "Kayıt onaylandı." });
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Reject(int id)
    {
        var enrollment = await db.Enrollments
            .Include(e => e.User)
            .Include(e => e.Course)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (enrollment is null)
            return NotFound("Kayıt talebi bulunamadı.");

        if (enrollment.Status != EnrollmentStatus.Pending)
            return BadRequest("Bu talep zaten işleme alınmış.");

        enrollment.Status = EnrollmentStatus.Rejected;
        await db.SaveChangesAsync();

        enrollmentService.NotifyRejected(enrollment.User.Name, enrollment.Course.Title);

        return Ok(new { message = "Kayıt reddedildi." });
    }

    [HttpGet("my")]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var enrollments = await db.Enrollments
            .Include(e => e.Course)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.RequestedAt)
            .Select(e => new EnrollmentDto(
                e.Id,
                e.User.Name,
                e.User.Email,
                e.Course.Title,
                e.Status,
                e.RequestedAt,
                e.ApprovedAt
            ))
            .ToListAsync();

        return Ok(enrollments);
    }
}
