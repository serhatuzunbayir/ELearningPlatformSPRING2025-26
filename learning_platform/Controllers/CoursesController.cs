using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using learning_platform.Models;
using learning_platform.Services;

namespace learning_platform.Controllers;

[Authorize]
public class CoursesController : Controller
{
    private readonly ApiService _apiService;

    public CoursesController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index(string? category, string? difficulty)
    {
        try
        {
            var url = "/api/courses";
            var queryParams = new List<string>();
            
            if (!string.IsNullOrEmpty(category))
                queryParams.Add($"category={Uri.EscapeDataString(category)}");
            
            if (!string.IsNullOrEmpty(difficulty))
                queryParams.Add($"difficulty={Uri.EscapeDataString(difficulty)}");

            if (queryParams.Any())
                url += "?" + string.Join("&", queryParams);

            var courses = await _apiService.GetAsync<List<CourseListDto>>(url);
            
            // Pass parameters back to view to keep selected values
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedDifficulty = difficulty;
            
            return View(courses ?? new List<CourseListDto>());
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load courses: " + ex.Message;
            return View(new List<CourseListDto>());
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var course = await _apiService.GetAsync<CourseDetailDto>($"/api/courses/{id}");
            if (course == null) return NotFound();
            return View(course);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load course details: " + ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    public async Task<IActionResult> Enroll(int courseId)
    {
        try
        {
            var success = await _apiService.PostAsync("/api/enrollments", new EnrollmentRequestDto(courseId));
            if (success)
            {
                TempData["SuccessMessage"] = "Enrollment request sent! Please wait for admin approval.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not send enrollment request. You might already be enrolled or pending.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Error: " + ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = courseId });
    }
}
