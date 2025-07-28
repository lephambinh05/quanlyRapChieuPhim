using Microsoft.AspNetCore.Mvc;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CinemaManagement.Controllers
{
    public class QuanLyController : Controller
    {
        private readonly CinemaDbContext _context;

        public QuanLyController(CinemaDbContext context)
        {
            _context = context;
        }

        private bool IsManager()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            return vaiTro == "Quản lý";
        }

        private bool IsManagerOrStaff()
        {
            var vaiTro = HttpContext.Session.GetString("VaiTro");
            return vaiTro == "Quản lý" || vaiTro == "Nhân viên";
        }

        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, string? tenPhim)
        {
            if (!IsManagerOrStaff())
            {
                return RedirectToAction("Login", "Auth");
            }

            var today = DateTime.Today;
            var thisWeek = today.AddDays(-(int)today.DayOfWeek);
            var thisMonth = new DateTime(today.Year, today.Month, 1);

            // Khởi tạo query gốc
            var veQuery = _context.Ves
                .Include(v => v.LichChieu)
                    .ThenInclude(lc => lc.Phim)
                .AsQueryable();

            // Áp dụng bộ lọc nếu có
            if (tuNgay.HasValue)
                veQuery = veQuery.Where(v => v.HanSuDung.Date >= tuNgay.Value.Date);
            if (denNgay.HasValue)
                veQuery = veQuery.Where(v => v.HanSuDung.Date <= denNgay.Value.Date);
            if (!string.IsNullOrEmpty(tenPhim))
                veQuery = veQuery.Where(v => v.LichChieu.Phim.TenPhim.Contains(tenPhim));

            // Gán vào ViewModel
            var dashboard = new DashboardViewModel
            {
                TuNgay = tuNgay,
                DenNgay = denNgay,
                TenPhim = tenPhim,

                // Thống kê cơ bản
                TongSoVe = await _context.Ves.CountAsync(),
                VeHomNay = await _context.Ves.CountAsync(v => v.HanSuDung.Date == today),
                VeTuanNay = await _context.Ves.CountAsync(v => v.HanSuDung >= thisWeek),
                VeThangNay = await _context.Ves.CountAsync(v => v.HanSuDung >= thisMonth),

                // Thống kê vé đã bán trong hóa đơn
                TongSoVeDaBanTrongHoaDon = await _context.HoaDons.SumAsync(h => h.SoLuong),
                VeDaBanTrongHoaDonHomNay = await _context.HoaDons.Where(h => h.ThoiGianTao.Date == today).SumAsync(h => h.SoLuong),

                // Thống kê doanh thu
                DoanhThuHomNay = await _context.Ves.Where(v => v.HanSuDung.Date == today).SumAsync(v => v.Gia),
                DoanhThuTuanNay = await _context.Ves.Where(v => v.HanSuDung >= thisWeek).SumAsync(v => v.Gia),
                DoanhThuThangNay = await _context.Ves.Where(v => v.HanSuDung >= thisMonth).SumAsync(v => v.Gia),

                // Thống kê tổng tiền hóa đơn
                TongTienHoaDon = await _context.HoaDons.SumAsync(h => h.TongTien),
                TienHoaDonHomNay = await _context.HoaDons.Where(h => h.ThoiGianTao.Date == today).SumAsync(h => h.TongTien),

                // Lịch chiếu (không áp dụng bộ lọc)
                LichChieuHomNay = await _context.LichChieus.CountAsync(l => l.ThoiGianBatDau.Date == today),
                LichChieuTuanNay = await _context.LichChieus.CountAsync(l => l.ThoiGianBatDau >= thisWeek),

                // Phim, phòng, ghế (không lọc)
                TongSoPhim = await _context.Phims.CountAsync(),
                PhimDangChieu = await _context.LichChieus
                    .Where(l => l.ThoiGianBatDau >= DateTime.Now)
                    .Select(l => l.MaPhim)
                    .Distinct()
                    .CountAsync(),
                TongSoPhong = await _context.PhongChieus.CountAsync(),
                TongSoGhe = await _context.GheNgois.CountAsync(),


                // Top phim theo bộ lọc - từ hóa đơn đã bán
                TopPhimBanChay = await _context.CTHDs
                    .Include(c => c.Ve)
                        .ThenInclude(v => v.LichChieu)
                        .ThenInclude(l => l.Phim)
                    .Include(c => c.HoaDon)
                    .Where(c => !tuNgay.HasValue || c.HoaDon.ThoiGianTao.Date >= tuNgay.Value.Date)
                    .Where(c => !denNgay.HasValue || c.HoaDon.ThoiGianTao.Date <= denNgay.Value.Date)
                    .Where(c => string.IsNullOrEmpty(tenPhim) || c.Ve.TenPhim.Contains(tenPhim))
                    .GroupBy(c => new { c.Ve.MaPhim, c.Ve.TenPhim })
                    .Select(g => new TopPhimViewModel
                    {
                        MaPhim = g.Key.MaPhim,
                        TenPhim = g.Key.TenPhim,
                        SoVe = g.Count(),
                        DoanhThu = g.Sum(c => c.DonGia)
                    })
                    .OrderByDescending(t => t.SoVe)
                    .Take(5)
                    .ToListAsync(),

                // Lịch chiếu gần nhất
                LichChieuGanNhat = await _context.LichChieus
                    .Include(l => l.Phim)
                    .Include(l => l.PhongChieu)
                    .Where(l => l.ThoiGianBatDau >= DateTime.Now)
                    .OrderBy(l => l.ThoiGianBatDau)
                    .Take(5)
                    .ToListAsync(),

                // Dữ liệu biểu đồ (chưa áp dụng lọc vì phụ thuộc yêu cầu)
                DoanhThuTheoNgay = await GetDoanhThuHoaDonTheoNgay(7),
                DoanhThuTheoThang = await GetDoanhThuTheoThang(12)
            };

            return View(dashboard);
        }


        public async Task<IActionResult> ThongKeChiTiet(DateTime? tuNgay, DateTime? denNgay, string? tenPhim)
        {
            if (!IsManagerOrStaff())
                return RedirectToAction("Login", "Auth");

            // Query từ hóa đơn đã bán
            var cthdQuery = _context.CTHDs
                .Include(c => c.Ve)
                    .ThenInclude(v => v.Phim)
                .Include(c => c.Ve)
                    .ThenInclude(v => v.PhongChieu)
                .Include(c => c.HoaDon)
                .AsQueryable();

            if (tuNgay.HasValue)
                cthdQuery = cthdQuery.Where(c => c.HoaDon.ThoiGianTao.Date >= tuNgay.Value.Date);
            if (denNgay.HasValue)
                cthdQuery = cthdQuery.Where(c => c.HoaDon.ThoiGianTao.Date <= denNgay.Value.Date);
            if (!string.IsNullOrEmpty(tenPhim))
                cthdQuery = cthdQuery.Where(c => c.Ve.TenPhim.Contains(tenPhim));

            var thongKe = new ThongKeChiTietViewModel
            {
                TuNgay = tuNgay,
                DenNgay = denNgay,
                TenPhim = tenPhim,

                TongSoVe = await cthdQuery.CountAsync(),
                TongDoanhThu = await cthdQuery.SumAsync(c => c.DonGia),
                TongSoPhim = await _context.Phims.CountAsync(),
                TongSoLichChieu = await _context.LichChieus.CountAsync(),

                ThongKeTheoPhim = await cthdQuery
                    .GroupBy(c => new { c.Ve.MaPhim, c.Ve.TenPhim })
                    .Select(g => new ThongKePhimChiTietViewModel
                    {
                        MaPhim = g.Key.MaPhim,
                        TenPhim = g.Key.TenPhim,
                        SoVe = g.Count(),
                        DoanhThu = g.Sum(c => c.DonGia),
                        GiaTrungBinh = g.Average(c => c.DonGia)
                    })
                    .OrderByDescending(t => t.DoanhThu)
                    .ToListAsync(),

                ThongKeTheoPhong = await cthdQuery
                    .GroupBy(c => new { c.Ve.MaPhong, c.Ve.TenPhong })
                    .Select(g => new ThongKePhongViewModel
                    {
                        MaPhong = g.Key.MaPhong,
                        TenPhong = g.Key.TenPhong,
                        SoVe = g.Count(),
                        DoanhThu = g.Sum(c => c.DonGia),
                        TiLeLapDay = 0 // Có thể tính sau
                    })
                    .OrderByDescending(t => t.DoanhThu)
                    .ToListAsync(),

                DoanhThuTheoNgay = await GetDoanhThuHoaDonTheoNgay(30),
                DoanhThuTheoThang = await GetDoanhThuTheoThang(12)
            };

            return View(thongKe);
        }


        [HttpGet]
        public async Task<IActionResult> GetDoanhThuData(string type = "day", int days = 7)
        {
            if (!IsManagerOrStaff())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                if (type == "day")
                {
                    var data = await GetDoanhThuTheoNgay(days);
                    return Json(new { success = true, data = data });
                }
                else if (type == "month")
                {
                    var data = await GetDoanhThuTheoThang(days);
                    return Json(new { success = true, data = data });
                }
                else
                {
                    return Json(new { success = false, message = "Loại dữ liệu không hợp lệ" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<List<DoanhThuTheoNgayViewModel>> GetDoanhThuTheoNgay(int days)
        {
            var result = new List<DoanhThuTheoNgayViewModel>();
            var startDate = DateTime.Today.AddDays(-days + 1);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                var doanhThu = await _context.Ves
                    .Where(v => v.HanSuDung.Date == date)
                    .SumAsync(v => v.Gia);

                var soVe = await _context.Ves
                    .CountAsync(v => v.HanSuDung.Date == date);

                result.Add(new DoanhThuTheoNgayViewModel
                {
                    Ngay = date,
                    DoanhThu = doanhThu,
                    SoVe = soVe
                });
            }

            return result;
        }

        private async Task<List<DoanhThuTheoNgayViewModel>> GetDoanhThuHoaDonTheoNgay(int days)
        {
            var result = new List<DoanhThuTheoNgayViewModel>();
            var startDate = DateTime.Today.AddDays(-days + 1);

            for (int i = 0; i < days; i++)
            {
                var date = startDate.AddDays(i);
                
                // Lấy doanh thu từ vé đã bán trong hóa đơn
                var doanhThu = await _context.CTHDs
                    .Join(_context.HoaDons, ct => ct.MaHoaDon, hd => hd.MaHoaDon, (ct, hd) => new { ct, hd })
                    .Where(x => x.hd.ThoiGianTao.Date == date)
                    .Join(_context.Ves, x => x.ct.MaVe, v => v.MaVe, (x, v) => v.Gia)
                    .SumAsync();

                // Đếm số vé đã bán
                var soVe = await _context.CTHDs
                    .Join(_context.HoaDons, ct => ct.MaHoaDon, hd => hd.MaHoaDon, (ct, hd) => new { ct, hd })
                    .Where(x => x.hd.ThoiGianTao.Date == date)
                    .CountAsync();

                result.Add(new DoanhThuTheoNgayViewModel
                {
                    Ngay = date,
                    DoanhThu = doanhThu,
                    SoVe = soVe
                });
            }

            return result;
        }

        private async Task<List<DoanhThuTheoThangViewModel>> GetDoanhThuTheoThang(int months)
        {
            var result = new List<DoanhThuTheoThangViewModel>();
            var startDate = DateTime.Today.AddMonths(-months + 1);
            startDate = new DateTime(startDate.Year, startDate.Month, 1);

            for (int i = 0; i < months; i++)
            {
                var date = startDate.AddMonths(i);
                var nextMonth = date.AddMonths(1);

                var doanhThu = await _context.Ves
                    .Where(v => v.HanSuDung >= date && v.HanSuDung < nextMonth)
                    .SumAsync(v => v.Gia);

                var soVe = await _context.Ves
                    .CountAsync(v => v.HanSuDung >= date && v.HanSuDung < nextMonth);

                result.Add(new DoanhThuTheoThangViewModel
                {
                    Thang = date.Month,
                    Nam = date.Year,
                    DoanhThu = doanhThu,
                    SoVe = soVe
                });
            }

            return result;
        }

        public async Task<IActionResult> QuanLyPhim()
        {
            if (!IsManager())
            {
                return RedirectToAction("Login", "Auth");
            }

            var phims = await _context.Phims.ToListAsync();
            return View(phims);
        }

        public async Task<IActionResult> QuanLyLichChieu()
        {
            if (!IsManager())
            {
                return RedirectToAction("Login", "Auth");
            }

            var lichChieus = await _context.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)
                .Include(l => l.NhanVien)
                .OrderBy(l => l.ThoiGianBatDau)
                .ToListAsync();

            return View(lichChieus);
        }

        public async Task<IActionResult> QuanLyNhanVien()
        {
            if (!IsManager())
            {
                return RedirectToAction("Login", "Auth");
            }

            var nhanViens = await _context.NhanViens
                .OrderBy(n => n.TenNhanVien)
                .ToListAsync();

            return View(nhanViens);
        }

        public async Task<IActionResult> BaoCao()
        {
            if (!IsManager())
            {
                return RedirectToAction("Login", "Auth");
            }

            var baoCao = new BaoCaoViewModel
            {
                TongDoanhThu = await _context.CTHDs
                    .Join(_context.HoaDons, ct => ct.MaHoaDon, hd => hd.MaHoaDon, (ct, hd) => new { ct, hd })
                    .Join(_context.Ves, x => x.ct.MaVe, v => v.MaVe, (x, v) => v.Gia)
                    .SumAsync(),
                TongSoVe = await _context.CTHDs.CountAsync(),
                TongSoPhim = await _context.Phims.CountAsync(),
                TongSoLichChieu = await _context.LichChieus.CountAsync(),
                DoanhThuTheoPhim = await _context.CTHDs
                    .Join(_context.HoaDons, ct => ct.MaHoaDon, hd => hd.MaHoaDon, (ct, hd) => new { ct, hd })
                    .Join(_context.Ves, x => x.ct.MaVe, v => v.MaVe, (x, v) => v)
                    .GroupBy(v => v.TenPhim)
                    .Select(g => new { TenPhim = g.Key, DoanhThu = g.Sum(v => v.Gia) })
                    .OrderByDescending(x => x.DoanhThu)
                    .Take(10)
                    .ToDictionaryAsync(x => x.TenPhim, x => x.DoanhThu)
            };

            return View(baoCao);
        }

        [HttpPost]
        public async Task<IActionResult> ThemPhim(string tenPhim, string theLoai, int thoiLuong, string doTuoiPhanAnh, string moTa, string viTriFilePhim)
        {
            if (!IsManager())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Tạo mã phim mới
                var lastPhim = await _context.Phims.OrderByDescending(p => p.MaPhim).FirstOrDefaultAsync();
                var newMaPhim = "P001";
                if (lastPhim != null)
                {
                    var lastNumber = int.Parse(lastPhim.MaPhim.Substring(1));
                    newMaPhim = $"P{(lastNumber + 1):D3}";
                }

                var maNhanVien = HttpContext.Session.GetString("maNhanVien");
                if (string.IsNullOrEmpty(maNhanVien))
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin nhân viên" });
                }

                var phim = new Phim
                {
                    MaPhim = newMaPhim,
                    TenPhim = tenPhim,
                    TheLoai = theLoai,
                    ThoiLuong = thoiLuong,
                    DoTuoiPhanAnh = doTuoiPhanAnh,
                    MoTa = moTa,
                    ViTriFilePhim = viTriFilePhim,
                    MaNhanVien = maNhanVien
                };

                _context.Phims.Add(phim);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm phim thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi thêm phim: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> XoaPhim(string maPhim)
        {
            if (!IsManager())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                var phim = await _context.Phims.FirstOrDefaultAsync(p => p.MaPhim == maPhim);
                if (phim == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy phim" });
                }

                // Kiểm tra xem phim có được sử dụng trong lịch chiếu không
                var coLichChieu = await _context.LichChieus.AnyAsync(l => l.MaPhim == maPhim);
                if (coLichChieu)
                {
                    return Json(new { success = false, message = "Không thể xóa phim đang có lịch chiếu" });
                }

                _context.Phims.Remove(phim);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa phim thành công" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa phim: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ChiTietPhim(string maPhim)
        {
            if (!IsManagerOrStaff())
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var phim = await _context.Phims
                .Include(p => p.NhanVien)
                .FirstOrDefaultAsync(p => p.MaPhim == maPhim);

            if (phim == null)
            {
                return Json(new { success = false, message = "Không tìm thấy phim" });
            }

            var lichChieuCount = await _context.LichChieus.CountAsync(l => l.MaPhim == maPhim);
            var veCount = await _context.Ves.CountAsync(v => v.MaPhim == maPhim);

            return Json(new
            {
                success = true,
                phim = new
                {
                    MaPhim = phim.MaPhim,
                    TenPhim = phim.TenPhim,
                    TheLoai = phim.TheLoai,
                    ThoiLuong = phim.ThoiLuong,
                    DoTuoiPhanAnh = phim.DoTuoiPhanAnh,
                    MoTa = phim.MoTa,
                    ViTriFilePhim = phim.ViTriFilePhim,
                    NhanVien = phim.NhanVien.TenNhanVien,
                    SoLichChieu = lichChieuCount,
                    SoVeBanRa = veCount
                }
            });
        }
    }
}

