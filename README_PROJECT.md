# 🎬 HỆ THỐNG QUẢN LÝ RẠP CHIẾU PHIM - CINEMA MANAGEMENT SYSTEM

## 📋 TỔNG QUAN SẢN PHẨM

**Cinema Management System** là một ứng dụng web hiện đại được xây dựng trên nền tảng **ASP.NET Core MVC** và **Entity Framework Core**, cung cấp giải pháp toàn diện cho việc quản lý rạp chiếu phim. Hệ thống tích hợp đầy đủ các chức năng từ đặt vé trực tuyến, quản lý phòng chiếu, thanh toán đa phương thức đến báo cáo thống kê chuyên sâu.

### 🎯 **Mục tiêu chính:**
- Tối ưu hóa trải nghiệm khách hàng với giao diện hiện đại, responsive
- Tự động hóa quy trình bán vé và quản lý rạp chiếu
- Tích hợp hệ thống thanh toán ngân hàng tự động
- Cung cấp công cụ quản lý mạnh mẽ cho nhân viên và quản lý
- Hỗ trợ chat realtime cho customer service

### 🏗️ **Kiến trúc hệ thống:**
- **Frontend**: ASP.NET Core MVC với Razor Views, Bootstrap 5, JavaScript
- **Backend**: ASP.NET Core Web API, Entity Framework Core
- **Database**: SQL Server với Code-First approach
- **Realtime**: SignalR cho chat và notifications
- **Payment**: Tích hợp chuyển khoản ngân hàng với QR code

---

## 🚀 CHỨC NĂNG CHÍNH

### 1. **🎫 Đặt vé trực tuyến (Customer Module)**

#### **Chức năng đặt vé:**
- **Xem danh sách phim**: Hiển thị phim với poster, thông tin chi tiết, trailer
- **Chọn suất chiếu**: Lịch chiếu theo ngày, phòng, thời gian
- **Chọn ghế**: Sơ đồ ghế động với trạng thái realtime (trống, đã đặt, đang chọn, hỏng)
- **Giỏ hàng tạm**: Lưu vé đã chọn trong session hoặc database tạm
- **Áp dụng voucher**: Tự động kiểm tra điều kiện, thời gian hiệu lực

#### **Design Pattern - Repository Pattern:**
```csharp
// Controllers/KhachHangController.cs
public class KhachHangController : Controller
{
    private readonly CinemaDbContext _context;
    
    // List phim với pagination và filtering
    public async Task<IActionResult> Index(string searchString, string genre, int? page)
    {
        var phims = from p in _context.Phims select p;
        
        if (!String.IsNullOrEmpty(searchString))
            phims = phims.Where(s => s.tenPhim.Contains(searchString));
            
        if (!String.IsNullOrEmpty(genre))
            phims = phims.Where(x => x.theLoai == genre);
            
        int pageSize = 6;
        return View(await PaginatedList<Phim>.CreateAsync(phims, page ?? 1, pageSize));
    }
}
```

#### **Sơ đồ ghế động:**
```javascript
// Views/KhachHang/ChonGhe.cshtml
function updateSeatStatus(seatId, status) {
    const seat = document.getElementById(seatId);
    seat.className = `seat ${status}`;
    seat.onclick = status === 'available' ? () => selectSeat(seatId) : null;
}

// Real-time seat updates via SignalR
connection.on("SeatStatusChanged", function (seatId, status) {
    updateSeatStatus(seatId, status);
});
```

### 2. **💳 Thanh toán chuyển khoản ngân hàng**

#### **Quy trình thanh toán:**
- **Tạo hóa đơn**: Lưu trạng thái "Chờ chuyển khoản"
- **Sinh QR code**: Sử dụng VietQR.io API
- **Hướng dẫn chi tiết**: Thông tin ngân hàng, nội dung chuyển khoản
- **Tự động xác thực**: Cron job kiểm tra giao dịch qua API bên thứ 3
- **Cập nhật trạng thái**: Tự động chuyển sang "Đã thanh toán"

#### **Design Pattern - Strategy Pattern:**
```csharp
// Services/PaymentService.cs
public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(Order order);
    Task<bool> VerifyPaymentAsync(string transactionId);
}

public class BankTransferService : IPaymentService
{
    public async Task<PaymentResult> ProcessPaymentAsync(Order order)
    {
        // Tạo QR code cho chuyển khoản
        var qrCode = await GenerateQRCodeAsync(order);
        
        // Lưu thông tin thanh toán
        order.PaymentMethod = "BankTransfer";
        order.Status = "Pending";
        
        return new PaymentResult { QRCode = qrCode, OrderId = order.Id };
    }
}
```

#### **Cron job xác thực thanh toán:**
```csharp
// Controllers/CronController.cs
[HttpGet("/api/cron/check-banking")]
public async Task<IActionResult> CheckBankingTransactions()
{
    var pendingOrders = await _context.HoaDons
        .Where(h => h.trangThai == "Chờ chuyển khoản")
        .ToListAsync();
        
    foreach (var order in pendingOrders)
    {
        // Gọi API bên thứ 3 để kiểm tra giao dịch
        var transaction = await _bankingApiService.CheckTransactionAsync(order.maHoaDon);
        
        if (transaction != null && transaction.Amount == order.tongTien)
        {
            order.trangThai = "Đã thanh toán";
            await _context.SaveChangesAsync();
            
            // Gửi thông báo cho khách hàng
            await _notificationService.SendPaymentSuccessAsync(order);
        }
    }
    
    return Json(new { success = true, processed = pendingOrders.Count });
}
```

### 3. **👥 Quản lý nhân viên (Staff Module)**

#### **Chức năng bán vé tại quầy:**
- **Chọn phim và suất chiếu**: Interface tương tự customer nhưng với quyền admin
- **Xác nhận vé**: Quét mã QR, kiểm tra trạng thái vé
- **In hóa đơn**: Xuất PDF hóa đơn với thông tin chi tiết
- **Báo cáo ca làm việc**: Thống kê doanh thu, số vé bán

#### **Design Pattern - Command Pattern:**
```csharp
// Commands/SellTicketCommand.cs
public class SellTicketCommand : ICommand
{
    public string CustomerId { get; set; }
    public string ShowTimeId { get; set; }
    public List<string> SeatIds { get; set; }
    public string VoucherCode { get; set; }
}

public class SellTicketCommandHandler : ICommandHandler<SellTicketCommand>
{
    public async Task<CommandResult> HandleAsync(SellTicketCommand command)
    {
        // Validate seats availability
        var seats = await _context.GheNgois
            .Where(g => command.SeatIds.Contains(g.maGhe))
            .ToListAsync();
            
        if (seats.Any(s => s.trangThai != "Trống"))
            return CommandResult.Failure("Một số ghế đã được đặt");
            
        // Create tickets
        var tickets = command.SeatIds.Select(seatId => new Ve
        {
            maVe = GenerateTicketId(),
            soGhe = seatId,
            trangThai = "Đã bán",
            // ... other properties
        }).ToList();
        
        // Apply voucher if provided
        if (!string.IsNullOrEmpty(command.VoucherCode))
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.maVoucher == command.VoucherCode);
            // Apply discount logic
        }
        
        await _context.Ves.AddRangeAsync(tickets);
        await _context.SaveChangesAsync();
        
        return CommandResult.Success(tickets);
    }
}
```

### 4. **🏢 Quản lý hệ thống (Manager Module)**

#### **Quản lý phim:**
- **CRUD operations**: Thêm, sửa, xóa phim với validation
- **Upload media**: Poster, trailer, file phim
- **Phân loại**: Thể loại, độ tuổi, thời lượng
- **Thống kê**: Phim bán chạy, doanh thu theo phim

#### **Design Pattern - Factory Pattern:**
```csharp
// Factories/MovieFactory.cs
public interface IMovieFactory
{
    Movie CreateMovie(MovieCreateDto dto);
}

public class MovieFactory : IMovieFactory
{
    public Movie CreateMovie(MovieCreateDto dto)
    {
        return new Phim
        {
            maPhim = GenerateMovieId(),
            tenPhim = dto.Title,
            theLoai = dto.Genre,
            thoiLuong = dto.Duration,
            doTuoiPhanAnh = dto.AgeRating,
            moTa = dto.Description,
            viTriFilePhim = await _fileService.UploadMovieFileAsync(dto.MovieFile),
            maNhanVien = dto.ManagerId
        };
    }
}

// Controllers/QuanLyController.cs
[HttpPost]
public async Task<IActionResult> CreateMovie([FromForm] MovieCreateDto dto)
{
    if (!ModelState.IsValid)
        return View(dto);
        
    var movie = _movieFactory.CreateMovie(dto);
    await _context.Phims.AddAsync(movie);
    await _context.SaveChangesAsync();
    
    return RedirectToAction(nameof(MovieList));
}
```

#### **Quản lý lịch chiếu:**
```csharp
// Services/ScheduleService.cs
public class ScheduleService : IScheduleService
{
    public async Task<ValidationResult> ValidateScheduleAsync(LichChieu schedule)
    {
        // Kiểm tra xung đột thời gian
        var conflicts = await _context.LichChieus
            .Where(l => l.maPhong == schedule.maPhong)
            .Where(l => (l.thoiGianBatDau <= schedule.thoiGianBatDau && 
                        l.thoiGianKetThuc > schedule.thoiGianBatDau) ||
                       (l.thoiGianBatDau < schedule.thoiGianKetThuc && 
                        l.thoiGianKetThuc >= schedule.thoiGianKetThuc))
            .ToListAsync();
            
        if (conflicts.Any())
            return ValidationResult.Failure("Xung đột lịch chiếu");
            
        return ValidationResult.Success();
    }
}
```

### 5. **🗨️ Hệ thống Chat Realtime**

#### **SignalR Hub Implementation:**
```csharp
// Hubs/ChatHub.cs
public class ChatHub : Hub
{
    private readonly CinemaDbContext _context;
    
    public async Task SendMessage(string content, string roomId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var userRole = Context.User?.FindFirst(ClaimTypes.Role)?.Value;
        
        var message = new ChatMessage
        {
            Content = content,
            RoomId = roomId,
            SenderId = userId,
            SenderRole = userRole,
            Timestamp = DateTime.Now
        };
        
        await _context.ChatMessages.AddAsync(message);
        await _context.SaveChangesAsync();
        
        // Broadcast to all clients in the room
        await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
    }
    
    public async Task JoinRoom(string roomId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        await Clients.Group(roomId).SendAsync("UserJoined", Context.User?.Identity?.Name);
    }
}
```

#### **Client-side JavaScript:**
```javascript
// Views/Chat/Index.cshtml
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/chatHub")
    .build();

connection.on("ReceiveMessage", function (message) {
    addMessage(message);
});

connection.on("UserTyping", function (userName) {
    showTypingIndicator(userName);
});

async function sendMessage() {
    const content = document.getElementById("message-input").value;
    await connection.invoke("SendMessage", content, currentRoomId);
    document.getElementById("message-input").value = "";
}
```

### 6. **📊 Báo cáo và thống kê**

#### **Dashboard với biểu đồ:**
```csharp
// ViewModels/DashboardViewModel.cs
public class DashboardViewModel
{
    public decimal TotalRevenue { get; set; }
    public int TotalTickets { get; set; }
    public int TotalMovies { get; set; }
    public List<ChartData> RevenueChart { get; set; }
    public List<TopMovie> TopMovies { get; set; }
}

// Controllers/QuanLyController.cs
public async Task<IActionResult> Dashboard()
{
    var today = DateTime.Today;
    var startOfMonth = new DateTime(today.Year, today.Month, 1);
    
    var viewModel = new DashboardViewModel
    {
        TotalRevenue = await _context.HoaDons
            .Where(h => h.thoiGianTao >= startOfMonth)
            .SumAsync(h => h.tongTien),
            
        TotalTickets = await _context.Ves
            .Where(v => v.trangThai == "Đã bán")
            .CountAsync(),
            
        RevenueChart = await _context.HoaDons
            .Where(h => h.thoiGianTao >= startOfMonth)
            .GroupBy(h => h.thoiGianTao.Date)
            .Select(g => new ChartData 
            { 
                Date = g.Key, 
                Revenue = g.Sum(h => h.tongTien) 
            })
            .ToListAsync()
    };
    
    return View(viewModel);
}
```

---

## 🏗️ KIẾN TRÚC VÀ DESIGN PATTERNS

### **1. MVC Pattern (Model-View-Controller)**
```csharp
// Model
public class Phim
{
    public string maPhim { get; set; }
    public string tenPhim { get; set; }
    public string theLoai { get; set; }
    // ... other properties
}

// Controller
public class KhachHangController : Controller
{
    public async Task<IActionResult> Index()
    {
        var phims = await _context.Phims.ToListAsync();
        return View(phims); // View
    }
}

// View (Razor)
@model List<Phim>
@foreach (var phim in Model)
{
    <div class="movie-card">
        <h3>@phim.tenPhim</h3>
        <p>@phim.theLoai</p>
    </div>
}
```

### **2. Repository Pattern**
```csharp
// Interfaces/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(string id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(string id);
}

// Repositories/PhimRepository.cs
public class PhimRepository : IRepository<Phim>
{
    private readonly CinemaDbContext _context;
    
    public PhimRepository(CinemaDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Phim>> GetAllAsync()
    {
        return await _context.Phims
            .Include(p => p.NhanVien)
            .ToListAsync();
    }
}
```

### **3. Unit of Work Pattern**
```csharp
// Interfaces/IUnitOfWork.cs
public interface IUnitOfWork : IDisposable
{
    IPhimRepository Phims { get; }
    IVeRepository Ves { get; }
    IHoaDonRepository HoaDons { get; }
    Task<int> SaveChangesAsync();
}

// Data/UnitOfWork.cs
public class UnitOfWork : IUnitOfWork
{
    private readonly CinemaDbContext _context;
    private IPhimRepository _phimRepository;
    
    public IPhimRepository Phims => 
        _phimRepository ??= new PhimRepository(_context);
    
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
```

### **4. Observer Pattern (SignalR)**
```csharp
// Hubs/ChatHub.cs
public class ChatHub : Hub
{
    // Observer pattern implementation
    public async Task SendMessage(string content, string roomId)
    {
        var message = new ChatMessage { Content = content, RoomId = roomId };
        
        // Notify all observers (clients) in the room
        await Clients.Group(roomId).SendAsync("ReceiveMessage", message);
    }
}
```

---

## 🗄️ DATABASE DESIGN

### **Entity Relationship Diagram:**
```
KhachHang (1) ←→ (N) HoaDon
NhanVien (1) ←→ (N) Phim
Phim (1) ←→ (N) LichChieu
PhongChieu (1) ←→ (N) GheNgoi
LichChieu (1) ←→ (N) Ve
HoaDon (1) ←→ (N) CTHD
ChatRoom (1) ←→ (N) ChatMessage
```

### **Code-First Approach:**
```csharp
// Data/CinemaDbContext.cs
public class CinemaDbContext : DbContext
{
    public DbSet<KhachHang> KhachHangs { get; set; }
    public DbSet<NhanVien> NhanViens { get; set; }
    public DbSet<Phim> Phims { get; set; }
    public DbSet<LichChieu> LichChieus { get; set; }
    public DbSet<Ve> Ves { get; set; }
    public DbSet<HoaDon> HoaDons { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<ChatRoom> ChatRooms { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure relationships
        modelBuilder.Entity<HoaDon>()
            .HasOne(h => h.KhachHang)
            .WithMany(k => k.HoaDons)
            .HasForeignKey(h => h.maKhachHang);
            
        // Configure table names
        modelBuilder.Entity<KhachHang>().ToTable("KhachHang");
        modelBuilder.Entity<ChatMessage>().ToTable("ChatMessage");
    }
}
```

---

## 🛡️ BẢO MẬT VÀ PHÂN QUYỀN

### **Authentication & Authorization:**
```csharp
// Controllers/AuthController.cs
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    var user = await _context.TaiKhoans
        .Include(t => t.KhachHang)
        .Include(t => t.NhanVien)
        .FirstOrDefaultAsync(t => t.Email == model.Email);
        
    if (user != null && VerifyPassword(model.Password, user.matKhau))
    {
        // Set session
        HttpContext.Session.SetString("UserId", user.maTK);
        HttpContext.Session.SetString("UserRole", user.role);
        
        return RedirectToAction("Index", "Home");
    }
    
    ModelState.AddModelError("", "Email hoặc mật khẩu không đúng");
    return View(model);
}
```

### **Role-based Authorization:**
```csharp
// Attributes/AuthorizeRolesAttribute.cs
public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }
}

// Usage in controllers
[AuthorizeRoles("Manager")]
public class QuanLyController : Controller
{
    // Only managers can access
}
```

---

## 🚀 DEPLOYMENT VÀ MAINTENANCE

### **Configuration:**
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=RapChieuPhim;Trusted_Connection=true;"
  },
  "SignalR": {
    "HubUrl": "/chatHub"
  },
  "Payment": {
    "BankApiUrl": "https://api.banking.com",
    "QRCodeApiUrl": "https://api.vietqr.io"
  }
}
```

### **Cron Jobs:**
```csharp
// Controllers/CronController.cs
[HttpGet("/api/cron/check-banking")]
public async Task<IActionResult> CheckBankingTransactions()
{
    // Automated payment verification
    // Runs every 2 minutes via external scheduler
}

[HttpGet("/api/cron/cancel-expired-orders")]
public async Task<IActionResult> CancelExpiredOrders()
{
    // Cancel orders older than 2 minutes
    // Free up seats for other customers
}
```

---

## 📈 PERFORMANCE OPTIMIZATION

### **Caching Strategy:**
```csharp
// Services/CacheService.cs
public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    
    public async Task<List<Phim>> GetMoviesAsync()
    {
        const string cacheKey = "movies_list";
        
        if (!_cache.TryGetValue(cacheKey, out List<Phim> movies))
        {
            movies = await _context.Phims.ToListAsync();
            _cache.Set(cacheKey, movies, TimeSpan.FromMinutes(10));
        }
        
        return movies;
    }
}
```

### **Database Optimization:**
```sql
-- Indexes for performance
CREATE INDEX IX_HoaDon_ThoiGianTao ON HoaDon(thoiGianTao);
CREATE INDEX IX_Ve_TrangThai ON Ve(trangThai);
CREATE INDEX IX_LichChieu_ThoiGianBatDau ON LichChieu(thoiGianBatDau);
```

---

## 🎯 KẾT LUẬN

**Cinema Management System** là một giải pháp toàn diện, hiện đại với:

- ✅ **Scalable Architecture**: MVC pattern với separation of concerns
- ✅ **Real-time Features**: SignalR cho chat và notifications
- ✅ **Secure Payment**: Tích hợp thanh toán ngân hàng tự động
- ✅ **Responsive Design**: Bootstrap 5 cho mobile-first approach
- ✅ **Comprehensive Reporting**: Dashboard với biểu đồ thống kê
- ✅ **Role-based Access**: Phân quyền rõ ràng cho từng vai trò

**Hệ thống sẵn sàng cho production và có thể mở rộng thêm các tính năng nâng cao như AI recommendation, mobile app, và multi-cinema management.** 🚀 