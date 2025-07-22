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

        // Thanh toán - GET method (chuyển hướng về trang chủ vì không dùng giỏ hàng)
        public IActionResult ThanhToan()
        {
            var gioHang = HttpContext.Session.GetObjectFromJson<List<GioHangItem>>("TempGioHang") ?? new List<GioHangItem>();
            if (!gioHang.Any())
            {
                TempData["ErrorMessage"] = "Vui lòng chọn ghế từ trang lịch chiếu để thanh toán";
                return RedirectToAction("Index");
            }
            // Nếu có dữ liệu, render view với model phù hợp (ví dụ: KhachHangThanhToanViewModel)
            // Lấy các voucher khả dụng nếu cần
            var vouchers = _context.Vouchers
                .Where(v => v.ThoiGianBatDau <= DateTime.Now && v.ThoiGianKetThuc >= DateTime.Now)
                .ToList();
            var viewModel = new KhachHangThanhToanViewModel
            {
                GioHang = gioHang,
                Vouchers = vouchers,
                TongTien = gioHang.Sum(x => x.Gia),
                IsDirectPayment = true
            };
            return View(viewModel);
        }

        // Thanh toán trực tiếp từ trang chọn ghế - POST method
        [HttpPost]
        public async Task<IActionResult> ThanhToan(string maLichChieu, List<SelectedSeatViewModel> selectedSeats, decimal tongTien)
        {
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
                var ghe = await _context.GheNgois
                    .FirstOrDefaultAsync(g => g.MaGhe == seat.MaGhe);

                if (ghe != null)
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
            }

            // Lưu vào session để sử dụng trong trang thanh toán
            HttpContext.Session.SetObjectAsJson("TempGioHang", gioHangItems);

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
        public async Task<IActionResult> XuLyThanhToan(string? maVoucher, string paymentMethod)
        {
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
            WriteErrorLog($"[DEBUG] Dữ liệu gioHang sau khi lấy từ session: {JsonSerializer.Serialize(gioHang)}");
            WriteErrorLog($"[DEBUG] Số lượng item trong gioHang: {gioHang.Count}");

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
                string maKhachHang = HttpContext.Session.GetString("MaKhachHang");

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
                // Khi tạo mới HoaDon, KHÔNG gán giá trị cho Id
                var hoaDon = new HoaDon
                {
                    // KHÔNG gán Id ở đây!
                    TongTien = tongTienSauGiam,
                    ThoiGianTao = DateTime.Now,
                    SoLuong = gioHang.Count,
                    MaKhachHang = maKhachHang ?? string.Empty,
                    MaNhanVien = "GUEST",
                    TrangThai = paymentMethod == "banking" ? "Chờ chuyển khoản" : "Đã thanh toán"
                };
                _context.HoaDons.Add(hoaDon);
                await _context.SaveChangesAsync(); // SQL Server sẽ tự động sinh Id

                // Update lại MaHoaDon dựa trên Id vừa insert
                hoaDon.MaHoaDon = $"HD{hoaDon.Id:D3}";
                await _context.SaveChangesAsync(); // Lưu lại mã hóa đơn
                Console.WriteLine($"Mã hóa đơn được tạo: {maHoaDon}");

                Console.WriteLine($"Tổng tiền gốc: {tongTien:N0} VNĐ");

                // Tạo vé và chi tiết hóa đơn
                Console.WriteLine("Bước 4: Tạo vé và chi tiết hóa đơn");
                var soThuTuVe = 1;
                var lastVe = await _context.Ves.OrderByDescending(v => v.MaVe).FirstOrDefaultAsync();
                if (lastVe != null && lastVe.MaVe.StartsWith("VE"))
                {
                    if (int.TryParse(lastVe.MaVe.Substring(2), out var soHienTaiVe))
                    {
                        soThuTuVe = soHienTaiVe + 1;
                    }
                }
                Console.WriteLine($"Bắt đầu tạo vé từ số thứ tự: {soThuTuVe}");

                var soThuTuCTHD = 1;
                var lastCTHD = await _context.CTHDs.OrderByDescending(c => c.MaCTHD).FirstOrDefaultAsync();
                if (lastCTHD != null && lastCTHD.MaCTHD.StartsWith("CT"))
                {
                    if (int.TryParse(lastCTHD.MaCTHD.Substring(2), out var soHienTaiCTHD))
                    {
                        soThuTuCTHD = soHienTaiCTHD + 1;
                    }
                }
                Console.WriteLine($"Bắt đầu tạo CTHD từ số thứ tự: {soThuTuCTHD}");

                foreach (var item in gioHang)
                {
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
                    var lichChieuInfo = await _context.LichChieus
                        .Include(lc => lc.Phim)
                        .Include(lc => lc.PhongChieu)
                        .FirstOrDefaultAsync(lc => lc.MaLichChieu == item.MaLichChieu);

                    if (lichChieuInfo == null)
                    {
                        Console.WriteLine($"LỖI: Không tìm thấy lịch chiếu {item.MaLichChieu}");
                        throw new Exception($"Không tìm thấy lịch chiếu {item.MaLichChieu}");
                    }

                    var ve = new Ve
                    {
                        MaVe = maVe,
                        TrangThai = "Đã bán",
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

                    // Tạo chi tiết hóa đơn
                    var maCTHD = $"CT{soThuTuCTHD:D3}";
                    soThuTuCTHD++;

                    var cthd = new CTHD
                    {
                        MaCTHD = maCTHD,
                        DonGia = item.Gia,
                        MaVe = maVe,
                        MaHoaDon = maHoaDon,
                        HoaDonId = hoaDon.Id
                    };

                    _context.CTHDs.Add(cthd);
                    Console.WriteLine($"Đã thêm CTHD {maCTHD} vào context (Vé: {maVe}, HĐ: {maHoaDon}, Giá: {item.Gia:N0})");
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
                if (!string.IsNullOrEmpty(maKhachHang))
                {
                    var khachHang = await _context.KhachHangs.FindAsync(maKhachHang);
                    if (khachHang != null)
                    {
                        var diemTichLuyMoi = (int)(tongTienSauGiam / 10000); // 1 điểm = 10,000 VNĐ
                        var diemCu = khachHang.DiemTichLuy;
                        khachHang.DiemTichLuy += diemTichLuyMoi;
                        _context.KhachHangs.Update(khachHang);

                        Console.WriteLine($"Cập nhật điểm tích lũy cho KH {maKhachHang}: {diemCu} → {khachHang.DiemTichLuy} (+{diemTichLuyMoi} điểm)");
                    }
                    else
                    {
                        Console.WriteLine($"CẢNH BÁO: Không tìm thấy khách hàng {maKhachHang}");
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
                    // Thay thế toàn bộ logic tạo hóa đơn như sau:
                    
                    // ĐÃ ĐƯỢC ADD VÀ SAVE Ở NGOÀI, KHÔNG LÀM LẠI!

                    // 2. Lưu thông tin ghế/vé vào bảng tạm TempGioHangItems
                    foreach (var item in gioHang)
                    {
                        // Lấy MaPhim, MaPhong từ LichChieu nếu không có trong item
                        var lichChieu = await _context.LichChieus.FirstOrDefaultAsync(lc => lc.MaLichChieu == item.MaLichChieu);
                        var temp = new TempGioHangItem
                        {
                            MaHoaDon = maHoaDon,
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
                    return View("HuongDanChuyenKhoan", new CinemaManagement.ViewModels.HuongDanChuyenKhoanViewModel { MaHoaDon = maHoaDon, SoTien = tongTienSauGiam });
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
            public string Description { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public string Time { get; set; } = string.Empty;
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
            var transactions = JsonSerializer.Deserialize<List<BankingTransaction>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<BankingTransaction>();

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
                        var lastCTHD = await _context.CTHDs.OrderByDescending(c => c.MaCTHD).FirstOrDefaultAsync();
                        int soThuTuCTHD = 1;
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

        private void WriteErrorLog(string message)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] [KhachHangController] {message}\n";
            System.IO.File.AppendAllText(logPath, logLine);
        }
    }
}
