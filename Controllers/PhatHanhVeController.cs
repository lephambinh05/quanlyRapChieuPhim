using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System;

namespace CinemaManagement.Controllers
{
    public class PhatHanhVeController : Controller
    {
        private readonly CinemaDbContext _context;

        public PhatHanhVeController(CinemaDbContext context)
        {
            _context = context;
        }

        private bool IsManagerOrStaff()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            return vaiTro == "Quản lý" || vaiTro == "Nhân viên";
        }

        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, string? maPhim, string? maPhong)
        {
            if (!IsManagerOrStaff())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Mặc định lọc theo ngày hiện tại nếu không có bộ lọc nào
            if (!tuNgay.HasValue && !denNgay.HasValue && string.IsNullOrEmpty(maPhim) && string.IsNullOrEmpty(maPhong))
            {
                tuNgay = DateTime.Today;
                denNgay = DateTime.Today;
            }

            var lichChieusQuery = _context.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)
                .Include(l => l.NhanVien)
                .AsQueryable();

            // Lọc theo ngày
            if (tuNgay.HasValue)
            {
                lichChieusQuery = lichChieusQuery.Where(l => l.ThoiGianBatDau.Date >= tuNgay.Value.Date);
            }

            if (denNgay.HasValue)
            {
                lichChieusQuery = lichChieusQuery.Where(l => l.ThoiGianBatDau.Date <= denNgay.Value.Date);
            }

            // Lọc theo phim
            if (!string.IsNullOrEmpty(maPhim))
            {
                lichChieusQuery = lichChieusQuery.Where(l => l.MaPhim == maPhim);
            }

            // Lọc theo phòng
            if (!string.IsNullOrEmpty(maPhong))
            {
                lichChieusQuery = lichChieusQuery.Where(l => l.MaPhong == maPhong);
            }

            var lichChieus = await lichChieusQuery
                .OrderBy(l => l.ThoiGianBatDau)
                .ToListAsync();

            // Chuẩn bị dữ liệu cho dropdown
            var danhSachPhim = await _context.Phims
                .Select(p => new SelectListItem { Value = p.MaPhim, Text = p.TenPhim })
                .ToListAsync();

            var danhSachPhong = await _context.PhongChieus
                .Select(p => new SelectListItem { Value = p.MaPhong, Text = p.TenPhong })
                .ToListAsync();

            var viewModel = new PhatHanhVeIndexViewModel
            {
                LichChieus = lichChieus,
                DanhSachPhim = danhSachPhim,
                DanhSachPhong = danhSachPhong,
                TuNgay = tuNgay?.ToString("yyyy-MM-dd"),
                DenNgay = denNgay?.ToString("yyyy-MM-dd"),
                MaPhimSelected = maPhim,
                MaPhongSelected = maPhong
            };

            return View(viewModel);
        }

        public async Task<IActionResult> DanhSachVe()
        {
            if (!IsManagerOrStaff())
            {
                return RedirectToAction("Login", "Auth");
            }

            var ves = await _context.Ves
                .Include(v => v.Phim)
                .Include(v => v.PhongChieu)
                .Include(v => v.GheNgoi)
                .Include(v => v.LichChieu)
                .OrderByDescending(v => v.HanSuDung ?? DateTime.MinValue)
                .ToListAsync();

            return View(ves);
        }

        public async Task<IActionResult> ChiTietVe(string id)
        {
            if (!IsManagerOrStaff())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var ve = await _context.Ves
                .Include(v => v.Phim)
                .Include(v => v.PhongChieu)
                .Include(v => v.GheNgoi)
                .Include(v => v.LichChieu)
                .FirstOrDefaultAsync(v => v.MaVe == id);

            if (ve == null)
            {
                return NotFound();
            }

            return View(ve);
        }

        public async Task<IActionResult> PhatHanhHangLoat(string maLichChieu)
        {
            if (!IsManagerOrStaff())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(maLichChieu))
            {
                return NotFound();
            }

            var lichChieu = await _context.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)
                .ThenInclude(p => p.GheNgois)
                .FirstOrDefaultAsync(l => l.MaLichChieu == maLichChieu);

            if (lichChieu == null)
            {
                return NotFound();
            }

            // Lấy danh sách ghế đã có vé
            var gheCoVe = await _context.Ves
                .Where(v => v.MaLichChieu == maLichChieu)
                .Select(v => v.MaGhe)
                .ToListAsync();

            var model = new PhatHanhHangLoatViewModel
            {
                LichChieu = lichChieu,
                DanhSachGhe = lichChieu.PhongChieu.GheNgois.ToList(),
                GheCoVe = gheCoVe
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> PhatHanhHangLoat(PhatHanhHangLoatViewModel model)
        {
            if (!IsManagerOrStaff())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (model.GheChon == null || !model.GheChon.Any())
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một ghế để phát hành vé.");
                return RedirectToAction("PhatHanhHangLoat", new { maLichChieu = model.MaLichChieu });
            }

            var lichChieu = await _context.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)
                .FirstOrDefaultAsync(l => l.MaLichChieu == model.MaLichChieu);

            if (lichChieu == null)
            {
                return NotFound();
            }

            // Sử dụng transaction để đảm bảo tính nhất quán
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var danhSachVeMoi = new List<Ve>();
                
                // Lấy counter hiện tại cho ngày hôm nay - sử dụng MAX để tránh race condition
                var today = DateTime.Now.ToString("yyMMdd");
                var maxCounter = await _context.Ves
                    .Where(v => v.MaVe.StartsWith($"V{today}"))
                    .Select(v => v.MaVe)
                    .ToListAsync();
                
                // Tìm số counter lớn nhất hiện tại
                var currentMaxCounter = 0;
                foreach (var ticket in maxCounter)
                {
                    if (ticket.Length == 10 && int.TryParse(ticket.Substring(7, 3), out int ticketCounter))
                    {
                        currentMaxCounter = Math.Max(currentMaxCounter, ticketCounter);
                    }
                }
                
                var counter = currentMaxCounter + 1;

                foreach (var maGhe in model.GheChon)
                {
                    // Kiểm tra xem ghế đã có vé chưa
                    var veExist = await _context.Ves
                        .AnyAsync(v => v.MaLichChieu == model.MaLichChieu && v.MaGhe == maGhe);

                    if (!veExist)
                    {
                        var ghe = await _context.GheNgois.FirstOrDefaultAsync(g => g.MaGhe == maGhe);
                        if (ghe != null)
                        {
                            // Tạo mã vé với timestamp để tránh trùng lặp
                            var timestamp = DateTime.Now.ToString("HHmmss");
                            var maVe = $"V{today}{timestamp}{counter:D2}";
                            counter++;

                            var ve = new Ve
                            {
                                MaVe = maVe,
                                TrangThai = "Chưa đặt",
                                SoGhe = ghe.SoGhe,
                                TenPhim = lichChieu.Phim.TenPhim,
                                HanSuDung = lichChieu.ThoiGianBatDau.AddHours(2),
                                Gia = lichChieu.Gia + ghe.GiaGhe,
                                TenPhong = lichChieu.PhongChieu.TenPhong,
                                MaGhe = maGhe,
                                MaLichChieu = model.MaLichChieu,
                                MaPhim = lichChieu.MaPhim,
                                MaPhong = lichChieu.MaPhong
                            };

                            danhSachVeMoi.Add(ve);
                        }
                    }
                }

                if (danhSachVeMoi.Any())
                {
                    _context.Ves.AddRange(danhSachVeMoi);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    TempData["Success"] = $"Đã phát hành thành công {danhSachVeMoi.Count} vé.";
                }
                else
                {
                    await transaction.RollbackAsync();
                    TempData["Error"] = "Không có vé nào được phát hành. Tất cả ghế đã có vé hoặc không hợp lệ.";
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Lỗi khi phát hành vé: {ex.Message}";
            }

            return RedirectToAction("DanhSachVe");
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatTrangThai(string maVe, string trangThai)
        {
            if (!IsManagerOrStaff())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var ve = await _context.Ves.FirstOrDefaultAsync(v => v.MaVe == maVe);
            if (ve == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vé" });
            }

            ve.TrangThai = trangThai;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
        }

        [HttpPost]
        public async Task<IActionResult> XoaVe(string maVe)
        {
            if (!IsManagerOrStaff())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var ve = await _context.Ves.FirstOrDefaultAsync(v => v.MaVe == maVe);
            if (ve == null)
            {
                return Json(new { success = false, message = "Không tìm thấy vé" });
            }

            // Kiểm tra xem vé đã được bán chưa
            var daBan = await _context.CTHDs.AnyAsync(c => c.MaVe == maVe);
            if (daBan)
            {
                return Json(new { success = false, message = "Không thể xóa vé đã được bán" });
            }

            _context.Ves.Remove(ve);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Xóa vé thành công" });
        }

        public async Task<IActionResult> SoDoGhe(string maLichChieu)
        {
            if (!IsManagerOrStaff())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(maLichChieu))
            {
                return NotFound();
            }

            var lichChieu = await _context.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)
                .ThenInclude(p => p.GheNgois)
                .FirstOrDefaultAsync(l => l.MaLichChieu == maLichChieu);

            if (lichChieu == null)
            {
                return NotFound();
            }

            // Lấy danh sách ghế đã có vé
            var gheCoVe = await _context.Ves
                .Where(v => v.MaLichChieu == maLichChieu)
                .Select(v => v.MaGhe)
                .ToListAsync();

            var model = new SoDoGheViewModel
            {
                LichChieu = lichChieu,
                DanhSachGhe = lichChieu.PhongChieu.GheNgois.ToList(),
                GheCoVe = gheCoVe
            };

            return View(model);
        }

        public async Task<IActionResult> ThongKe()
        {
            if (!IsManagerOrStaff())
            {
                return RedirectToAction("Login", "Auth");
            }

            var thongKe = new ThongKeVeViewModel
            {
                TongSoVe = await _context.Ves.CountAsync(),
                VeConHan = await _context.Ves.CountAsync(v => v.TrangThai == "Còn hạn"),
                VeHetHan = await _context.Ves.CountAsync(v => v.TrangThai == "Hết hạn"),
                VeDaBan = await _context.Ves.CountAsync(v => v.TrangThai == "Đã sử dụng"),
                TongDoanhThu = await _context.Ves.SumAsync(v => v.Gia),
                ThongKeTheoPhim = await _context.Ves
                    .Include(v => v.Phim)
                    .GroupBy(v => v.Phim.TenPhim)
                    .Select(g => new ThongKePhimViewModel
                    {
                        TenPhim = g.Key,
                        SoVe = g.Count(),
                        DoanhThu = g.Sum(v => v.Gia)
                    })
                    .OrderByDescending(t => t.SoVe)
                    .ToListAsync()
            };

            return View(thongKe);
        }
    }
}
