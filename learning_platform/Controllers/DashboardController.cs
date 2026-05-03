using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using learning_platform.Models;
using learning_platform.Services;

namespace learning_platform.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApiService _apiService;

    public DashboardController(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var summary = await _apiService.GetAsync<ProgressSummaryDto>("/api/progress/summary");
            return View(summary);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load dashboard data: " + ex.Message;
            return View(null);
        }
    }
}
