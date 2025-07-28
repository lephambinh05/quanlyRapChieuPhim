using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CinemaManagement.Models;

namespace CinemaManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        // Kiểm tra đăng nhập
        var role = HttpContext.Session.GetString("Role");
        var maNhanVien = HttpContext.Session.GetString("MaNhanVien");
        var maKhachHang = HttpContext.Session.GetString("MaKhachHang");

        if (string.IsNullOrEmpty(role))
        {
            return RedirectToAction("Login", "Auth");
        }

        // Chuyển hướng theo role
        if (role == "Khách hàng" && !string.IsNullOrEmpty(maKhachHang))
        {
            return RedirectToAction("Index", "KhachHang");
        }
        else if (role == "Nhân viên" && !string.IsNullOrEmpty(maNhanVien))
        {
            return RedirectToAction("Index", "BanVe");
        }
        else if (role == "Quản lý" && !string.IsNullOrEmpty(maNhanVien))
        {
            return RedirectToAction("Index", "QuanLy");
        }
        else
        {
            return RedirectToAction("Login", "Auth");
        }
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
