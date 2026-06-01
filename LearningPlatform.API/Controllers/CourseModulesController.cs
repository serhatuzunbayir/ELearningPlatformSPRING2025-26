using LearningPlatform.API.Data;
using LearningPlatform.API.DTOs;
using LearningPlatform.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LearningPlatform.API.Controllers;

[ApiController]
[Route("api/courses/{courseId:int}/modules")]
[Authorize(Roles = "Admin")]
public class CourseModulesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(int courseId)
    {
        if (!await db.Courses.AnyAsync(c => c.Id == courseId))
            return NotFound("Kurs bulunamadı.");

        var modules = await db.CourseModules
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.Order)
            .Select(m => new CourseModuleDetailDto(m.Id, m.Title, m.Content, m.Order))
            .ToListAsync();

        return Ok(modules);
    }

    [HttpPost]
    public async Task<IActionResult> Create(int courseId, CreateModuleDto dto)
    {
        var course = await db.Courses.FindAsync(courseId);
        if (course is null)
            return NotFound("Kurs bulunamadı.");

        var module = new CourseModule
        {
            CourseId = courseId,
            Title = dto.Title,
            Content = dto.Content,
            Order = dto.Order
        };

        db.CourseModules.Add(module);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAll), new { courseId },
            new CourseModuleDetailDto(module.Id, module.Title, module.Content, module.Order));
    }

    [HttpPut("{moduleId:int}")]
    public async Task<IActionResult> Update(int courseId, int moduleId, UpdateModuleDto dto)
    {
        var module = await db.CourseModules
            .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId);

        if (module is null)
            return NotFound("Modül bulunamadı.");

        module.Title = dto.Title;
        module.Content = dto.Content;
        module.Order = dto.Order;
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{moduleId:int}")]
    public async Task<IActionResult> Delete(int courseId, int moduleId)
    {
        var module = await db.CourseModules
            .FirstOrDefaultAsync(m => m.Id == moduleId && m.CourseId == courseId);

        if (module is null)
            return NotFound("Modül bulunamadı.");

        db.CourseModules.Remove(module);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
