using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using CinemaManagement.Extensions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.IO;

namespace CinemaManagement.Controllers
{
    public class KhachHangController : Controller
    {
        private static readonly object _logLock = new object();
        private readonly CinemaDbContext _context;

        public KhachHangController(CinemaDbContext context)
        {
            _context = context;
        }

        // Middleware kiểm tra đăng nhập
        private bool IsCustomerLoggedIn()
        {
            var role = HttpContext.Session.GetString("Role");
            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            return role == "Khách hàng" && !string.IsNullOrEmpty(maKhachHang);
        }

        // Trang chủ khách hàng - Hiển thị danh sách phim
        public async Task<IActionResult> Index(string? theLoai, string? searchTerm)
        {
            WriteErrorLog($"[ACTION] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [Index] Email: {HttpContext.Session.GetString("Email")}, theLoai: {theLoai}, searchTerm: {searchTerm}, SessionKeys: {string.Join(",", HttpContext.Session.Keys)}");
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var phims = _context.Phims.AsQueryable();

            // Lọc theo thể loại
            if (!string.IsNullOrEmpty(theLoai))
            {
                phims = phims.Where(p => p.TheLoai.Contains(theLoai));
            }

            // Tìm kiếm theo tên phim
            if (!string.IsNullOrEmpty(searchTerm))
            {
                phims = phims.Where(p => p.TenPhim.Contains(searchTerm));
            }

            var phimList = await phims.ToListAsync();

            // Lấy danh sách thể loại để hiển thị filter
            ViewBag.TheLoais = await _context.Phims
                .Select(p => p.TheLoai)
                .Distinct()
                .ToListAsync();

            // Lấy lịch chiếu trong 7 ngày tới
            var lichChieuSapToi = await _context.LichChieus
                .Include(lc => lc.Phim)
                .Include(lc => lc.PhongChieu)
                .Where(lc => lc.ThoiGianBatDau >= DateTime.Now && 
                            lc.ThoiGianBatDau <= DateTime.Now.AddDays(7))
                .OrderBy(lc => lc.ThoiGianBatDau)
                .ToListAsync();

            // Chuyển đổi sang DTO để tránh vòng lặp serialization
            var lichChieuDto = lichChieuSapToi.Select(lc => new ScheduleDto
            {
                MaLichChieu = lc.MaLichChieu,
                ThoiGianBatDau = lc.ThoiGianBatDau,
                ThoiGianKetThuc = lc.ThoiGianKetThuc,
                Gia = lc.Gia,
                MaPhim = lc.MaPhim,
                Phim = new PhimDto
                {
                    MaPhim = lc.Phim.MaPhim,
                    TenPhim = lc.Phim.TenPhim,
                    TheLoai = lc.Phim.TheLoai,
                    ThoiLuong = lc.Phim.ThoiLuong,
                    DoTuoiPhanAnh = lc.Phim.DoTuoiPhanAnh
                },
                PhongChieu = new PhongChieuDto
                {
                    MaPhong = lc.PhongChieu.MaPhong,
                    TenPhong = lc.PhongChieu.TenPhong,
                    SoChoNgoi = lc.PhongChieu.SoChoNgoi
                }
            }).ToList();

            ViewBag.LichChieuSapToi = lichChieuSapToi; // Cho server-side rendering
            ViewBag.LichChieuDto = lichChieuDto; // Cho JavaScript serialization
            ViewBag.CurrentTheLoai = theLoai;
            ViewBag.CurrentSearch = searchTerm;

            return View(phimList);
        }

        // Chi tiết phim
        public async Task<IActionResult> ChiTietPhim(string id)
        {
            WriteErrorLog($"[ACTION] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ChiTietPhim] Email: {HttpContext.Session.GetString("Email")}, id: {id}, SessionKeys: {string.Join(",", HttpContext.Session.Keys)}");
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var phim = await _context.Phims
                .Include(p => p.LichChieus)
                    .ThenInclude(lc => lc.PhongChieu)
                .FirstOrDefaultAsync(p => p.MaPhim == id);

            if (phim == null)
            {
                return NotFound();
            }

            // Lấy lịch chiếu từ hôm nay trở đi
            var lichChieuTuHienTai = phim.LichChieus
                .Where(lc => lc.ThoiGianBatDau >= DateTime.Now)
                .OrderBy(lc => lc.ThoiGianBatDau)
                .ToList();

            // Chuyển đổi sang object đơn giản để tránh vòng lặp serialization
            var lichChieuSimple = lichChieuTuHienTai.Select(lc => new
            {
                maLichChieu = lc.MaLichChieu,
                thoiGianBatDau = lc.ThoiGianBatDau,
                thoiGianKetThuc = lc.ThoiGianKetThuc,
                gia = lc.Gia,
                maPhim = lc.MaPhim,
                phongChieu = new
                {
                    maPhong = lc.PhongChieu.MaPhong,
                    tenPhong = lc.PhongChieu.TenPhong,
                    soChoNgoi = lc.PhongChieu.SoChoNgoi
                }
            }).ToList();

            ViewBag.LichChieus = lichChieuSimple;

            return View(phim);
        }

        // Chọn ghế ngồi
        public async Task<IActionResult> ChonGhe(string maLichChieu)
        {
            HttpContext.Session.Remove("maHoaDon");
            WriteErrorLog($"[ACTION] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ChonGhe] Email: {HttpContext.Session.GetString("Email")}, maLichChieu: {maLichChieu}, SessionKeys: {string.Join(",", HttpContext.Session.Keys)}");
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(maLichChieu))
            {
                return NotFound();
            }

            var lichChieu = await _context.LichChieus
                .Include(lc => lc.Phim)
                .Include(lc => lc.PhongChieu)
                    .ThenInclude(pc => pc.GheNgois)
                .FirstOrDefaultAsync(lc => lc.MaLichChieu == maLichChieu);

            if (lichChieu == null)
            {
                return NotFound();
            }

            var danhSachGhe = await _context.GheNgois
                .Where(g => g.MaPhong == lichChieu.MaPhong)
                .OrderBy(g => g.SoGhe)
                .ToListAsync();

            var danhSachVeDaBan = await _context.Ves
                .Where(v => v.MaLichChieu == maLichChieu && v.TrangThai == "Đã bán")
                .ToListAsync();

            var danhSachVeDaPhatHanh = await _context.Ves
                .Where(v => v.MaLichChieu == maLichChieu && (v.TrangThai == "Chưa đặt" || v.TrangThai == "Còn hạn"))
                .ToListAsync();

            // Lấy danh sách ghế đã được đặt cho lịch chiếu này
            var gheDaDat = danhSachVeDaBan.Select(v => v.MaGhe).ToList();

            var viewModel = new KhachHangChonGheViewModel
            {
                LichChieu = lichChieu,
                DanhSachGhe = danhSachGhe,
                DanhSachVeDaBan = danhSachVeDaBan,
                DanhSachVeDaPhatHanh = danhSachVeDaPhatHanh,
                GheDaDat = gheDaDat
            };

            return View(viewModel);
        }

        // Thanh toán - GET method (hỗ trợ truyền maHoaDon để thanh toán lại đơn cũ)
        public async Task<IActionResult> ThanhToan(string? maHoaDon)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var email = HttpContext.Session.GetString("Email") ?? "Unknown";
            var sessionKeys = string.Join(",", HttpContext.Session.Keys);
            var tempGioHang = HttpContext.Session.GetString("TempGioHang") ?? "null";
            System.IO.File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ACTION] [GET ThanhToan] Email: {email}, maHoaDon: {maHoaDon}, SessionKeys: {sessionKeys}, TempGioHang: {tempGioHang}\n");
            // Log chi tiết TempGioHang khi vào action
            var gioHangDebug = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("TempGioHang") ?? new List<GioHangItem>();
            WriteErrorLog($"[DEBUG] Dữ liệu TempGioHang khi vào ThanhToan: {System.Text.Json.JsonSerializer.Serialize(gioHangDebug)}");
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }
            if (!string.IsNullOrEmpty(maHoaDon))
            {
                var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);
                if (hoaDon == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy hóa đơn!";
                    return RedirectToAction("Index");
                }
                // Tính thời gian còn lại từ lúc tạo hóa đơn (1 phút = 60 giây)
                var thoiGianConLai = 60 - (int)(DateTime.Now - hoaDon.ThoiGianTao).TotalSeconds;
                if (thoiGianConLai < 0) thoiGianConLai = 0;
                // Nếu hết hạn mà trạng thái chưa phải 'Đã hủy', cập nhật luôn
                if (thoiGianConLai <= 0 && hoaDon.TrangThai != "Đã hủy")
                {
                    hoaDon.TrangThai = "Đã hủy";
                    await _context.SaveChangesAsync();
                }
                var gioHang = await _context.TempGioHangItems
                    .Where(x => x.MaHoaDon == hoaDon.MaHoaDon)
                    .Select(x => new GioHangItem
                    {
                        MaLichChieu = x.MaLichChieu,
                        MaGhe = x.MaGhe,
                        SoGhe = x.SoGhe,
                        Gia = x.Gia,
                        TenPhim = x.TenPhim,
                        ThoiGianChieu = x.ThoiGianChieu,
                        TenPhong = x.TenPhong,
                        PhongChieu = x.TenPhong
                    }).ToListAsync();
                var vouchers = await _context.Vouchers
                    .Where(v => v.ThoiGianBatDau <= DateTime.Now && v.ThoiGianKetThuc >= DateTime.Now)
                    .ToListAsync();
                var viewModel = new KhachHangThanhToanViewModel
                {
                    GioHang = gioHang,
                    Vouchers = vouchers,
                    TongTien = hoaDon.TongTien,
                    IsDirectPayment = false,
                    MaHoaDon = hoaDon.MaHoaDon,
                    ThoiGianConLai = thoiGianConLai,
                    HoaDon = hoaDon,
                    IsExpired = hoaDon.TrangThai == "Đã hủy"
                };
                return View(viewModel);
            }
            // Nếu không có maHoaDon (chọn ghế mới), luôn tạo đơn mới, reset thời gian giữ ghế
            var gioHangSession = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("TempGioHang") ?? new List<GioHangItem>();
            if (!gioHangSession.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ghế từ trang lịch chiếu để thanh toán";
                return RedirectToAction("Index");
            }
            var vouchers2 = _context.Vouchers
                .Where(v => v.ThoiGianBatDau <= DateTime.Now && v.ThoiGianKetThuc >= DateTime.Now)
                .ToList();
            var viewModel2 = new KhachHangThanhToanViewModel
            {
                GioHang = gioHangSession,
                Vouchers = vouchers2,
                TongTien = gioHangSession.Sum(x => x.Gia),
                IsDirectPayment = true,
                ThoiGianConLai = 120, // Luôn 2 phút cho trường hợp mới
                HoaDon = null,
                IsExpired = false
            };
            // Không set lại maHoaDon vào session ở đây!
            return View(viewModel2);
        }

        // Thanh toán trực tiếp từ trang chọn ghế - POST method
        [HttpPost]
        public async Task<IActionResult> ThanhToan(string maLichChieu, List<SelectedSeatViewModel> selectedSeats, decimal tongTien)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var email = HttpContext.Session.GetString("Email") ?? "Unknown";
            var sessionKeys = string.Join(",", HttpContext.Session.Keys);
            System.IO.File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ACTION] [POST ThanhToan] Email: {email}, maLichChieu: {maLichChieu}, seatIds: {string.Join(",", selectedSeats?.Select(s => s.MaGhe) ?? new List<string>())}, tongTien: {tongTien}, SessionKeys: {sessionKeys}\n");
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            // Log để debug
            Console.WriteLine($"=== NHẬN DỮ LIỆU THANH TOÁN ===");
            Console.WriteLine($"Mã lịch chiếu: {maLichChieu}");
            Console.WriteLine($"Số ghế: {selectedSeats?.Count ?? 0}");
            Console.WriteLine($"Tổng tiền: {tongTien}");

            if (selectedSeats == null || !selectedSeats.Any())
            {
                TempData["ErrorMessage"] = "Không có ghế nào được chọn";
                return RedirectToAction("ChonGhe", new { maLichChieu });
            }

            // Kiểm tra lịch chiếu tồn tại
            var lichChieu = await _context.LichChieus
                .Include(lc => lc.Phim)
                .Include(lc => lc.PhongChieu)
                .FirstOrDefaultAsync(lc => lc.MaLichChieu == maLichChieu);

            if (lichChieu == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy lịch chiếu";
                return RedirectToAction("Index");
            }

            // Tạo danh sách ghế cho giỏ hàng tạm thời
            var gioHangItems = new List<GioHangItem>();
            foreach (var seat in selectedSeats)
            {
                gioHangItems.Add(new GioHangItem
                {
                    MaLichChieu = maLichChieu,
                    MaGhe = seat.MaGhe,
                    SoGhe = seat.SoGhe,
                    Gia = seat.GiaGhe,
                    TenPhim = lichChieu.Phim.TenPhim,
                    ThoiGianChieu = lichChieu.ThoiGianBatDau,
                    TenPhong = lichChieu.PhongChieu.TenPhong,
                    PhongChieu = lichChieu.PhongChieu.TenPhong
                });
            }

            // Lưu vào session để sử dụng trong trang thanh toán
            HttpContext.Session.SetObjectAsJson("TempGioHang", gioHangItems);
            var tempGioHangAfterSave = HttpContext.Session.GetString("TempGioHang") ?? "null";
            System.IO.File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [DEBUG] TempGioHang sau khi lưu: {tempGioHangAfterSave}\n");

            // Lấy danh sách voucher có thể sử dụng
            var vouchers = await _context.Vouchers
                .Where(v => v.ThoiGianBatDau <= DateTime.Now && v.ThoiGianKetThuc >= DateTime.Now)
                .ToListAsync();

            var viewModel = new KhachHangThanhToanViewModel
            {
                GioHang = gioHangItems,
                Vouchers = vouchers,
                TongTien = tongTien,
                IsDirectPayment = true // Flag để biết đây là thanh toán trực tiếp
            };

            return View(viewModel);
        }

        // Xử lý thanh toán
        [HttpPost]
        public async Task<IActionResult> XuLyThanhToan(string? maVoucher, string paymentMethod, string? maHoaDonTemp, List<string>? seatIds)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var email = HttpContext.Session.GetString("Email") ?? "Unknown";
            var sessionKeys = string.Join(",", HttpContext.Session.Keys);
            var tempGioHang = HttpContext.Session.GetString("TempGioHang") ?? "null";
            System.IO.File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ACTION] [XuLyThanhToan] Email: {email}, paymentMethod: {paymentMethod}, maHoaDonTemp: {maHoaDonTemp}, seatIds: {string.Join(",", seatIds ?? new List<string>())}, SessionKeys: {sessionKeys}, TempGioHang: {tempGioHang}\n");
            Console.WriteLine("=== BẮT ĐẦU XỬ LÝ THANH TOÁN ===");
            Console.WriteLine($"Thời gian: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"Mã voucher nhận được: '{maVoucher}'");
            Console.WriteLine($"Phương thức thanh toán: '{paymentMethod}'");

            // Log toàn bộ session keys và giá trị
            var allSession = HttpContext.Session.Keys.ToList();
            WriteErrorLog($"Session keys: {string.Join(", ", allSession)}");
            foreach (var key in allSession)
            {
                var val = HttpContext.Session.GetString(key);
                WriteErrorLog($"Session[{key}] = {val}");
            }

            WriteErrorLog("[DEBUG] Bắt đầu lấy TempGioHang từ session");
            var gioHang = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("TempGioHang") ?? new List<GioHangItem>();
            WriteErrorLog($"[DEBUG] Dữ liệu gioHang sau khi lấy từ session: {System.Text.Json.JsonSerializer.Serialize(gioHang)}");
            WriteErrorLog($"[DEBUG] Số lượng item trong gioHang: {gioHang.Count}");

            // Nếu session rỗng nhưng có maHoaDonTemp, lấy lại từ bảng tạm
            if (!gioHang.Any() && !string.IsNullOrEmpty(maHoaDonTemp))
            {
                WriteErrorLog($"[DEBUG] Session TempGioHang rỗng, thử lấy lại từ TempGioHangItems với MaHoaDon={maHoaDonTemp}");
                gioHang = await _context.TempGioHangItems
                    .Where(x => x.MaHoaDon == maHoaDonTemp)
                    .Select(x => new GioHangItem
                    {
                        MaLichChieu = x.MaLichChieu,
                        MaGhe = x.MaGhe,
                        SoGhe = x.SoGhe,
                        Gia = x.Gia,
                        TenPhim = x.TenPhim,
                        ThoiGianChieu = x.ThoiGianChieu,
                        TenPhong = x.TenPhong,
                        PhongChieu = x.TenPhong
                    }).ToListAsync();
                WriteErrorLog($"[DEBUG] Đã lấy lại {gioHang.Count} item từ TempGioHangItems theo MaHoaDon={maHoaDonTemp}");
            }

            if (!gioHang.Any())
            {
                WriteErrorLog("[ERROR] Giỏ hàng trống - chuyển hướng về trang chủ");
                TempData["ErrorMessage"] = "Giỏ hàng trống. Vui lòng chọn ghế từ trang lịch chiếu để thanh toán.";
                return RedirectToAction("Index");
            }

            // Log chi tiết từng item trong giỏ hàng
            Console.WriteLine("Chi tiết giỏ hàng:");
            for (int i = 0; i < gioHang.Count; i++)
            {
                var item = gioHang[i];
                Console.WriteLine($"  Item {i + 1}: Ghế {item.SoGhe}, Phim: {item.TenPhim}, Giá: {item.Gia:N0}, Lịch chiếu: {item.MaLichChieu}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            Console.WriteLine("Đã bắt đầu transaction database");
            
            try
            {
                // Khai báo và gán giá trị cho các biến trước khi dùng
                var tongTien = gioHang.Sum(item => item.Gia);
                decimal tienGiamGia = 0;
                decimal tongTienSauGiam = tongTien;
                var khachHangEntity = HttpContext.Session.GetString("MaKhachHang");

                // Áp dụng voucher nếu có
                Console.WriteLine("Bước 2: Xử lý voucher");
                Voucher? voucher = null;
                if (!string.IsNullOrEmpty(maVoucher))
                {
                    Console.WriteLine($"Tìm kiếm voucher: {maVoucher}");
                    voucher = await _context.Vouchers
                        .FirstOrDefaultAsync(v => v.MaGiamGia == maVoucher && 
                                                 v.ThoiGianBatDau <= DateTime.Now && 
                                                 v.ThoiGianKetThuc >= DateTime.Now);
                    if (voucher != null)
                    {
                        tienGiamGia = tongTien * voucher.PhanTramGiam / 100;
                        tongTienSauGiam = tongTien - tienGiamGia;
                        Console.WriteLine($"Voucher hợp lệ: Giảm {voucher.PhanTramGiam}% = {tienGiamGia:N0} VNĐ");
                        Console.WriteLine($"Tổng tiền sau giảm: {tongTienSauGiam:N0} VNĐ");
                    }
                    else
                    {
                        Console.WriteLine("Voucher không hợp lệ hoặc hết hạn");
                    }
                }
                else
                {
                    Console.WriteLine("Không sử dụng voucher");
                }

                Console.WriteLine("Bước 3: Tạo hóa đơn");
                // Sinh mã hóa đơn duy nhất bằng Guid, chỉ lấy 8 ký tự đầu và kiểm tra tồn tại
                string maHoaDon;
                while (true)
                {
                    maHoaDon = "HD" + Guid.NewGuid().ToString("N").Substring(0, 8);
                    var exists = await _context.HoaDons.AsNoTracking().AnyAsync(h => h.MaHoaDon == maHoaDon);
                    if (!exists) break;
                }
                // Tạo hóa đơn với MaHoaDon tạm thời (tối đa 10 ký tự)
                var tempMaHoaDon = "TMP" + Guid.NewGuid().ToString("N").Substring(0, 7); // 10 ký tự
                var hoaDon = new HoaDon
                {
                    MaHoaDon = tempMaHoaDon, // KHÔNG để null!
                    TongTien = tongTienSauGiam,
                    ThoiGianTao = DateTime.Now,
                    SoLuong = gioHang.Count,
                    MaKhachHang = khachHangEntity ?? string.Empty,
                    MaNhanVien = "GUEST",
                    TrangThai = paymentMethod == "banking" ? "Chờ chuyển khoản" : "Đã thanh toán"
                };
                WriteErrorLog($"[DEBUG] Tạo HoaDon: MaHoaDon={tempMaHoaDon}, TongTien={tongTienSauGiam}, ThoiGianTao={DateTime.Now}, SoLuong={gioHang.Count}, MaKhachHang={khachHangEntity}, TrangThai={paymentMethod}");
                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync(); // SQL Server sẽ tự động sinh Id
                WriteErrorLog($"[DEBUG] Sau SaveChanges HoaDon: Id={hoaDon.Id}, MaHoaDon={hoaDon.MaHoaDon}, TongTien={hoaDon.TongTien}, SoLuong={hoaDon.SoLuong}, MaKhachHang={hoaDon.MaKhachHang}, TrangThai={hoaDon.TrangThai}");
                // Gán lại MaHoaDon dựa trên Id (HD + 7 số, tối đa 9 ký tự)
                hoaDon.MaHoaDon = $"HD{hoaDon.Id:D7}";
                _context.HoaDons.Update(hoaDon);
                await _context.SaveChangesAsync(); // Lưu lại mã hóa đơn mới
                Console.WriteLine($"Mã hóa đơn được tạo: {maHoaDon}");

                // Cập nhật lại MaHoaDon trong bảng tạm nếu có
                var tempItems = _context.TempGioHangItems.Where(x => x.MaHoaDon == maHoaDon).ToList();
                if (tempItems.Any())
                {
                    foreach (var item in tempItems)
                    {
                        item.MaHoaDon = hoaDon.MaHoaDon;
                    }
                    await _context.SaveChangesAsync();
                    WriteErrorLog($"[DEBUG] Đã cập nhật MaHoaDon trong TempGioHangItems từ {maHoaDon} sang {hoaDon.MaHoaDon}");
                }

                Console.WriteLine($"Tổng tiền gốc: {tongTien:N0} VNĐ");

                // Tạo vé và chi tiết hóa đơn
                Console.WriteLine("Bước 4: Tạo vé và chi tiết hóa đơn");
                var soThuTuVe = 1;
                // Lấy số thứ tự vé
                var lastVe = await _context.Ves.OrderByDescending(v => v.MaVe).FirstOrDefaultAsync();
                if (lastVe != null && lastVe.MaVe.StartsWith("VE"))
                {
                    var soVeStr = lastVe.MaVe.Substring(2);
                    if (!int.TryParse(soVeStr, out var soHienTaiVe))
                    {
                        WriteErrorLog($"[ERROR] Không parse được số thứ tự vé từ MaVe={lastVe.MaVe}");
                        soThuTuVe = 1;
                    }
                    else
                    {
                        soThuTuVe = soHienTaiVe + 1;
                    }
                }
                else
                {
                    WriteErrorLog("[DEBUG] Không tìm thấy vé trước đó hoặc MaVe không đúng format.");
                    soThuTuVe = 1;
                }
                // Lấy số thứ tự CTHD
                var soThuTuCTHD = 1;
                var lastCTHD = await _context.CTHDs.OrderByDescending(c => c.MaCTHD).FirstOrDefaultAsync();
                if (lastCTHD != null && lastCTHD.MaCTHD.StartsWith("CT"))
                {
                    var soCTHDStr = lastCTHD.MaCTHD.Substring(2);
                    if (!int.TryParse(soCTHDStr, out var soHienTaiCTHD))
                    {
                        WriteErrorLog($"[ERROR] Không parse được số thứ tự CTHD từ MaCTHD={lastCTHD.MaCTHD}");
                        soThuTuCTHD = 1;
                    }
                    else
                    {
                        soThuTuCTHD = soHienTaiCTHD + 1;
                    }
                }
                else
                {
                    WriteErrorLog("[DEBUG] Không tìm thấy CTHD trước đó hoặc MaCTHD không đúng format.");
                    soThuTuCTHD = 1;
                }

                foreach (var item in gioHang)
                {
                    WriteErrorLog($"[DEBUG] Xử lý ghế: MaGhe={item.MaGhe}, MaLichChieu={item.MaLichChieu}, SoGhe={item.SoGhe}");
                    var lichChieuInfo = await _context.LichChieus
                        .Include(lc => lc.Phim)
                        .Include(lc => lc.PhongChieu)
                        .FirstOrDefaultAsync(lc => lc.MaLichChieu == item.MaLichChieu);
                    if (lichChieuInfo == null)
                    {
                        WriteErrorLog($"[ERROR] Không tìm thấy lịch chiếu với MaLichChieu={item.MaLichChieu}");
                        TempData["ErrorMessage"] = "Không tìm thấy lịch chiếu. Vui lòng thử lại!";
                        return RedirectToAction("ChonGhe", new { maLichChieu = item.MaLichChieu });
                    }
                    if (lichChieuInfo.Phim == null)
                    {
                        WriteErrorLog($"[ERROR] Lịch chiếu {item.MaLichChieu} không có thông tin phim");
                        TempData["ErrorMessage"] = "Dữ liệu phim không hợp lệ. Vui lòng thử lại!";
                        return RedirectToAction("ChonGhe", new { maLichChieu = item.MaLichChieu });
                    }
                    if (lichChieuInfo.PhongChieu == null)
                    {
                        WriteErrorLog($"[ERROR] Lịch chiếu {item.MaLichChieu} không có thông tin phòng chiếu");
                        TempData["ErrorMessage"] = "Dữ liệu phòng chiếu không hợp lệ. Vui lòng thử lại!";
                        return RedirectToAction("ChonGhe", new { maLichChieu = item.MaLichChieu });
                    }
                    Console.WriteLine($"Xử lý ghế: {item.SoGhe} - Lịch chiếu: {item.MaLichChieu}");
                    
                    // Kiểm tra ghế có bị đặt trong thời gian này không
                    var gheExist = await _context.Ves
                        .FirstOrDefaultAsync(v => v.MaLichChieu == item.MaLichChieu && 
                                                 v.MaGhe == item.MaGhe && 
                                                 v.TrangThai == "Đã đặt");
                    
                    if (gheExist != null)
                    {
                        Console.WriteLine($"LỖI: Ghế {item.SoGhe} đã bị đặt (MaVe: {gheExist.MaVe})");
                        throw new Exception($"Ghế {item.SoGhe} đã được đặt bởi khách hàng khác");
                    }

                    var maVe = $"VE{soThuTuVe:D3}";
                    Console.WriteLine($"Tạo vé: {maVe} cho ghế {item.SoGhe}");
                    soThuTuVe++;
                    
                    // Lấy thông tin phim và phòng từ lịch chiếu
                    var ve = new Ve
                    {
                        MaVe = maVe,
                        TrangThai = "Chưa bán",
                        SoGhe = item.SoGhe,
                        TenPhim = item.TenPhim,
                        HanSuDung = item.ThoiGianChieu.AddHours(2),
                        Gia = item.Gia,
                        TenPhong = item.TenPhong,
                        MaGhe = item.MaGhe,
                        MaLichChieu = item.MaLichChieu,
                        MaPhim = lichChieuInfo.MaPhim,
                        MaPhong = lichChieuInfo.MaPhong
                    };

                    _context.Ves.Add(ve);
                    Console.WriteLine($"Đã thêm vé {maVe} vào context");
                    WriteErrorLog($"[DEBUG] Tạo vé: MaVe={maVe}, MaPhim={lichChieuInfo.MaPhim}, MaPhong={lichChieuInfo.MaPhong}, MaLichChieu={item.MaLichChieu}, Gia={item.Gia}, SoGhe={item.SoGhe}, TenPhim={item.TenPhim}, TenPhong={item.TenPhong}");

                    // Tạo chi tiết hóa đơn
                    var maCTHD = $"CT{Guid.NewGuid().ToString("N").Substring(0, 10)}";

                    var cthd = new CTHD
                    {
                        MaCTHD = maCTHD,
                        DonGia = item.Gia,
                        MaVe = maVe,
                        MaHoaDon = hoaDon.MaHoaDon, // Sử dụng mã hóa đơn thực tế đã cập nhật!
                        HoaDonId = hoaDon.Id
                    };

                    _context.CTHDs.Add(cthd);
                    Console.WriteLine($"Đã thêm CTHD {maCTHD} vào context (Vé: {maVe}, HĐ: {maHoaDon}, Giá: {item.Gia:N0})");
                    WriteErrorLog($"[DEBUG] Tạo CTHD: MaCTHD={maCTHD}, DonGia={item.Gia}, MaVe={maVe}, MaHoaDon={maHoaDon}, HoaDonId={hoaDon.Id}");
                }

                // Lưu voucher sử dụng nếu có
                Console.WriteLine("Bước 5: Xử lý voucher sử dụng");
                if (voucher != null)
                {
                    Console.WriteLine($"Lưu thông tin sử dụng voucher: {voucher.MaGiamGia}");
                    var hdVoucher = new HDVoucher
                    {
                        MaHoaDon = maHoaDon,
                        MaGiamGia = voucher.MaGiamGia,
                        SoLuongVoucher = 1,
                        TongTien = tongTienSauGiam,
                        HoaDonId = hoaDon.Id
                    };

                    _context.HDVouchers.Add(hdVoucher);
                    Console.WriteLine($"Đã thêm HDVoucher vào context");
                }

                // Cập nhật điểm tích lũy cho khách hàng
                Console.WriteLine("Bước 6: Cập nhật điểm tích lũy");
                // Đổi tên biến bên trong nhánh if để tránh trùng tên
                if (!string.IsNullOrEmpty(khachHangEntity))
                {
                    var khachHangDb = await _context.KhachHangs.FindAsync(khachHangEntity);
                    if (khachHangDb != null)
                    {
                        var diemTichLuyMoi = (int)(tongTienSauGiam / 10000); // 1 điểm = 10,000 VNĐ
                        var diemCu = khachHangDb.DiemTichLuy;
                        khachHangDb.DiemTichLuy += diemTichLuyMoi;
                        _context.KhachHangs.Update(khachHangDb);

                        Console.WriteLine($"Cập nhật điểm tích lũy cho KH {khachHangEntity}: {diemCu} → {khachHangDb.DiemTichLuy} (+{diemTichLuyMoi} điểm)");
                    }
                    else
                    {
                        Console.WriteLine($"CẢNH BÁO: Không tìm thấy khách hàng {khachHangEntity}");
                    }
                }

                Console.WriteLine("Bước 7: Lưu tất cả thay đổi vào database");
                await _context.SaveChangesAsync();
                Console.WriteLine("Đã lưu thành công vào database");

                Console.WriteLine("Bước 8: Commit transaction");
                await transaction.CommitAsync();
                Console.WriteLine("Transaction đã được commit thành công");

                // Xóa giỏ hàng tạm thời
                Console.WriteLine("Bước 9: Dọn dẹp session");
                HttpContext.Session.Remove("TempGioHang");
                Console.WriteLine("Đã xóa giỏ hàng tạm thời khỏi session");

                Console.WriteLine($"=== THANH TOÁN THÀNH CÔNG ===");
                Console.WriteLine($"Mã hóa đơn: {maHoaDon}");
                Console.WriteLine($"Tổng tiền: {tongTienSauGiam:N0} VNĐ");
                Console.WriteLine($"Số vé: {gioHang.Count}");

                TempData["SuccessMessage"] = "Thanh toán thành công!";

                if (paymentMethod == "banking")
                {
                    // 1. Lưu hóa đơn với trạng thái "Chờ chuyển khoản"
                    // ĐÃ ĐƯỢC ADD VÀ SAVE Ở NGOÀI, KHÔNG LÀM LẠI!

                    // 2. Lưu thông tin ghế/vé vào bảng tạm TempGioHangItems (sau khi đã có hoaDon.MaHoaDon thực tế)
                    foreach (var item in gioHang)
                    {
                        // Lấy MaPhim, MaPhong từ LichChieu nếu không có trong item
                        var lichChieu = await _context.LichChieus.FirstOrDefaultAsync(lc => lc.MaLichChieu == item.MaLichChieu);
                        var temp = new TempGioHangItem
                        {
                            MaHoaDon = hoaDon.MaHoaDon, // luôn dùng mã hóa đơn thực tế
                            MaGhe = item.MaGhe,
                            SoGhe = item.SoGhe,
                            Gia = item.Gia,
                            MaLichChieu = item.MaLichChieu,
                            MaPhim = (item.GetType().GetProperty("MaPhim") != null ? (string)item.GetType().GetProperty("MaPhim").GetValue(item) : (lichChieu?.MaPhim ?? "")),
                            TenPhim = item.TenPhim,
                            MaPhong = (item.GetType().GetProperty("MaPhong") != null ? (string)item.GetType().GetProperty("MaPhong").GetValue(item) : (lichChieu?.MaPhong ?? "")),
                            TenPhong = item.TenPhong,
                            ThoiGianChieu = item.ThoiGianChieu
                        };
                        _context.TempGioHangItems.Add(temp);
                    }
                    await _context.SaveChangesAsync();

                    // 3. Hiển thị hướng dẫn chuyển khoản cho khách (có thể là view riêng)
                    // Trước khi tạo mới hóa đơn chuyển khoản, kiểm tra đã có hóa đơn chờ chuyển khoản chưa
                    if (paymentMethod == "banking")
                    {
                        var khachHangCheck = khachHangEntity;
                        var tongTienGioHang = gioHang.Sum(x => x.Gia);
                        var hoaDonCho = await _context.HoaDons.FirstOrDefaultAsync(hd => hd.MaKhachHang == khachHangCheck && hd.TrangThai == "Chờ chuyển khoản" && hd.TongTien == tongTienGioHang);
                        if (hoaDonCho != null)
                        {
                            // Nếu đã có hóa đơn chờ chuyển khoản, redirect sang GET hướng dẫn chuyển khoản
                            return RedirectToAction("HuongDanChuyenKhoan", new { maHoaDon = hoaDonCho.MaHoaDon });
                        }
                    }
                    return RedirectToAction("HuongDanChuyenKhoan", new { maHoaDon = hoaDon.MaHoaDon });
                }
                return RedirectToAction("ThanhToanThanhCong", new { maHoaDon = maHoaDon });
            }
            catch (Exception ex)
            {
                WriteErrorLog($"LỖI TRONG QUÁ TRÌNH THANH TOÁN: {ex.Message} | StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    WriteErrorLog($"Inner exception: {ex.InnerException.Message}");
                }
                try
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("Transaction đã được rollback");
                }
                catch (Exception rollbackEx)
                {
                    Console.WriteLine($"Lỗi khi rollback transaction: {rollbackEx.Message}");
                }

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thanh toán: " + ex.Message;
                WriteErrorLog($"Chuyển hướng về trang thanh toán với thông báo lỗi: {TempData["ErrorMessage"]}");
                return RedirectToAction("ThanhToan");
            }
        }

        // Trang thành công
        public async Task<IActionResult> ThanhToanThanhCong(string maHoaDon)
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            if (string.IsNullOrEmpty(maHoaDon))
            {
                return RedirectToAction("Index");
            }

            var hoaDon = await _context.HoaDons
                .Include(hd => hd.KhachHang)
                .FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);

            if (hoaDon == null)
            {
                return NotFound();
            }

            // Kiểm tra trạng thái thanh toán
            if (hoaDon.TrangThai != "Đã thanh toán")
            {
                TempData["ErrorMessage"] = "Đơn hàng này chưa được thanh toán! Vui lòng thanh toán để hoàn tất.";
                return RedirectToAction("ThanhToan", new { maHoaDon = hoaDon.MaHoaDon });
            }

            // Cập nhật trạng thái vé sang 'Đã bán' nếu chưa cập nhật
            var cthds = await _context.CTHDs.Where(ct => ct.MaHoaDon == maHoaDon).ToListAsync();
            foreach (var cthd in cthds)
            {
                var ve = await _context.Ves.FirstOrDefaultAsync(v => v.MaVe == cthd.MaVe);
                if (ve != null && ve.TrangThai != "Đã bán")
                {
                    ve.TrangThai = "Đã bán";
                    _context.Ves.Update(ve);
                }
            }
            await _context.SaveChangesAsync();

            // Lấy chi tiết hóa đơn và vé
            var chiTietHoaDon = await _context.CTHDs
                .Include(ct => ct.Ve)
                .Where(ct => ct.MaHoaDon == maHoaDon)
                .ToListAsync();

            // Lấy thông tin lịch chiếu cho từng vé
            var chiTietVe = new List<VeChiTietViewModel>();
            foreach (var cthd in chiTietHoaDon)
            {
                var ve = cthd.Ve;
                if (ve != null)
                {
                    // Lấy thông tin lịch chiếu
                    var lichChieu = await _context.LichChieus
                        .FirstOrDefaultAsync(lc => lc.MaLichChieu == ve.MaLichChieu);

                    chiTietVe.Add(new VeChiTietViewModel
                    {
                        MaVe = ve.MaVe,
                        TenPhim = ve.TenPhim,
                        TenPhong = ve.TenPhong,
                        SoGhe = ve.SoGhe,
                        ThoiGianChieu = lichChieu?.ThoiGianBatDau ?? DateTime.Now,
                        HanSuDung = ve.HanSuDung,
                        Gia = ve.Gia,
                        TrangThai = ve.TrangThai
                    });
                }
            }

            // Lấy thông tin voucher nếu có
            var hdVoucher = await _context.HDVouchers
                .Include(hv => hv.Voucher)
                .FirstOrDefaultAsync(hv => hv.MaHoaDon == maHoaDon);

            // Tính điểm tích lũy nhận được
            var diemTichLuyNhan = (int)(hoaDon.TongTien / 10000);

            var viewModel = new ThanhToanThanhCongViewModel
            {
                HoaDon = hoaDon,
                ChiTietVe = chiTietVe,
                VoucherSuDung = hdVoucher?.Voucher,
                TienGiamGia = hdVoucher?.TongTien ?? 0,
                KhachHang = hoaDon.KhachHang,
                DiemTichLuyNhan = diemTichLuyNhan
            };

            return View(viewModel);
        }

        // Lịch sử đặt vé
        public async Task<IActionResult> LichSuDatVe()
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            
            var lichSu = await _context.HoaDons
                .Include(hd => hd.CTHDs)
                    .ThenInclude(ct => ct.Ve)
                        .ThenInclude(v => v.LichChieu)
                            .ThenInclude(lc => lc.Phim)
                .Where(hd => hd.MaKhachHang == maKhachHang)
                .OrderByDescending(hd => hd.ThoiGianTao)
                .ToListAsync();

            return View(lichSu);
        }

        // Quản lý tài khoản
        public async Task<IActionResult> TaiKhoan()
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.MaKhachHang == maKhachHang);

            if (khachHang == null)
            {
                return NotFound();
            }

            return View(khachHang);
        }

        // Cập nhật thông tin tài khoản
        [HttpPost]
        public async Task<IActionResult> CapNhatTaiKhoan(KhachHang model)
        {
            if (!IsCustomerLoggedIn())
            {
                return RedirectToAction("Login", "Auth");
            }

            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(kh => kh.MaKhachHang == maKhachHang);

            if (khachHang == null)
            {
                return NotFound();
            }

            khachHang.HoTen = model.HoTen;
            khachHang.SDT = model.SDT;

            try
            {
                await _context.SaveChangesAsync();
                
                // Cập nhật session
                HttpContext.Session.SetString("TenKhachHang", khachHang.HoTen);
                
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("TaiKhoan");
        }

        // API để lấy thống kê khách hàng
        [HttpGet]
        public async Task<IActionResult> GetThongKeKhachHang()
        {
            if (!IsCustomerLoggedIn())
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            
            try
            {
                // Lấy tổng số vé đã mua thông qua HoaDon -> CTHD -> Ve
                var tongSoVe = await _context.CTHDs
                    .Where(cthd => cthd.HoaDon.MaKhachHang == maKhachHang)
                    .CountAsync();

                // Lấy tổng số tiền đã chi tiêu
                var tongChiTieu = await _context.HoaDons
                    .Where(hd => hd.MaKhachHang == maKhachHang)
                    .SumAsync(hd => hd.TongTien);

                // Lấy thể loại phim yêu thích (thể loại được mua nhiều nhất)
                var theLoaiYeuThich = await _context.CTHDs
                    .Where(cthd => cthd.HoaDon.MaKhachHang == maKhachHang)
                    .Include(cthd => cthd.Ve)
                    .ThenInclude(v => v.Phim)
                    .GroupBy(cthd => cthd.Ve.Phim.TheLoai)
                    .OrderByDescending(g => g.Count())
                    .Select(g => g.Key)
                    .FirstOrDefaultAsync() ?? "Chưa có";

                // Lấy thông tin khách hàng để có điểm tích lũy
                var khachHang = await _context.KhachHangs
                    .FirstOrDefaultAsync(kh => kh.MaKhachHang == maKhachHang);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        tongSoVe = tongSoVe,
                        tongChiTieu = tongChiTieu,
                        theLoaiYeuThich = theLoaiYeuThich,
                        diemTichLuy = khachHang?.DiemTichLuy ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API để lấy trạng thái ghế real-time
        [HttpGet]
        public async Task<IActionResult> GetTrangThaiGhe(string maLichChieu)
        {
            if (!IsCustomerLoggedIn())
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            try
            {
                var danhSachVeDaBan = await _context.Ves
                    .Where(v => v.MaLichChieu == maLichChieu && v.TrangThai == "Đã bán")
                    .Select(v => v.MaGhe)
                    .ToListAsync();

                var danhSachVeDaPhatHanh = await _context.Ves
                    .Where(v => v.MaLichChieu == maLichChieu && (v.TrangThai == "Chưa đặt" || v.TrangThai == "Còn hạn"))
                    .Select(v => v.MaGhe)
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        gheDaBan = danhSachVeDaBan,
                        gheDaPhatHanh = danhSachVeDaPhatHanh
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Model cho response API banking
        public class BankingTransaction
        {
            [System.Text.Json.Serialization.JsonPropertyName("transactionID")]
            [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
            public string TransactionID { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("amount")]
            public decimal Amount { get; set; }

            [System.Text.Json.Serialization.JsonPropertyName("description")]
            public string Description { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("transactionDate")]
            public string TransactionDate { get; set; } = string.Empty;

            [System.Text.Json.Serialization.JsonPropertyName("type")]
            public string Type { get; set; } = string.Empty;
        }

        // Converter cho phép đọc số hoặc chuỗi thành string
        public class FlexibleStringConverter : System.Text.Json.Serialization.JsonConverter<string>
        {
            public override string Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
            {
                if (reader.TokenType == System.Text.Json.JsonTokenType.String)
                    return reader.GetString() ?? "";
                if (reader.TokenType == System.Text.Json.JsonTokenType.Number)
                    return reader.GetInt64().ToString();
                return "";
            }
            public override void Write(System.Text.Json.Utf8JsonWriter writer, string value, System.Text.Json.JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }

        // Endpoint cho cron job kiểm tra giao dịch banking
        [HttpPost]
        [Route("/api/cron/check-banking")]
        public async Task<IActionResult> CronCheckBanking()
        {
            var pendingOrders = await _context.HoaDons
                .Where(h => h.TrangThai == "Chờ chuyển khoản")
                .ToListAsync();

            if (!pendingOrders.Any())
                return Ok(new { message = "No pending orders." });

            var apiUrl = "https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
                return StatusCode(502, "Banking API error");

            var json = await response.Content.ReadAsStringAsync();
            WriteErrorLog("[DEBUG] JSON banking API: " + json);
            List<BankingTransaction> transactions = new List<BankingTransaction>();
            using (var doc = System.Text.Json.JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("transactions", out var txArr))
                {
                    transactions = System.Text.Json.JsonSerializer.Deserialize<List<BankingTransaction>>(txArr.GetRawText()) ?? new List<BankingTransaction>();
                }
                else
                {
                    transactions = new List<BankingTransaction>();
                }
            }

            int updated = 0;
            foreach (var order in pendingOrders)
            {
                var matched = transactions.FirstOrDefault(t => t.Description != null && t.Description.Contains(order.MaHoaDon, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    // Nếu hóa đơn đã có vé và chi tiết hóa đơn thì bỏ qua
                    var daCoCTHD = await _context.CTHDs.AnyAsync(c => c.MaHoaDon == order.MaHoaDon);
                    if (!daCoCTHD)
                    {
                        // Lấy thông tin các ghế đã lưu cho hóa đơn này (giả sử có bảng tạm hoặc lưu thông tin ghế trong hóa đơn hoặc bảng liên quan)
                        // Ở đây giả lập: lấy từ bảng tạm hoặc custom logic, ví dụ: order.MaKhachHang, order.ThoiGianTao, ...
                        // Nếu không có, bỏ qua tạo vé/CTHD
                        // --- BẮT ĐẦU GIẢ LẬP ---
                        // Giả sử có bảng tạm hoặc có thể truy xuất lại các ghế đã đặt cho hóa đơn này
                        // Nếu hệ thống bạn lưu thông tin ghế ở đâu đó, hãy thay thế đoạn này cho phù hợp
                        var tempSeats = await _context.TempGioHangItems
                            .Where(x => x.MaHoaDon == order.MaHoaDon)
                            .ToListAsync();
                        if (tempSeats.Count == 0) continue;
                        // --- KẾT THÚC GIẢ LẬP ---
                        // Tạo mã vé, mã CTHD tự động
                        var lastVe = await _context.Ves.OrderByDescending(v => v.MaVe).FirstOrDefaultAsync();
                        int soThuTuVe = 1;
                        if (lastVe != null && lastVe.MaVe.StartsWith("VE"))
                        {
                            int.TryParse(lastVe.MaVe.Substring(2), out soThuTuVe);
                            soThuTuVe++;
                        }
                        var soThuTuCTHD = 1;
                        var lastCTHD = await _context.CTHDs.OrderByDescending(c => c.MaCTHD).FirstOrDefaultAsync();
                        if (lastCTHD != null && lastCTHD.MaCTHD.StartsWith("CT"))
                        {
                            int.TryParse(lastCTHD.MaCTHD.Substring(2), out soThuTuCTHD);
                            soThuTuCTHD++;
                        }
                        foreach (var seat in tempSeats)
                        {
                            var maVe = $"VE{soThuTuVe:D3}";
                            soThuTuVe++;
                            var ve = new Ve
                            {
                                MaVe = maVe,
                                TrangThai = "Đã bán",
                                SoGhe = seat.SoGhe,
                                TenPhim = seat.TenPhim,
                                HanSuDung = seat.ThoiGianChieu.AddHours(2),
                                Gia = seat.Gia,
                                TenPhong = seat.TenPhong,
                                MaGhe = seat.MaGhe,
                                MaLichChieu = seat.MaLichChieu,
                                MaPhim = seat.MaPhim,
                                MaPhong = seat.MaPhong
                            };
                            _context.Ves.Add(ve);
                            var maCTHD = $"CT{soThuTuCTHD:D3}";
                            soThuTuCTHD++;
                            maCTHD = $"CT{Guid.NewGuid().ToString("N").Substring(0, 10)}";
                            var cthd = new CTHD
                            {
                                MaCTHD = maCTHD,
                                DonGia = seat.Gia,
                                MaVe = maVe,
                                MaHoaDon = order.MaHoaDon,
                                HoaDonId = order.Id
                            };
                            _context.CTHDs.Add(cthd);
                        }
                    }
                    // Cập nhật trạng thái hóa đơn
                    order.TrangThai = "Đã thanh toán";
                    updated++;
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { updated });
        }

        // Bổ sung: Nếu người dùng truy cập GET vào XuLyThanhToan, redirect về Index
        [HttpGet]
        public IActionResult XuLyThanhToan()
        {
            return RedirectToAction("Index");
        }

        // Action GET hướng dẫn chuyển khoản, chỉ load lại hóa đơn cũ
        [HttpGet]
        public async Task<IActionResult> HuongDanChuyenKhoan(string maHoaDon)
        {
            var hoaDon = await _context.HoaDons.FirstOrDefaultAsync(hd => hd.MaHoaDon == maHoaDon);
            if (hoaDon == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index");
            }
            var thoiGianConLai = 120 - (int)(DateTime.Now - hoaDon.ThoiGianTao).TotalSeconds;
            if (thoiGianConLai < 0) thoiGianConLai = 0;
            var viewModel = new CinemaManagement.ViewModels.HuongDanChuyenKhoanViewModel
            {
                MaHoaDon = hoaDon.MaHoaDon,
                SoTien = hoaDon.TongTien,
                ThoiGianConLai = thoiGianConLai
            };
            return View(viewModel);
        }

        [HttpGet]
        [Route("/api/cron/check-banking")]
        public async Task<IActionResult> GetBankingCronStatus()
        {
            var pendingOrders = await _context.HoaDons
                .Where(h => h.TrangThai == "Chờ chuyển khoản")
                .ToListAsync();

            var apiUrl = "https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123";
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
                return StatusCode(502, "Banking API error");
            var json = await response.Content.ReadAsStringAsync();
            WriteErrorLog("[DEBUG] JSON banking API: " + json);
            List<BankingTransaction> transactions = new List<BankingTransaction>();
            using (var doc = System.Text.Json.JsonDocument.Parse(json))
            {
                if (doc.RootElement.TryGetProperty("transactions", out var txArr))
                {
                    transactions = System.Text.Json.JsonSerializer.Deserialize<List<BankingTransaction>>(txArr.GetRawText()) ?? new List<BankingTransaction>();
                }
                else
                {
                    transactions = new List<BankingTransaction>();
                }
            }

            var matchedOrders = new List<object>();
            int updated = 0;
            foreach (var order in pendingOrders)
            {
                var matched = transactions.FirstOrDefault(t => t.Description != null && t.Description.Contains(order.MaHoaDon, StringComparison.OrdinalIgnoreCase));
                if (matched != null)
                {
                    // Tự động cập nhật trạng thái hóa đơn
                    order.TrangThai = "Đã thanh toán";
                    updated++;
                    matchedOrders.Add(new {
                        order.MaHoaDon,
                        order.TongTien,
                        order.ThoiGianTao,
                        order.MaKhachHang,
                        MatchedTransaction = matched
                    });
                }
            }
            if (updated > 0)
            {
                await _context.SaveChangesAsync();
            }
            return Ok(new {
                count = matchedOrders.Count,
                updated,
                matchedOrders
            });
        }

        private void WriteErrorLog(string message)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] [KhachHangController] {message}\n";
            lock (_logLock)
            {
                System.IO.File.AppendAllText(logPath, logLine);
            }
        }
    }
}
