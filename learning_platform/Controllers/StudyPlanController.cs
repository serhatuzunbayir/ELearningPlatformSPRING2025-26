using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using learning_platform.Models;
using learning_platform.Services;

namespace learning_platform.Controllers;

[Authorize]
public class StudyPlanController : Controller
{
    private readonly ApiService _apiService;

    public StudyPlanController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            if (User.IsInRole("Student"))
            {
                ViewBag.Recommendations = await _apiService.GetAsync<List<RecommendedCourseDto>>("/api/recommendations")
                    ?? new List<RecommendedCourseDto>();
            }

            var plan = await _apiService.GetAsync<StudyPlanDto>("/api/studyplan");
            return View(plan);
        }
        catch (Exception ex)
        {
            if (ex.Message.Contains("404"))
            {
                // No plan exists yet
                return View(null);
            }
            TempData["ErrorMessage"] = "Could not load study plan: " + ex.Message;
            return View(null);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Generate()
    {
        try
        {
            var plan = await _apiService.PostAsync<object, StudyPlanDto>("/api/studyplan/generate", new { });
            if (plan != null)
            {
                TempData["SuccessMessage"] = "Study plan generated successfully based on your approved enrollments!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to generate study plan. You need approved enrollments first.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not generate study plan: " + ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
