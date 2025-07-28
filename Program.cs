using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using CinemaManagement.Services;
using CinemaManagement.Hubs;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình Serilog
// Log.Logger = new LoggerConfiguration()
//     .MinimumLevel.Information()
//     .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
//     .MinimumLevel.Override("Microsoft.AspNetCore.SignalR", LogEventLevel.Debug)
//     .MinimumLevel.Override("CinemaManagement.Hubs", LogEventLevel.Debug)
//     .MinimumLevel.Override("CinemaManagement.Controllers", LogEventLevel.Debug)
//     .WriteTo.Console()
//     .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt"), 
//         fileSizeLimitBytes: 10485760,
//         retainedFileCountLimit: 5,
//         outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
//     .CreateLogger();

// builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add SignalR
builder.Services.AddSignalR();

// Add Memory Cache
builder.Services.AddMemoryCache();

// Add Antiforgery
builder.Services.AddAntiforgery(options =>
{
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly = true;
});

// Add Entity Framework
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Data Protection
builder.Services.AddDataProtection();

// Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8); // Tăng từ 2 giờ lên 8 giờ
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.MaxAge = TimeSpan.FromHours(8); // Tăng từ 2 giờ lên 8 giờ
});

// Add Email and Password Reset Services
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordResetService, PasswordResetService>();
builder.Services.AddScoped<ITwoFactorService, TwoFactorService>();
builder.Services.AddScoped<IChatLogService, ChatLogService>();
builder.Services.AddScoped<BankingService>();

// Add Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // Tăng từ 2 giờ lên 8 giờ
    options.SlidingExpiration = true;
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    options.CallbackPath = "/signin-google";
    options.SaveTokens = true;
    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.None;
    options.CorrelationCookie.HttpOnly = true;
    options.CorrelationCookie.MaxAge = TimeSpan.FromHours(1);
    
    // Thêm logging chi tiết
    options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
    {
        OnRemoteFailure = context =>
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_ERROR] RemoteFailure: {context.Failure?.Message}\n";
            System.IO.File.AppendAllText(logPath, logLine);
            return Task.CompletedTask;
        },
        OnTicketReceived = context =>
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_DEBUG] TicketReceived\n";
            System.IO.File.AppendAllText(logPath, logLine);
            return Task.CompletedTask;
        },
        OnCreatingTicket = context =>
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_DEBUG] CreatingTicket\n";
            System.IO.File.AppendAllText(logPath, logLine);
            return Task.CompletedTask;
        },
        OnRedirectToAuthorizationEndpoint = context =>
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_DEBUG] RedirectToAuthorizationEndpoint: {context.RedirectUri}\n";
            System.IO.File.AppendAllText(logPath, logLine);
            context.Response.Redirect(context.RedirectUri);
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();
// Tạo khách hàng GUEST nếu chưa tồn tại
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
    try
    {
        // Tạo nhân viên GUEST
        var guestEmployee = await context.NhanViens.FindAsync("GUEST");
        if (guestEmployee == null)
        {
            context.NhanViens.Add(new CinemaManagement.Models.NhanVien
            {
                maNhanVien = "GUEST",
                TenNhanVien = "Hệ thống",
                ChucVu = "Tự động",
                SDT = "0000000000",
                NgaySinh = DateTime.Now
            });
            await context.SaveChangesAsync();
            Console.WriteLine("Đã tạo nhân viên GUEST cho hệ thống");
        }

        // Tạo khách hàng GUEST
        var guestCustomer = await context.KhachHangs.FindAsync("GUEST");
        if (guestCustomer == null)
        {
            context.KhachHangs.Add(new CinemaManagement.Models.KhachHang
            {
                maKhachHang = "GUEST",
                HoTen = "Khách lẻ",
                SDT = "0000000000",
                DiemTichLuy = 0
            });
            await context.SaveChangesAsync();
            Console.WriteLine("Đã tạo khách hàng GUEST cho khách lẻ");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi khi tạo dữ liệu GUEST: {ex.Message}");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

// Map SignalR Hub
app.MapHub<ChatHub>("/chatHub");

// XÓA hoặc comment dòng: app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Add Google authentication callback handler
app.MapGet("/signin-google", async (HttpContext context) =>
{
    try
    {
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
        var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_DEBUG] MapGet /signin-google được gọi\n";
        System.IO.File.AppendAllText(logPath, logLine);
        
        // Redirect to Auth controller để xử lý
        context.Response.Redirect("/Auth/ExternalLoginCallback");
    }
    catch (Exception ex)
    {
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
        var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_ERROR] MapGet Exception: {ex.Message}\n";
        System.IO.File.AppendAllText(logPath, logLine);
        
        context.Response.Redirect("/Auth/Login?error=google_auth_error");
    }
});

// Đảm bảo thư mục logs tồn tại
var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logsDirectory))
{
    Directory.CreateDirectory(logsDirectory);
}

try
{
    Log.Information("Starting Cinema Management Application");
    
    // Tự động mở trình duyệt khi khởi động (chỉ trong Development)
    if (app.Environment.IsDevelopment())
    {
        var urls = app.Urls.ToList();
        if (urls.Any())
        {
            var url = urls.First();
            if (url.StartsWith("https://"))
            {
                try
                {
                    // Chờ một chút để ứng dụng khởi động hoàn toàn
                    await Task.Delay(2000);
                    
                    // Mở trình duyệt với URL của ứng dụng
                    var processStartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(processStartInfo);
                    
                    Console.WriteLine($"✅ Đã tự động mở trình duyệt: {url}");
                    Log.Information($"Auto-opened browser: {url}");
                }
                catch (Exception browserEx)
                {
                    Console.WriteLine($"⚠️ Không thể mở trình duyệt: {browserEx.Message}");
                    Log.Warning($"Failed to open browser: {browserEx.Message}");
                }
            }
        }
    }
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

