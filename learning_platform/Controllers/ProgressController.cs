using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using learning_platform.Models;
using learning_platform.Services;

namespace learning_platform.Controllers;

[Authorize]
public class ProgressController : Controller
{
    private readonly ApiService _apiService;

    public ProgressController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Course(int id)
    {
        try
        {
            var courseProgress = await _apiService.GetAsync<CourseProgressDto>($"/api/progress/course/{id}");
            if (courseProgress == null) return NotFound();
            
            return View(courseProgress);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load course progress. You might not be enrolled yet. " + ex.Message;
            return RedirectToAction("Index", "Dashboard");
        }
    }

    [HttpPost]
    public async Task<IActionResult> CompleteModule(int moduleId, int courseId)
    {
        try
        {
            var success = await _apiService.PostAsync("/api/progress/complete", new CompleteModuleDto(moduleId));
            if (success)
            {
                TempData["SuccessMessage"] = "Module marked as completed!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to complete module.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error completing module: " + ex.Message;
        }

        return RedirectToAction(nameof(Course), new { id = courseId });
    }
}
