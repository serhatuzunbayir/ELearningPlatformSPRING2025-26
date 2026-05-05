using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using learning_platform.Models;
using learning_platform.Services;

namespace learning_platform.Controllers;

// Admin paneli sadece Admin rolündeki kullanıcılara açılır.
// Tüm aksiyonlar API'deki [Authorize(Roles="Admin")] uçlarına proxy görevi görür.
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApiService _apiService;

    public AdminController(ApiService apiService)
    {
        _apiService = apiService;
    }

    // Admin ana sayfası: özet kartlarda gösterilecek toplam kurs, bekleyen talep
    // ve toplam kayıt sayılarını LINQ ile hesaplar.
    public async Task<IActionResult> Index()
    {
        var vm = new AdminDashboardViewModel();

        try
        {
            var courses = await _apiService.GetAsync<List<CourseListDto>>("/api/courses")
                          ?? new List<CourseListDto>();
            var pending = await _apiService.GetAsync<List<EnrollmentDto>>("/api/enrollments/pending")
                          ?? new List<EnrollmentDto>();

            vm.TotalCourses = courses.Count;
            vm.PendingEnrollments = pending.Count;
            vm.TotalApprovedEnrollments = courses.Sum(c => c.EnrollmentCount);
            vm.RecentPending = pending.Take(5).ToList();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load admin dashboard: " + ex.Message;
        }

        return View(vm);
    }

    // Kurs listesini API'den çeker; tüm CRUD aksiyonları bu sayfa üstünden yönetilir.
    public async Task<IActionResult> Courses()
    {
        try
        {
            var courses = await _apiService.GetAsync<List<CourseListDto>>("/api/courses");
            return View(courses ?? new List<CourseListDto>());
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load courses: " + ex.Message;
            return View(new List<CourseListDto>());
        }
    }

    [HttpGet]
    public IActionResult CreateCourse()
    {
        return View(new CreateCourseDto(string.Empty, string.Empty, string.Empty, DifficultyLevel.Beginner, 0));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CreateCourseDto model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var success = await _apiService.PostAsync("/api/courses", model);
            if (success)
            {
                TempData["SuccessMessage"] = $"Course '{model.Title}' created successfully.";
                return RedirectToAction(nameof(Courses));
            }

            ModelState.AddModelError(string.Empty, "Could not create course.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        try
        {
            var course = await _apiService.GetAsync<CourseDetailDto>($"/api/courses/{id}");
            if (course == null) return NotFound();

            var dto = new UpdateCourseDto(
                course.Title,
                course.Description,
                course.Category,
                course.Difficulty,
                course.EctsCredit
            );

            ViewBag.CourseId = id;
            return View(dto);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load course: " + ex.Message;
            return RedirectToAction(nameof(Courses));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int id, UpdateCourseDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CourseId = id;
            return View(model);
        }

        try
        {
            await _apiService.PutAsync($"/api/courses/{id}", model);
            TempData["SuccessMessage"] = $"Course '{model.Title}' updated successfully.";
            return RedirectToAction(nameof(Courses));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
            ViewBag.CourseId = id;
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        try
        {
            await _apiService.DeleteAsync($"/api/courses/{id}");
            TempData["SuccessMessage"] = "Course deleted.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not delete course: " + ex.Message;
        }

        return RedirectToAction(nameof(Courses));
    }

    public async Task<IActionResult> Enrollments()
    {
        try
        {
            var pending = await _apiService.GetAsync<List<EnrollmentDto>>("/api/enrollments/pending");
            return View(pending ?? new List<EnrollmentDto>());
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not load pending enrollments: " + ex.Message;
            return View(new List<EnrollmentDto>());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveEnrollment(int id)
    {
        try
        {
            await _apiService.PutAsync<object>($"/api/enrollments/{id}/approve");
            TempData["SuccessMessage"] = "Enrollment approved.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not approve enrollment: " + ex.Message;
        }

        return RedirectToAction(nameof(Enrollments));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectEnrollment(int id)
    {
        try
        {
            await _apiService.PutAsync<object>($"/api/enrollments/{id}/reject");
            TempData["SuccessMessage"] = "Enrollment rejected.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = "Could not reject enrollment: " + ex.Message;
        }

        return RedirectToAction(nameof(Enrollments));
    }
}

// Admin Index sayfasına özet veriyi taşıyan basit view model.
public class AdminDashboardViewModel
{
    public int TotalCourses { get; set; }
    public int PendingEnrollments { get; set; }
    public int TotalApprovedEnrollments { get; set; }
    public List<EnrollmentDto> RecentPending { get; set; } = new();
}
