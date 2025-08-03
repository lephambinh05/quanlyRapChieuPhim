# HỆ THỐNG QUẢN LÝ RẠP CHIẾU PHIM
## Giải thích Code và Design Patterns (13-15 phút)

---

## 📋 TỔNG QUAN HỆ THỐNG

**Công nghệ sử dụng:**
- **Backend:** ASP.NET Core MVC (.NET 8.0)
- **Database:** SQL Server với Entity Framework Core
- **Frontend:** Razor Views, Bootstrap 5, JavaScript
- **Real-time:** SignalR cho chat và thông báo
- **Authentication:** JWT, Two-Factor Authentication, Google OAuth
- **Email:** SMTP với Gmail
- **Background Services:** Cron jobs cho banking check

**Kiến trúc:** MVC Pattern với Repository Pattern và Service Layer

---

## 🎯 4 CHỨC NĂNG CHÍNH (3-4 phút/chức năng)

---

### 1. 🎫 ĐẶT VÉ TRỰC TUYẾN (Online Ticket Booking)

#### **Mục đích:** Cho phép khách hàng đặt vé phim trực tuyến với giao diện thân thiện

#### **Code thực tế và Design Patterns:**

**1. MVC Pattern với Repository Pattern:**
```csharp
// Controllers/KhachHangController.cs - Dòng 46-214
public async Task<IActionResult> Index(string? theLoai, string? searchTerm, string? sortBy, string? sortOrder)
{
    try
    {
        _logger.LogInformation("[ACTION] [Index] Email: {Email}, theLoai: {TheLoai}, searchTerm: {SearchTerm}, sortBy: {SortBy}, sortOrder: {SortOrder}, SessionKeys: {SessionKeys}", 
            HttpContext.Session.GetString("Email"), theLoai, searchTerm, sortBy, sortOrder, string.Join(",", HttpContext.Session.Keys));
        
        if (!IsCustomerLoggedIn())
        {
            return RedirectToAction("Login", "Auth");
        }

        // Repository Pattern: Sử dụng Entity Framework để truy vấn dữ liệu
        var lichChieuQuery = _context.LichChieus
            .Include(lc => lc.Phim)
            .Include(lc => lc.PhongChieu)
            .Where(lc => lc.ThoiGianBatDau > DateTime.Now);

        // Strategy Pattern: Xử lý filter và sort khác nhau
        if (!string.IsNullOrEmpty(theLoai))
            lichChieuQuery = lichChieuQuery.Where(lc => lc.Phim.TheLoai == theLoai);
            
        if (!string.IsNullOrEmpty(searchTerm))
            lichChieuQuery = lichChieuQuery.Where(lc => lc.Phim.TenPhim.Contains(searchTerm));

        // Command Pattern: Xử lý sort
        lichChieuQuery = sortBy switch
        {
            "tenPhim" => sortOrder == "desc" ? lichChieuQuery.OrderByDescending(lc => lc.Phim.TenPhim) : lichChieuQuery.OrderBy(lc => lc.Phim.TenPhim),
            "thoiGian" => sortOrder == "desc" ? lichChieuQuery.OrderByDescending(lc => lc.ThoiGianBatDau) : lichChieuQuery.OrderBy(lc => lc.ThoiGianBatDau),
            "gia" => sortOrder == "desc" ? lichChieuQuery.OrderByDescending(lc => lc.Gia) : lichChieuQuery.OrderBy(lc => lc.Gia),
            _ => lichChieuQuery.OrderBy(lc => lc.ThoiGianBatDau)
        };

        var lichChieus = await lichChieuQuery.ToListAsync();
        return View(lichChieus);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Lỗi trong Index action");
        return View(new List<LichChieu>());
    }
}
```

**2. Observer Pattern (Real-time seat selection):**
```csharp
// Controllers/KhachHangController.cs - Dòng 214-283
public async Task<IActionResult> ChonGhe(string maLichChieu)
{
    HttpContext.Session.Remove("maHoaDon");
    WriteErrorLog($"[ACTION] [{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ChonGhe] Email: {HttpContext.Session.GetString("Email")}, maLichChieu: {maLichChieu}, SessionKeys: {string.Join(",", HttpContext.Session.Keys)}");
    
    if (!IsCustomerLoggedIn())
    {
        return RedirectToAction("Login", "Auth");
    }

    // Repository Pattern: Load data với Include
    var lichChieu = await _context.LichChieus
        .Include(lc => lc.Phim)
        .Include(lc => lc.PhongChieu)
            .ThenInclude(pc => pc.GheNgois)
        .FirstOrDefaultAsync(lc => lc.MaLichChieu == maLichChieu);

    // Observer Pattern: Theo dõi trạng thái ghế
    var danhSachGhe = await _context.GheNgois
        .Where(g => g.MaPhong == lichChieu.MaPhong)
        .OrderBy(g => g.SoGhe)
        .ToListAsync();

    var danhSachVeDaBan = await _context.Ves
        .Where(v => v.MaLichChieu == maLichChieu && v.TrangThai == "Đã bán")
        .ToListAsync();

    // Factory Pattern: Tạo ViewModel
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
```

**3. Builder Pattern (Tạo hóa đơn):**
```csharp
// Controllers/KhachHangController.cs - Dòng 283-998
public async Task<IActionResult> ThanhToan(string? maHoaDon)
{
    // Builder Pattern: Xây dựng hóa đơn từng bước
    var hoaDon = new HoaDon
    {
        MaHoaDon = GenerateMaHoaDon(),
        ThoiGianTao = DateTime.Now,
        TrangThai = "Chờ chuyển khoản",
        TongTien = tongTien,
        maKhachHang = maKhachHang,
        maNhanVien = null // Khách hàng tự đặt
    };

    _context.HoaDons.Add(hoaDon);
    await _context.SaveChangesAsync();

    // Observer Pattern: Thông báo real-time
    await _chatHub.SendPaymentStatus(hoaDon.MaHoaDon, "Chờ thanh toán");

    return RedirectToAction("HuongDanChuyenKhoan", new { maHoaDon = hoaDon.MaHoaDon });
}
```

#### **Luồng xử lý thực tế:**
1. **Hiển thị lịch chiếu** → MVC Pattern + Repository Pattern
2. **Chọn phim và suất chiếu** → Strategy Pattern (filter/sort)
3. **Chọn ghế** → Observer Pattern (real-time updates)
4. **Tạo hóa đơn** → Builder Pattern
5. **Thanh toán** → Command Pattern

---

### 2. 💳 THANH TOÁN CHUYỂN KHOẢN (Bank Transfer Payment)

#### **Mục đích:** Xử lý thanh toán qua chuyển khoản ngân hàng với xác thực tự động

#### **Code thực tế và Design Patterns:**

**1. Service Pattern với Background Processing:**
```csharp
// Services/BankingService.cs - Dòng 1-238
public class BankingService
{
    private readonly CinemaDbContext _context;
    private readonly string _logFilePath;
    private readonly string _apiUrl;

    public BankingService(CinemaDbContext context)
    {
        _context = context;
        _logFilePath = Path.Combine("logs", "banking_log.txt");
        _apiUrl = "https://api.sieuthicode.net/historyapiacbv2/978225ae27ff5741c543da2ae7d44123";
    }

    // Service Pattern: Xử lý logic banking
    public async Task<BankingCheckResult> CheckBankingTransactionsAsync()
    {
        var result = new BankingCheckResult
        {
            Timestamp = DateTime.Now,
            TotalTransactions = 0,
            MatchedOrders = 0,
            UpdatedOrders = 0,
            Errors = new List<string>()
        };

        try
        {
            await LogToFile($"=== BẮT ĐẦU KIỂM TRA BANKING {DateTime.Now:yyyy-MM-dd HH:mm:ss} ===");

            // Repository Pattern: Lấy đơn hàng chờ thanh toán
            var pendingOrders = await _context.HoaDons
                .Where(h => h.TrangThai == "Chờ chuyển khoản")
                .ToListAsync();

            // Strategy Pattern: Xử lý từng loại giao dịch
            var transactions = await GetBankingTransactionsAsync();
            
            foreach (var transaction in transactions)
            {
                // Command Pattern: Xử lý từng giao dịch
                var matchedOrder = await ProcessTransactionAsync(transaction, pendingOrders);
                if (matchedOrder != null)
                {
                    result.MatchedOrders++;
                    result.UpdatedOrders++;
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            result.Errors.Add(ex.Message);
            return result;
        }
    }
}
```

**2. Observer Pattern (Real-time payment status):**
```csharp
// Hubs/ChatHub.cs - Dòng 1-24
public class ChatHub : Hub
{
    private readonly CinemaDbContext _context;
    private readonly ILogger<ChatHub> _logger;
    private readonly IChatLogService _chatLogService;
    private static readonly Dictionary<string, string> _userConnections = new();

    // Observer Pattern: Thông báo trạng thái thanh toán
    public async Task SendPaymentStatus(string maHoaDon, string status)
    {
        await Clients.All.SendAsync("ReceivePaymentStatus", maHoaDon, status);
        _chatLogService.LogMessage("payment", "system", "System", $"Payment status updated: {maHoaDon} - {status}");
    }
}
```

**3. Cron Job Pattern (Background processing):**
```csharp
// Controllers/CronController.cs - Dòng 1-37
[ApiController]
[Route("api/cron")]
[AllowAnonymous]
public class CronController : ControllerBase
{
    private readonly CinemaDbContext _context;
    private readonly BankingService _bankingService;

    public CronController(CinemaDbContext context, BankingService bankingService)
    {
        _context = context;
        _bankingService = bankingService;
    }

    // Cron Job Pattern: Chạy định kỳ
    [HttpGet("")]
    public async Task<IActionResult> RunAllCrons()
    {
        var log = new List<object>();
        int totalPaid = 0, totalCancelled = 0, totalReleasedSeats = 0;

        // 1. Kiểm tra thanh toán ngân hàng
        var bankingResult = await _bankingService.CheckBankingTransactionsAsync();
        totalPaid = bankingResult.UpdatedOrders;
        log.Add(new { step = "banking", result = bankingResult });

        return Ok(new { 
            success = true, 
            totalPaid, 
            totalCancelled, 
            totalReleasedSeats,
            log 
        });
    }
}
```

#### **Luồng xử lý thực tế:**
1. **Tạo hóa đơn** → Builder Pattern
2. **Hiển thị thông tin chuyển khoản** → Template Method Pattern
3. **Kiểm tra thanh toán** → Service Pattern + Cron Job
4. **Cập nhật trạng thái** → Observer Pattern
5. **Gửi thông báo** → Command Pattern

---

### 3. 🎟️ PHÁT HÀNH VÉ (Ticket Issuance)

#### **Mục đích:** Cho phép nhân viên phát hành vé hàng loạt và quản lý vé

#### **Code thực tế và Design Patterns:**

**1. Repository Pattern với LINQ:**
```csharp
// Controllers/PhatHanhVeController.cs - Dòng 1-403
public class PhatHanhVeController : Controller
{
    private readonly CinemaDbContext _context;

    public PhatHanhVeController(CinemaDbContext context)
    {
        _context = context;
    }

    // Repository Pattern: Truy vấn dữ liệu với Include
    public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, string? maPhim, string? maPhong)
    {
        if (!IsManagerOrStaff())
        {
            return RedirectToAction("Login", "Auth");
        }

        var lichChieusQuery = _context.LichChieus
            .Include(l => l.Phim)
            .Include(l => l.PhongChieu)
            .Include(l => l.NhanVien)
            .AsQueryable();

        // Strategy Pattern: Xử lý filter khác nhau
        if (tuNgay.HasValue)
        {
            lichChieusQuery = lichChieusQuery.Where(l => l.ThoiGianBatDau.Date >= tuNgay.Value.Date);
        }

        if (denNgay.HasValue)
        {
            lichChieusQuery = lichChieusQuery.Where(l => l.ThoiGianBatDau.Date <= denNgay.Value.Date);
        }

        if (!string.IsNullOrEmpty(maPhim))
        {
            lichChieusQuery = lichChieusQuery.Where(l => l.MaPhim == maPhim);
        }

        if (!string.IsNullOrEmpty(maPhong))
        {
            lichChieusQuery = lichChieusQuery.Where(l => l.MaPhong == maPhong);
        }

        var lichChieus = await lichChieusQuery
            .OrderBy(l => l.ThoiGianBatDau)
            .ToListAsync();

        return View(lichChieus);
    }
}
```

**2. Factory Pattern (Tạo vé hàng loạt):**
```csharp
// Controllers/PhatHanhVeController.cs - Dòng 178-285
[HttpPost]
public async Task<IActionResult> PhatHanhHangLoat(string maLichChieu, List<string> selectedSeats)
{
    if (!IsManagerOrStaff())
    {
        return Json(new { success = false, message = "Không có quyền truy cập" });
    }

    try
    {
        // Factory Pattern: Tạo vé cho từng ghế được chọn
        var ves = new List<Ve>();
        foreach (var maGhe in selectedSeats)
        {
            var ve = new Ve
            {
                MaVe = GenerateMaVe(),
                MaLichChieu = maLichChieu,
                MaGhe = maGhe,
                TrangThai = "Chưa đặt",
                Gia = lichChieu.Gia,
                HanSuDung = lichChieu.ThoiGianBatDau.AddDays(30)
            };
            ves.Add(ve);
        }

        // Repository Pattern: Lưu vào database
        _context.Ves.AddRange(ves);
        await _context.SaveChangesAsync();

        return Json(new { success = true, soLuongVe = ves.Count });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}
```

**3. Command Pattern (Cập nhật trạng thái vé):**
```csharp
// Controllers/PhatHanhVeController.cs - Dòng 285-339
[HttpPost]
public async Task<IActionResult> CapNhatTrangThai(string maVe, string trangThai)
{
    if (!IsManagerOrStaff())
    {
        return Json(new { success = false, message = "Không có quyền truy cập" });
    }

    // Command Pattern: Xử lý command cập nhật trạng thái
    var ve = await _context.Ves.FirstOrDefaultAsync(v => v.MaVe == maVe);
    if (ve == null)
    {
        return Json(new { success = false, message = "Không tìm thấy vé" });
    }

    ve.TrangThai = trangThai;
    await _context.SaveChangesAsync();

    return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
}
```

#### **Luồng xử lý thực tế:**
1. **Validate input** → Template Method Pattern
2. **Generate tickets** → Factory Pattern
3. **Save to database** → Repository Pattern
4. **Update status** → Command Pattern
5. **Send notifications** → Observer Pattern

---

### 4. 📊 DASHBOARD BÁO CÁO (Reporting Dashboard)

#### **Mục đích:** Hiển thị thống kê và báo cáo chi tiết về hoạt động rạp phim

#### **Code thực tế và Design Patterns:**

**1. Repository Pattern với LINQ Aggregation:**
```csharp
// Controllers/QuanLyController.cs - Dòng 1-755
public class QuanLyController : Controller
{
    private readonly CinemaDbContext _context;

    public QuanLyController(CinemaDbContext context)
    {
        _context = context;
    }

    // Repository Pattern: Truy vấn thống kê
    public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay, string? tenPhim)
    {
        if (!IsManagerOrStaff())
        {
            return RedirectToAction("Login", "Auth");
        }

        var today = DateTime.Today;
        var thisWeek = today.AddDays(-(int)today.DayOfWeek);
        var thisMonth = new DateTime(today.Year, today.Month, 1);

        // Repository Pattern với LINQ queries
        var veQuery = _context.Ves
            .Include(v => v.LichChieu)
                .ThenInclude(lc => lc.Phim)
            .AsQueryable();

        // Strategy Pattern: Xử lý filter khác nhau
        if (tuNgay.HasValue)
            veQuery = veQuery.Where(v => v.HanSuDung.HasValue && v.HanSuDung.Value.Date >= tuNgay.Value.Date);
        if (denNgay.HasValue)
            veQuery = veQuery.Where(v => v.HanSuDung.HasValue && v.HanSuDung.Value.Date <= denNgay.Value.Date);
        if (!string.IsNullOrEmpty(tenPhim))
            veQuery = veQuery.Where(v => v.LichChieu.Phim.TenPhim.Contains(tenPhim));

        // Aggregation Pattern: Tính toán thống kê
        var dashboardData = new DashboardViewModel
        {
            VeHomNay = await veQuery.CountAsync(v => v.HanSuDung.HasValue && v.HanSuDung.Value.Date == today),
            DoanhThuHomNay = await veQuery.Where(v => v.HanSuDung.HasValue && v.HanSuDung.Value.Date == today).SumAsync(v => v.Gia),
            TongKhachHang = await _context.KhachHangs.CountAsync(),
            TopPhim = await _context.LichChieus
                .Include(lc => lc.Phim)
                .GroupBy(lc => lc.Phim.TenPhim)
                .Select(g => new PhimViewModel
                {
                    TenPhim = g.Key,
                    SoVe = g.Count()
                })
                .OrderByDescending(p => p.SoVe)
                .Take(5)
                .ToListAsync()
        };

        return View(dashboardData);
    }
}
```

**2. Strategy Pattern (Các loại báo cáo khác nhau):**
```csharp
// Controllers/QuanLyController.cs - Dòng 308-707
private async Task<List<DoanhThuTheoThangViewModel>> GetDoanhThuTheoThangAsync(int nam)
{
    var result = new List<DoanhThuTheoThangViewModel>();

    // Strategy Pattern: Xử lý từng tháng khác nhau
    for (int thang = 1; thang <= 12; thang++)
    {
        var startDate = new DateTime(nam, thang, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var doanhThu = await _context.Ves
            .Where(v => v.HanSuDung.HasValue && 
                       v.HanSuDung.Value >= startDate && 
                       v.HanSuDung.Value <= endDate)
            .SumAsync(v => v.Gia);

        var soVe = await _context.Ves
            .Where(v => v.HanSuDung.HasValue && 
                       v.HanSuDung.Value >= startDate && 
                       v.HanSuDung.Value <= endDate)
            .CountAsync();

        result.Add(new DoanhThuTheoThangViewModel
        {
            Thang = thang,
            Nam = nam,
            DoanhThu = doanhThu,
            SoVe = soVe
        });
    }

    return result;
}
```

**3. Observer Pattern (Real-time updates):**
```csharp
// Controllers/QuanLyController.cs - Dòng 707-755
[HttpGet]
public async Task<IActionResult> GetRealTimeStats()
{
    if (!IsManager())
    {
        return Json(new { success = false, message = "Không có quyền truy cập" });
    }

    // Observer Pattern: Cập nhật real-time
    var stats = new
    {
        VeHomNay = await _context.Ves
            .Where(v => v.HanSuDung.HasValue && v.HanSuDung.Value.Date == DateTime.Today)
            .CountAsync(),
        DoanhThuHomNay = await _context.Ves
            .Where(v => v.HanSuDung.HasValue && v.HanSuDung.Value.Date == DateTime.Today)
            .SumAsync(v => v.Gia),
        TongKhachHang = await _context.KhachHangs.CountAsync(),
        TongNhanVien = await _context.NhanViens.CountAsync()
    };

    return Json(new { success = true, data = stats });
}
```

#### **Luồng xử lý thực tế:**
1. **Thu thập dữ liệu** → Repository Pattern
2. **Xử lý thống kê** → Strategy Pattern
3. **Aggregate data** → Aggregation Pattern
4. **Hiển thị real-time** → Observer Pattern
5. **Export báo cáo** → Template Method Pattern

---

## 🏗️ KIẾN TRÚC TỔNG THỂ

### **Layered Architecture thực tế:**
```
┌─────────────────────────────────────┐
│           Presentation Layer        │
│        (Controllers + Views)        │
├─────────────────────────────────────┤
│           Business Layer            │
│         (Services + Logic)          │
├─────────────────────────────────────┤
│           Data Access Layer         │
│      (DbContext + Repository)       │
├─────────────────────────────────────┤
│           Database Layer            │
│           (SQL Server)              │
└─────────────────────────────────────┘
```

### **Dependency Injection thực tế:**
```csharp
// Program.cs - Dòng 1-90
builder.Services.AddScoped<CinemaDbContext>();
builder.Services.AddScoped<BankingService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IChatLogService, ChatLogService>();
builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
```

---

## 🔧 CÁC LỆNH VÀ HÀM QUAN TRỌNG

### **Entity Framework Commands thực tế:**
```bash
# Tạo migration
dotnet ef migrations add AddNewFeature

# Cập nhật database
dotnet ef database update

# Xóa migration cuối
dotnet ef migrations remove
```

### **LINQ Queries thực tế:**
```csharp
// Include related data
var lichChieu = await _context.LichChieus
    .Include(lc => lc.Phim)
    .Include(lc => lc.PhongChieu)
    .FirstOrDefaultAsync(lc => lc.MaLichChieu == maLichChieu);

// Group by và aggregate
var revenueByDate = await _context.Ves
    .Where(v => v.HanSuDung.HasValue)
    .GroupBy(v => v.HanSuDung.Value.Date)
    .Select(g => new { Date = g.Key, Revenue = g.Sum(v => v.Gia) })
    .ToListAsync();

// Count và Any
var hasTickets = await _context.Ves
    .AnyAsync(v => v.MaLichChieu == maLichChieu);
```

### **JavaScript Functions thực tế:**
```javascript
// Real-time updates
function updateSeatStatus(seatId, isSelected) {
    const seat = document.getElementById(seatId);
    seat.classList.toggle('selected', isSelected);
    
    // Observer pattern: Thông báo cho các component khác
    hubConnection.invoke("UpdateSeatStatus", seatId, isSelected);
}

// Form validation
function validatePaymentForm() {
    const amount = document.getElementById('amount').value;
    const accountNumber = document.getElementById('accountNumber').value;
    
    if (!amount || !accountNumber) {
        showError('Vui lòng điền đầy đủ thông tin');
        return false;
    }
    
    return true;
}
```

---

## 📈 HIỆU SUẤT VÀ TỐI ƯU HÓA

### **Caching Strategy thực tế:**
- **Memory Cache:** Dashboard data, user sessions
- **Database Query Optimization:** Include, Where clauses
- **Real-time Updates:** SignalR for live data

### **Security Measures thực tế:**
- **JWT Authentication:** Secure API endpoints
- **Two-Factor Authentication:** Enhanced security
- **Input Validation:** Prevent SQL injection
- **HTTPS:** Encrypted communication

### **Error Handling thực tế:**
```csharp
try
{
    // Business logic
}
catch (SqlException ex)
{
    _logger.LogError(ex, "Database error");
    return Json(new { success = false, message = "Lỗi cơ sở dữ liệu" });
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error");
    return Json(new { success = false, message = "Lỗi hệ thống" });
}
```

---

## 🎯 KẾT LUẬN

Hệ thống quản lý rạp chiếu phim được xây dựng với kiến trúc MVC hiện đại, áp dụng nhiều design patterns để đảm bảo:

1. **Tính mở rộng:** Dễ dàng thêm tính năng mới
2. **Tính bảo trì:** Code có cấu trúc rõ ràng
3. **Tính hiệu suất:** Caching và tối ưu hóa database
4. **Tính bảo mật:** Authentication và validation
5. **Tính real-time:** SignalR cho trải nghiệm người dùng tốt

Các design patterns được sử dụng giúp code dễ hiểu, dễ test và dễ maintain, đáp ứng các nguyên tắc SOLID và clean architecture. 