using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System.Security.Claims;

namespace CinemaManagement.Controllers
{
    public class DanhGiaController : Controller
    {
        private readonly CinemaDbContext _context;

        public DanhGiaController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: DanhGia/Create/{maPhim}
        public async Task<IActionResult> Create(string maPhim)
        {
            Console.WriteLine($"[DANHGIA_CREATE] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Bắt đầu Create với maPhim: {maPhim}");
            
            if (string.IsNullOrEmpty(maPhim))
            {
                Console.WriteLine($"[DANHGIA_CREATE] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] maPhim rỗng, return NotFound");
                return NotFound();
            }

            var phim = await _context.Phims
                .FirstOrDefaultAsync(p => p.MaPhim == maPhim);

            if (phim == null)
            {
                return NotFound();
            }

            // Lấy thông tin khách hàng từ session
            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            if (string.IsNullOrEmpty(maKhachHang))
            {
                return RedirectToAction("Login", "Auth");
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.MaKhachHang == maKhachHang);

            if (khachHang == null)
            {
                return NotFound();
            }

            // Kiểm tra xem khách hàng đã đánh giá phim này chưa
            var existingDanhGia = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.MaKhachHang == maKhachHang && d.MaPhim == maPhim);

            if (existingDanhGia != null)
            {
                // Nếu đã đánh giá, chuyển đến trang chỉnh sửa
                return RedirectToAction("Edit", new { id = existingDanhGia.Id });
            }

            var viewModel = new DanhGiaViewModel
            {
                MaPhim = phim.MaPhim!,
                TenPhim = phim.TenPhim!,
                MaKhachHang = khachHang.MaKhachHang!,
                HoTenKhachHang = khachHang.HoTen!
            };

            return View(viewModel);
        }

        // POST: DanhGia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhGiaViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var danhGia = new DanhGia
                {
                    MaKhachHang = viewModel.MaKhachHang,
                    MaPhim = viewModel.MaPhim,
                    SoSao = viewModel.SoSao,
                    NoiDungBinhLuan = viewModel.NoiDungBinhLuan,
                    NgayDanhGia = DateTime.Now,
                    DaXemPhim = true // Giả sử khách hàng đã xem phim khi đánh giá
                };

                _context.Add(danhGia);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đánh giá của bạn đã được ghi nhận!";
                return RedirectToAction("Details", "KhachHang", new { maPhim = viewModel.MaPhim });
            }

            return View(viewModel);
        }

        // GET: DanhGia/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            // Log chi tiết để debug
            Console.WriteLine($"[DANHGIA_EDIT] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Bắt đầu Edit với id: {id}");
            
            var danhGia = await _context.DanhGias
                .Include(d => d.KhachHang)
                .Include(d => d.Phim)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (danhGia == null)
            {
                Console.WriteLine($"[DANHGIA_EDIT] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Không tìm thấy đánh giá với id: {id}");
                return NotFound();
            }

            Console.WriteLine($"[DANHGIA_EDIT] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Tìm thấy đánh giá: MaKhachHang={danhGia.MaKhachHang}, MaPhim={danhGia.MaPhim}");

            // Kiểm tra quyền chỉnh sửa
            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            Console.WriteLine($"[DANHGIA_EDIT] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MaKhachHang từ session: {maKhachHang}");
            
            if (string.IsNullOrEmpty(maKhachHang))
            {
                Console.WriteLine($"[DANHGIA_EDIT] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Không có MaKhachHang trong session, redirect to Login");
                return RedirectToAction("Login", "Auth");
            }
            
            if (danhGia.MaKhachHang != maKhachHang)
            {
                Console.WriteLine($"[DANHGIA_EDIT] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Không có quyền chỉnh sửa: danhGia.MaKhachHang={danhGia.MaKhachHang}, session.MaKhachHang={maKhachHang}");
                return Forbid();
            }

            var viewModel = new DanhGiaViewModel
            {
                MaPhim = danhGia.MaPhim,
                TenPhim = danhGia.Phim.TenPhim!,
                MaKhachHang = danhGia.MaKhachHang,
                HoTenKhachHang = danhGia.KhachHang.HoTen!,
                SoSao = danhGia.SoSao,
                NoiDungBinhLuan = danhGia.NoiDungBinhLuan
            };

            return View(viewModel);
        }

        // POST: DanhGia/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhGiaViewModel viewModel)
        {
            if (id != viewModel.MaPhim.GetHashCode()) // Sử dụng hash code làm ID tạm thời
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var danhGia = await _context.DanhGias
                    .FirstOrDefaultAsync(d => d.MaKhachHang == viewModel.MaKhachHang && d.MaPhim == viewModel.MaPhim);

                if (danhGia == null)
                {
                    return NotFound();
                }

                danhGia.SoSao = viewModel.SoSao;
                danhGia.NoiDungBinhLuan = viewModel.NoiDungBinhLuan;
                danhGia.NgayDanhGia = DateTime.Now;

                _context.Update(danhGia);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đánh giá của bạn đã được cập nhật!";
                return RedirectToAction("Details", "KhachHang", new { maPhim = viewModel.MaPhim });
            }

            return View(viewModel);
        }

        // GET: DanhGia/List/{maPhim}
        public async Task<IActionResult> List(string maPhim)
        {
            Console.WriteLine($"[DANHGIA_LIST] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Bắt đầu List với maPhim: {maPhim}");
            
            if (string.IsNullOrEmpty(maPhim))
            {
                Console.WriteLine($"[DANHGIA_LIST] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] maPhim rỗng, return NotFound");
                return NotFound();
            }

            var phim = await _context.Phims
                .Include(p => p.DanhGias)
                .ThenInclude(d => d.KhachHang)
                .FirstOrDefaultAsync(p => p.MaPhim == maPhim);

            if (phim == null)
            {
                return NotFound();
            }

            var viewModel = new DanhGiaListViewModel
            {
                MaPhim = phim.MaPhim!,
                TenPhim = phim.TenPhim!,
                DiemTrungBinh = phim.DiemTrungBinh,
                TongSoDanhGia = phim.TongSoDanhGia,
                DanhGias = phim.DanhGias
                    .OrderByDescending(d => d.NgayDanhGia)
                    .Select(d => new DanhGiaItemViewModel
                    {
                        Id = d.Id,
                        SoSao = d.SoSao,
                        NoiDungBinhLuan = d.NoiDungBinhLuan,
                        NgayDanhGia = d.NgayDanhGia,
                        HoTenKhachHang = d.KhachHang.HoTen!,
                        DaXemPhim = d.DaXemPhim
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // POST: DanhGia/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                return NotFound();
            }

            // Kiểm tra quyền xóa
            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            if (danhGia.MaKhachHang != maKhachHang)
            {
                return Forbid();
            }

            _context.DanhGias.Remove(danhGia);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đánh giá đã được xóa!";
            return RedirectToAction("Details", "KhachHang", new { maPhim = danhGia.MaPhim });
        }
    }
} 