using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System.Text.Json;

namespace CinemaManagement.Controllers
{
    public class BanVeController : Controller
    {
        private readonly CinemaDbContext _context;

        public BanVeController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: BanVe - Trang chọn phim và lịch chiếu
        public async Task<IActionResult> Index()
        {
            // Kiểm tra đăng nhập
            var maNhanVien = HttpContext.Session.GetString("maNhanVien");
            if (string.IsNullOrEmpty(maNhanVien))
            {
                return RedirectToAction("Login", "Auth");
            }

            var viewModel = new BanVeViewModel
            {
                DanhSachPhim = await _context.Phims.ToListAsync(),
                DanhSachLichChieu = await _context.LichChieus
                    .Include(l => l.Phim)
                    .Include(l => l.PhongChieu)
                    .Where(l => l.ThoiGianBatDau > DateTime.Now)
                    .OrderBy(l => l.ThoiGianBatDau)
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: BanVe/ChonGhe/{maLichChieu}
        public async Task<IActionResult> ChonGhe(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var lichChieu = await _context.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)
                .FirstOrDefaultAsync(l => l.MaLichChieu == id);

            if (lichChieu == null)
            {
                return RedirectToAction("Index");
            }

            var danhSachGhe = await _context.GheNgois
                .Where(g => g.MaPhong == lichChieu.MaPhong)
                .OrderBy(g => g.SoGhe)
                .ToListAsync();

            var danhSachVeDaBan = await _context.Ves
                .Where(v => v.MaLichChieu == id && v.TrangThai == "Đã bán")
                .ToListAsync();

            var danhSachVeDaPhatHanh = await _context.Ves
                .Where(v => v.MaLichChieu == id && (v.TrangThai == "Chưa đặt" || v.TrangThai == "Còn hạn"))
                .ToListAsync();

            var viewModel = new ChonGheViewModel
            {
                LichChieu = lichChieu,
                DanhSachGhe = danhSachGhe,
                DanhSachVeDaBan = danhSachVeDaBan,
                DanhSachVeDaPhatHanh = danhSachVeDaPhatHanh
            };

            return View(viewModel);
        }

        // POST: BanVe/ThanhToan
        [HttpPost]
        public async Task<IActionResult> ThanhToan([FromBody] ThanhToanRequest request)
        {
            if (request.GheDuocChon == null || request.GheDuocChon.Count == 0)
            {
                return Json(new { success = false, message = "Vui lòng chọn ghế" });
            }

            var lichChieu = await _context.LichChieus
                .Include(l => l.Phim)
                .Include(l => l.PhongChieu)
                .FirstOrDefaultAsync(l => l.MaLichChieu == request.MaLichChieu);

            if (lichChieu == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lịch chiếu" });
            }

            var danhSachGheDuocChon = await _context.GheNgois
                .Where(g => request.GheDuocChon.Contains(g.MaGhe))
                .ToListAsync();

            // Kiểm tra ghế đã được bán chưa
            var ghesDaBan = await _context.Ves
                .Where(v => v.MaLichChieu == request.MaLichChieu && 
                           request.GheDuocChon.Contains(v.MaGhe) && 
                           v.TrangThai == "Đã bán")
                .Select(v => v.MaGhe)
                .ToListAsync();

            if (ghesDaBan.Any())
            {
                return Json(new { success = false, message = "Một số ghế đã được bán" });
            }

            var tongTien = danhSachGheDuocChon.Sum(g => g.GiaGhe);

            // Lưu thông tin vào session
            HttpContext.Session.SetString("ThanhToan_MaLichChieu", request.MaLichChieu);
            HttpContext.Session.SetString("ThanhToan_GheDuocChon", JsonSerializer.Serialize(request.GheDuocChon));
            HttpContext.Session.SetString("ThanhToan_TongTien", tongTien.ToString());

            return Json(new { success = true, redirectUrl = Url.Action("ThanhToan") });
        }

        // GET: BanVe/ThanhToan
        public async Task<IActionResult> ThanhToan()
        {
            try
            {
                var maLichChieu = HttpContext.Session.GetString("ThanhToan_MaLichChieu");
                var gheDuocChonJson = HttpContext.Session.GetString("ThanhToan_GheDuocChon");
                var tongTienStr = HttpContext.Session.GetString("ThanhToan_TongTien");

                Console.WriteLine($"ThanhToan GET - Session data: MaLichChieu={maLichChieu}, GheDuocChon={gheDuocChonJson}, TongTien={tongTienStr}");

                if (string.IsNullOrEmpty(maLichChieu) || string.IsNullOrEmpty(gheDuocChonJson))
                {
                    Console.WriteLine("Missing session data, redirecting to Index");
                    return RedirectToAction("Index");
                }

                var gheDuocChon = JsonSerializer.Deserialize<List<string>>(gheDuocChonJson) ?? new List<string>();
                var tongTien = decimal.Parse(tongTienStr ?? "0");

                Console.WriteLine($"Deserializing data - GheDuocChon count: {gheDuocChon.Count}, TongTien: {tongTien}");

                // Test database connection
                var canConnect = await _context.Database.CanConnectAsync();
                Console.WriteLine($"Database connection status: {canConnect}");

                if (!canConnect)
                {
                    Console.WriteLine("Cannot connect to database");
                    ViewBag.Error = "Không thể kết nối với cơ sở dữ liệu";
                    return View("Error");
                }

                var lichChieu = await _context.LichChieus
                    .Include(l => l.Phim)
                    .Include(l => l.PhongChieu)
                    .FirstOrDefaultAsync(l => l.MaLichChieu == maLichChieu);

                Console.WriteLine($"LichChieu found: {lichChieu != null}");
                if (lichChieu != null)
                {
                    Console.WriteLine($"Movie: {lichChieu.Phim?.TenPhim}, Room: {lichChieu.PhongChieu?.TenPhong}");
                }

                var danhSachGheDuocChon = await _context.GheNgois
                    .Where(g => gheDuocChon.Contains(g.MaGhe))
                    .ToListAsync();

                Console.WriteLine($"Seats found: {danhSachGheDuocChon.Count}");

                var danhSachVoucherKhaDung = await _context.Vouchers
                    .Where(v => v.ThoiGianBatDau <= DateTime.Now && v.ThoiGianKetThuc >= DateTime.Now)
                    .ToListAsync();

                Console.WriteLine($"Available vouchers: {danhSachVoucherKhaDung.Count}");

                var viewModel = new ThanhToanViewModel
                {
                    LichChieu = lichChieu!,
                    DanhSachGheDuocChon = danhSachGheDuocChon,
                    TongTien = tongTien,
                    DanhSachVoucherKhaDung = danhSachVoucherKhaDung,
                    ThanhTien = tongTien
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ThanhToan GET: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                ViewBag.Error = $"Lỗi hệ thống: {ex.Message}";
                return View("Error");
            }
        }

        // POST: BanVe/XacNhanThanhToan
        [HttpPost]
        public async Task<IActionResult> XacNhanThanhToan([FromBody] XacNhanThanhToanRequest request)
        {
            try
            {
                var maNhanVien = HttpContext.Session.GetString("maNhanVien");
                if (string.IsNullOrEmpty(maNhanVien))
                {
                    return Json(new { success = false, message = "Phiên đăng nhập hết hạn" });
                }

                var maLichChieu = HttpContext.Session.GetString("ThanhToan_MaLichChieu");
                var gheDuocChonJson = HttpContext.Session.GetString("ThanhToan_GheDuocChon");

                if (string.IsNullOrEmpty(maLichChieu) || string.IsNullOrEmpty(gheDuocChonJson))
                {
                    return Json(new { success = false, message = "Thông tin thanh toán không hợp lệ" });
                }

                var gheDuocChon = JsonSerializer.Deserialize<List<string>>(gheDuocChonJson) ?? new List<string>();

                var lichChieu = await _context.LichChieus
                    .Include(l => l.Phim)
                    .Include(l => l.PhongChieu)
                    .FirstOrDefaultAsync(l => l.MaLichChieu == maLichChieu);

                if (lichChieu == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lịch chiếu" });
                }

                var danhSachGheDuocChon = await _context.GheNgois
                    .Where(g => gheDuocChon.Contains(g.MaGhe))
                    .ToListAsync();

                // Kiểm tra ghế đã được bán chưa
                var ghesDaBan = await _context.Ves
                    .Where(v => v.MaLichChieu == maLichChieu && 
                               gheDuocChon.Contains(v.MaGhe) && 
                               v.TrangThai == "Đã bán")
                    .Select(v => v.MaGhe)
                    .ToListAsync();

                if (ghesDaBan.Any())
                {
                    return Json(new { success = false, message = "Một số ghế đã được bán. Vui lòng chọn lại." });
                }

                // Tạo mã hóa đơn mới
                var lastHoaDon = await _context.HoaDons
                    .OrderByDescending(h => h.MaHoaDon)
                    .FirstOrDefaultAsync();

                var soThuTu = 1;
                if (lastHoaDon != null)
                {
                    var soHienTai = int.Parse(lastHoaDon.MaHoaDon.Substring(2));
                    soThuTu = soHienTai + 1;
                }

                var maHoaDon = $"HD{soThuTu:D3}";

                // Tính toán giá
                var tongTien = danhSachGheDuocChon.Sum(g => g.GiaGhe);
                var tienGiamGia = 0m;
                var thanhTien = tongTien;

                // Áp dụng voucher nếu có
                Voucher? voucherSuDung = null;
                if (!string.IsNullOrEmpty(request.VoucherDuocChon))
                {
                    voucherSuDung = await _context.Vouchers.FindAsync(request.VoucherDuocChon);
                    if (voucherSuDung != null && 
                        voucherSuDung.ThoiGianBatDau <= DateTime.Now && 
                        voucherSuDung.ThoiGianKetThuc >= DateTime.Now)
                    {
                        tienGiamGia = tongTien * voucherSuDung.PhanTramGiam / 100;
                        thanhTien = tongTien - tienGiamGia;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Voucher không hợp lệ hoặc đã hết hạn" });
                    }
                }

                // Tìm khách hàng
                KhachHang? khachHang = null;
                if (!string.IsNullOrEmpty(request.maKhachHang))
                {
                    khachHang = await _context.KhachHangs.FindAsync(request.maKhachHang);
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                
                try
                {
                    // Tạo hóa đơn
                    var hoaDon = new HoaDon
                    {
                        MaHoaDon = maHoaDon,
                        TongTien = thanhTien,
                        ThoiGianTao = DateTime.Now,
                        SoLuong = danhSachGheDuocChon.Count,
                        maKhachHang = khachHang?.maKhachHang ?? "GUEST", // GUEST cho khách lẻ
                        maNhanVien = maNhanVien
                    };

                    _context.HoaDons.Add(hoaDon);
                    await _context.SaveChangesAsync();

                    // Tạo vé và chi tiết hóa đơn
                    var danhSachVe = new List<Ve>();
                    var danhSachCTHD = new List<CTHD>();

                    // Lấy số thứ tự lớn nhất của mã vé hiện có
                    var lastVe = await _context.Ves.OrderByDescending(v => v.MaVe).FirstOrDefaultAsync();
                    var soThuTuVe = 1;
                    if (lastVe != null)
                    {
                        var soHienTaiVe = int.Parse(lastVe.MaVe.Substring(2));
                        soThuTuVe = soHienTaiVe + 1;
                    }

                    // Lấy số thứ tự lớn nhất của mã CTHD hiện có
                    var lastCTHD = await _context.CTHDs.OrderByDescending(c => c.MaCTHD).FirstOrDefaultAsync();
                    var soThuTuCTHD = 1;
                    if (lastCTHD != null)
                    {
                        var soHienTaiCTHD = int.Parse(lastCTHD.MaCTHD.Substring(2));
                        soThuTuCTHD = soHienTaiCTHD + 1;
                    }

                    for (int i = 0; i < danhSachGheDuocChon.Count; i++)
                    {
                        var ghe = danhSachGheDuocChon[i];

                        var maVe = $"VE{soThuTuVe:D3}";
                        soThuTuVe++;

                        var ve = new Ve
                        {
                            MaVe = maVe,
                            TrangThai = "Chưa bán",
                            SoGhe = ghe.SoGhe,
                            TenPhim = lichChieu!.Phim.TenPhim,
                            HanSuDung = lichChieu.ThoiGianBatDau,
                            Gia = ghe.GiaGhe,
                            TenPhong = lichChieu.PhongChieu.TenPhong,
                            MaGhe = ghe.MaGhe,
                            MaLichChieu = lichChieu.MaLichChieu,
                            MaPhim = lichChieu.MaPhim,
                            MaPhong = lichChieu.MaPhong
                        };

                        _context.Ves.Add(ve);
                        danhSachVe.Add(ve);

                        var maCTHD = $"CT{soThuTuCTHD:D3}";
                        soThuTuCTHD++;

                        var cthd = new CTHD
                        {
                            MaCTHD = maCTHD,
                            DonGia = ghe.GiaGhe,
                            MaVe = maVe,
                            MaHoaDon = maHoaDon
                        };

                        _context.CTHDs.Add(cthd);
                        danhSachCTHD.Add(cthd);
                    }

                    // Lưu voucher nếu có
                    if (voucherSuDung != null)
                    {
                        var hdVoucher = new HDVoucher
                        {
                            MaHoaDon = maHoaDon,
                            MaGiamGia = voucherSuDung.MaGiamGia,
                            SoLuongVoucher = 1,
                            TongTien = thanhTien
                        };

                        _context.HDVouchers.Add(hdVoucher);
                    }

                    // Cập nhật điểm tích lũy cho khách hàng
                    if (khachHang != null)
                    {
                        var diemTichLuyMoi = (int)(thanhTien / 10000); // 1 điểm = 10,000đ
                        khachHang.DiemTichLuy += diemTichLuyMoi;
                        _context.KhachHangs.Update(khachHang);
                        
                        Console.WriteLine($"Cập nhật điểm tích lũy cho khách hàng {khachHang.maKhachHang}: +{diemTichLuyMoi} điểm");
                    }
                    else
                    {
                        Console.WriteLine("Khách lẻ - không tích điểm");
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Xóa session
                    HttpContext.Session.Remove("ThanhToan_MaLichChieu");
                    HttpContext.Session.Remove("ThanhToan_GheDuocChon");
                    HttpContext.Session.Remove("ThanhToan_TongTien");

                    return Json(new { 
                        success = true, 
                        message = "Thanh toán thành công", 
                        redirectUrl = Url.Action("HoaDon", new { id = maHoaDon }) 
                    });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Lỗi khi lưu dữ liệu: " + ex.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // GET: BanVe/HoaDon/{id}
        public async Task<IActionResult> HoaDon(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToAction("Index");
            }

            var hoaDon = await _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            if (hoaDon == null)
            {
                return NotFound();
            }

            var chiTietHoaDon = await _context.CTHDs
                .Include(c => c.Ve)
                .ThenInclude(v => v.GheNgoi)
                .Where(c => c.MaHoaDon == id)
                .ToListAsync();

            var hdVoucher = await _context.HDVouchers
                .Include(h => h.Voucher)
                .FirstOrDefaultAsync(h => h.MaHoaDon == id);

            var viewModel = new HoaDonViewModel
            {
                HoaDon = hoaDon,
                ChiTietHoaDon = chiTietHoaDon,
                KhachHang = hoaDon.KhachHang,
                NhanVien = hoaDon.NhanVien,
                VoucherSuDung = hdVoucher?.Voucher,
                TienGiamGia = hdVoucher?.TongTien ?? 0
            };

            return View(viewModel);
        }

        // API: Lấy lịch chiếu theo phim
        [HttpGet]
        public async Task<IActionResult> GetLichChieuByPhim(string maPhim, string? tuNgay = null, string? denNgay = null)
        {
            try
            {
                var query = _context.LichChieus
                    .Include(l => l.PhongChieu)
                    .Where(l => l.MaPhim == maPhim && l.ThoiGianBatDau > DateTime.Now);

                // Áp dụng bộ lọc thời gian nếu có
                if (!string.IsNullOrEmpty(tuNgay) && DateTime.TryParse(tuNgay, out var fromDate))
                {
                    query = query.Where(l => l.ThoiGianBatDau.Date >= fromDate.Date);
                }

                if (!string.IsNullOrEmpty(denNgay) && DateTime.TryParse(denNgay, out var toDate))
                {
                    query = query.Where(l => l.ThoiGianBatDau.Date <= toDate.Date);
                }

                var lichChieus = await query
                    .OrderBy(l => l.ThoiGianBatDau)
                    .Select(l => new
                    {
                        maLichChieu = l.MaLichChieu,
                        thoiGianBatDau = l.ThoiGianBatDau.ToString("dd/MM/yyyy HH:mm"),
                        thoiGianKetThuc = l.ThoiGianKetThuc.ToString("dd/MM/yyyy HH:mm"),
                        tenPhong = l.PhongChieu.TenPhong,
                        loaiPhong = l.PhongChieu.LoaiPhong,
                        gia = l.Gia
                    })
                    .ToListAsync();

                return Json(lichChieus);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLichChieuByPhim: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }

        // API: Tìm khách hàng
        [HttpGet]
        public async Task<IActionResult> TimKhachHang(string? sdt, string? maKH)
        {
            try
            {
                Console.WriteLine($"TimKhachHang called with SDT: {sdt}, MaKH: {maKH}");
                
                KhachHang? khachHang = null;

                if (!string.IsNullOrEmpty(maKH))
                {
                    khachHang = await _context.KhachHangs.FindAsync(maKH);
                    Console.WriteLine($"Search by MaKH: {maKH} - Found: {khachHang != null}");
                }
                else if (!string.IsNullOrEmpty(sdt))
                {
                    khachHang = await _context.KhachHangs.FirstOrDefaultAsync(k => k.SDT == sdt);
                    Console.WriteLine($"Search by SDT: {sdt} - Found: {khachHang != null}");
                }

                if (khachHang != null)
                {
                    Console.WriteLine($"Customer found: {khachHang.HoTen} - {khachHang.SDT}");
                    return Json(new
                    {
                        success = true,
                        khachHang = new
                        {
                            maKhachHang = khachHang.maKhachHang,
                            hoTen = khachHang.HoTen,
                            sdt = khachHang.SDT,
                            diemTichLuy = khachHang.DiemTichLuy
                        }
                    });
                }

                Console.WriteLine("Customer not found");
                return Json(new { success = false, message = "Không tìm thấy khách hàng" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TimKhachHang: {ex.Message}");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // API: Tính tiền giảm giá
        [HttpPost]
        public async Task<IActionResult> TinhTienGiamGia([FromBody] TinhGiamGiaRequest request)
        {
            try
            {
                Console.WriteLine($"TinhTienGiamGia called with MaVoucher: {request.MaVoucher}, TongTien: {request.TongTien}");
                
                if (string.IsNullOrEmpty(request.MaVoucher))
                {
                    Console.WriteLine("No voucher provided, returning original amount");
                    return Json(new { success = true, tienGiamGia = 0, thanhTien = request.TongTien });
                }

                var voucher = await _context.Vouchers.FindAsync(request.MaVoucher);
                Console.WriteLine($"Voucher found: {voucher != null}");
                
                if (voucher != null)
                {
                    Console.WriteLine($"Voucher details: {voucher.TenGiamGia}, Start: {voucher.ThoiGianBatDau}, End: {voucher.ThoiGianKetThuc}, Discount: {voucher.PhanTramGiam}%");
                    Console.WriteLine($"Current time: {DateTime.Now}");
                }
                
                if (voucher == null || voucher.ThoiGianBatDau > DateTime.Now || voucher.ThoiGianKetThuc < DateTime.Now)
                {
                    Console.WriteLine("Voucher is invalid or expired");
                    return Json(new { success = false, message = "Voucher không hợp lệ hoặc đã hết hạn" });
                }

                var tienGiamGia = request.TongTien * voucher.PhanTramGiam / 100;
                var thanhTien = request.TongTien - tienGiamGia;

                Console.WriteLine($"Discount calculated: {tienGiamGia}, Final amount: {thanhTien}");

                return Json(new
                {
                    success = true,
                    tienGiamGia = tienGiamGia,
                    thanhTien = thanhTien,
                    phanTramGiam = voucher.PhanTramGiam
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TinhTienGiamGia: {ex.Message}");
                return Json(new { success = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        // Test database connection
        [HttpGet]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                Console.WriteLine($"Database connection test: {canConnect}");
                
                if (!canConnect)
                {
                    return Json(new { success = false, message = "Cannot connect to database" });
                }

                // Test basic queries
                var customerCount = await _context.KhachHangs.CountAsync();
                var voucherCount = await _context.Vouchers.CountAsync();
                var seatCount = await _context.GheNgois.CountAsync();
                
                Console.WriteLine($"Database counts - Customers: {customerCount}, Vouchers: {voucherCount}, Seats: {seatCount}");

                return Json(new { 
                    success = true, 
                    message = "Database connected successfully",
                    data = new {
                        customers = customerCount,
                        vouchers = voucherCount,
                        seats = seatCount
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database test error: {ex.Message}");
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Test page for debugging
        [HttpGet]
        public IActionResult Test()
        {
            return View();
        }
    }

    // DTOs for API requests
    public class ThanhToanRequest
    {
        public string MaLichChieu { get; set; } = string.Empty;
        public List<string> GheDuocChon { get; set; } = new List<string>();
    }

    public class XacNhanThanhToanRequest
    {
        public string? maKhachHang { get; set; }
        public string? VoucherDuocChon { get; set; }
        public decimal TongTien { get; set; }
        public decimal TienGiamGia { get; set; }
        public decimal ThanhTien { get; set; }
    }

    public class TinhGiamGiaRequest
    {
        public string MaVoucher { get; set; } = string.Empty;
        public decimal TongTien { get; set; }
    }
}

