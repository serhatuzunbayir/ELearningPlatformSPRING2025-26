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
            return RedirectToAction("Index", "Dashboard");
            
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid) return View(model);

        try
        {
            var result = await _apiService.PostAsync<LoginDto, AuthResponseDto>("/api/auth/login", model);

            if (result != null && !string.IsNullOrEmpty(result.Token))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, result.Name),
                    new Claim(ClaimTypes.Email, result.Email),
                    new Claim(ClaimTypes.Role, result.Role),
                    new Claim("access_token", result.Token)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme, 
                    new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Dashboard");
            }

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
            return RedirectToAction("Index", "Dashboard");
            
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

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Auth");
    }
}
