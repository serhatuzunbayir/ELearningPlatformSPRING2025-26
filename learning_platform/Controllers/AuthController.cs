using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using learning_platform.Models;
using learning_platform.Services;

namespace learning_platform.Controllers;

public class AuthController : Controller
{
    private readonly ApiService _apiService;

    public AuthController(ApiService apiService)
    {
        _apiService = apiService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity is { IsAuthenticated: true })
            return RedirectToAction(User.IsInRole("Admin") ? "Index" : "Index",
                                    User.IsInRole("Admin") ? "Admin" : "Dashboard");
            
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var result = await _apiService.PostAsync<LoginDto, LoginStepResponseDto>("/api/auth/login", model);

            if (result is null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            if (result.RequiresTwoFactor)
            {
                // Keep email between requests.
                TempData["TwoFactorEmail"] = result.Email;
                if (!string.IsNullOrEmpty(result.DevVerificationCode))
                    TempData["DevVerificationCode"] = result.DevVerificationCode;
                return RedirectToAction(nameof(VerifyTwoFactor));
            }

            if (!string.IsNullOrEmpty(result.Token))
                return await SignInAndRedirect(result.Token, result.Name!, result.Email!, result.Role!);

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity is { IsAuthenticated: true })
            return RedirectToAction(User.IsInRole("Admin") ? "Index" : "Index",
                                    User.IsInRole("Admin") ? "Admin" : "Dashboard");
            
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var success = await _apiService.PostAsync("/api/auth/register", model);
            
            if (success)
            {
                TempData["SuccessMessage"] = "Registration successful! Please login.";
                return RedirectToAction("Login");
            }
            
            ModelState.AddModelError(string.Empty, "Registration failed. Email might already exist.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult VerifyTwoFactor()
    {
        ViewBag.Email = TempData["TwoFactorEmail"] as string ?? "";
        ViewBag.DevCode = TempData["DevVerificationCode"] as string;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyTwoFactor(VerifyTwoFactorDto model)
    {
        try
        {
            var result = await _apiService.PostAsync<VerifyTwoFactorDto, AuthResponseDto>("/api/auth/verify-2fa", model);
            if (result is not null && !string.IsNullOrEmpty(result.Token))
                return await SignInAndRedirect(result.Token, result.Name, result.Email, result.Role);

            ModelState.AddModelError(string.Empty, "Invalid verification code.");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Error: " + ex.Message);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth");
    }

    private async Task<IActionResult> SignInAndRedirect(string token, string name, string email, string role)
    {
        // Save token in auth claims.
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new("access_token", token)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

        return role == "Admin"
            ? RedirectToAction("Index", "Admin")
            : RedirectToAction("Index", "Dashboard");
    }
}
