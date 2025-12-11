using Microsoft.AspNetCore.Mvc;
using MinecraftBackend.Models;
using System.Diagnostics;

namespace MinecraftBackend.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            // Mặc định chuyển hướng người dùng vào trang Admin Dashboard
            return RedirectToAction("Dashboard", "Admin");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        // Action xử lý lỗi hệ thống
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}