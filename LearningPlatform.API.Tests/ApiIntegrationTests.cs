using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LearningPlatform.API.DTOs;
using LearningPlatform.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LearningPlatform.API.Tests;

public class ApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;
    // JSON field names can differ by casing.
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public ApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        SeedDatabase();
    }

    private void SeedDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<LearningPlatform.API.Data.AppDbContext>();
        // Create/update test DB schema.
        db.Database.Migrate();

        EnsureUser(db, "admin@test.com", "Test Admin", "Admin1234!", UserRole.Admin, twoFactorEnabled: false);
        EnsureUser(db, "student@test.com", "Test Student", "Student1234!", UserRole.Student, preferredCategory: "Programming");

        if (!db.Courses.Any(c => c.Title == "Test Course"))
        {
            var course = new Course
            {
                Title = "Test Course",
                Description = "Desc",
                Category = "Programming",
                Difficulty = DifficultyLevel.Beginner,
                EctsCredit = 3
            };
            db.Courses.Add(course);
            db.SaveChanges();
            // Add one module for progress tests.
            db.CourseModules.Add(new CourseModule { CourseId = course.Id, Title = "M1", Content = "C", Order = 1 });
            db.SaveChanges();
        }
    }

    private static void EnsureUser(
        LearningPlatform.API.Data.AppDbContext db,
        string email,
        string name,
        string password,
        UserRole role,
        bool twoFactorEnabled = false,
        string? preferredCategory = null)
    {
        // Skip if user already exists.
        if (db.Users.Any(u => u.Email == email))
            return;

        db.Users.Add(new User
        {
            Name = name,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = role,
            TwoFactorEnabled = twoFactorEnabled,
            PreferredCategory = preferredCategory
        });
        db.SaveChanges();
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        // These tests use non-2FA login.
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto(email, password));
        response.EnsureSuccessStatusCode();
        var step = await response.Content.ReadFromJsonAsync<LoginStepResponseDto>(JsonOptions);
        Assert.NotNull(step);
        Assert.False(step!.RequiresTwoFactor);
        Assert.NotNull(step.Token);
        return step.Token!;
    }

    [Fact]
    public async Task Register_WithNewEmail_ReturnsSuccess()
    {
        var email = $"user{Guid.NewGuid():N}@test.com";
        var response = await _client.PostAsJsonAsync("/api/auth/register",
            new RegisterDto("New User", email, "Password1!", "Programming"));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginDto("student@test.com", "wrong"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetCourses_ReturnsOkWithoutAuth()
    {
        var response = await _client.GetAsync("/api/courses");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateCourse_AsAdmin_ReturnsCreated()
    {
        var token = await LoginAsync("admin@test.com", "Admin1234!");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/courses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new CreateCourseDto(
            "New Course", "D", "Programming", DifficultyLevel.Beginner, 4));
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateCourse_AsStudent_ReturnsForbidden()
    {
        var token = await LoginAsync("student@test.com", "Student1234!");
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/courses");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new CreateCourseDto(
            "X", "D", "Programming", DifficultyLevel.Beginner, 1));
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreateModule_AsAdmin_ReturnsCreated()
    {
        var token = await LoginAsync("admin@test.com", "Admin1234!");
        var courseId = await GetFirstCourseIdAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/courses/{courseId}/modules");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new CreateModuleDto("Module B", "Content", 2));
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task EnrollmentRequest_AsStudent_ReturnsOk()
    {
        var token = await LoginAsync("student@test.com", "Student1234!");
        var courseId = await GetFirstCourseIdAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/enrollments");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        request.Content = JsonContent.Create(new EnrollmentRequestDto(courseId));
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminAnalytics_AsAdmin_ReturnsOk()
    {
        var token = await LoginAsync("admin@test.com", "Admin1234!");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/analytics");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminAnalytics_AsStudent_ReturnsForbidden()
    {
        var token = await LoginAsync("student@test.com", "Student1234!");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin/analytics");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Recommendations_AsStudent_ReturnsOk()
    {
        var token = await LoginAsync("student@test.com", "Student1234!");
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/recommendations");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task VerifyTwoFactor_WithInvalidCode_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/verify-2fa",
            new VerifyTwoFactorDto("student@test.com", "000000"));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<int> GetFirstCourseIdAsync()
    {
        // Use seeded course id.
        var courses = await _client.GetFromJsonAsync<List<CourseListDto>>("/api/courses", JsonOptions);
        Assert.NotNull(courses);
        Assert.NotEmpty(courses!);
        return courses![0].Id;
    }
}
