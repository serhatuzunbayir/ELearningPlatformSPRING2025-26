using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using learning_platform.Models;

namespace learning_platform.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        if (User.Identity is { IsAuthenticated: true })
            return RedirectToAction("Index", "Dashboard");
            
        return RedirectToAction("Login", "Auth");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
