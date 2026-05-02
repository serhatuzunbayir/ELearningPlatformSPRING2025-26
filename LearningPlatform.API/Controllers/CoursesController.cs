using LearningPlatform.API.Data;
using LearningPlatform.API.DTOs;
using LearningPlatform.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? category,
        [FromQuery] DifficultyLevel? difficulty)
    {
        var query = db.Courses
            .Include(c => c.Modules)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(c => c.Category.ToLower() == category.ToLower());

        if (difficulty.HasValue)
            query = query.Where(c => c.Difficulty == difficulty.Value);

        var courses = await query
            .OrderBy(c => c.Title)
            .Select(c => new CourseListDto(
                c.Id,
                c.Title,
                c.Category,
                c.Difficulty,
                c.EctsCredit,
                c.Modules.Count,
                c.Enrollments.Count(e => e.Status == EnrollmentStatus.Approved)
            ))
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var course = await db.Courses
            .Include(c => c.Modules.OrderBy(m => m.Order))
            .FirstOrDefaultAsync(c => c.Id == id);

        if (course is null)
            return NotFound("Kurs bulunamadı.");

        var dto = new CourseDetailDto(
            course.Id,
            course.Title,
            course.Description,
            course.Category,
            course.Difficulty,
            course.EctsCredit,
            course.Modules.Select(m => new CourseModuleDto(m.Id, m.Title, m.Order)).ToList()
        );

        return Ok(dto);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CreateCourseDto dto)
    {
        var course = new Course
        {
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            Difficulty = dto.Difficulty,
            EctsCredit = dto.EctsCredit
        };

        db.Courses.Add(course);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = course.Id }, new { course.Id, course.Title });
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, UpdateCourseDto dto)
    {
        var course = await db.Courses.FindAsync(id);

        if (course is null)
            return NotFound("Kurs bulunamadı.");

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.Category = dto.Category;
        course.Difficulty = dto.Difficulty;
        course.EctsCredit = dto.EctsCredit;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var course = await db.Courses.FindAsync(id);

        if (course is null)
            return NotFound("Kurs bulunamadı.");

        db.Courses.Remove(course);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
