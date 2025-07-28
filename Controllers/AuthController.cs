using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using CinemaManagement.Services;
using CinemaManagement.ViewModels;

namespace CinemaManagement.Controllers
{
    public class AuthController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly IEmailService _emailService;
        private readonly IPasswordResetService _passwordResetService;
        private readonly ITwoFactorService _twoFactorService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(CinemaDbContext context, IEmailService emailService, IPasswordResetService passwordResetService, ITwoFactorService twoFactorService, ILogger<AuthController> logger)
        {
            _context = context;
            _emailService = emailService;
            _passwordResetService = passwordResetService;
            _twoFactorService = twoFactorService;
            _logger = logger;
        }

        // Helper method để kiểm tra trạng thái đăng nhập và chuyển hướng phù hợp
        private IActionResult GetRedirectBasedOnLoginStatus()
        {
            var maNhanVien = HttpContext.Session.GetString("MaNhanVien");
            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            var role = HttpContext.Session.GetString("Role");

            if (!string.IsNullOrEmpty(maNhanVien))
            {
                if (role == "Quản lý")
                {
                    return RedirectToAction("Index", "QuanLy");
                }
                else if (role == "Nhân viên")
                {
                    return RedirectToAction("Index", "BanVe");
                }
            }
            else if (!string.IsNullOrEmpty(maKhachHang))
            {
                return RedirectToAction("Index", "KhachHang");
            }

            // Nếu chưa đăng nhập, chuyển về trang login
            return RedirectToAction("Login");
        }

        // GET: Auth/Login
        public IActionResult Login()
        {
            return View();
        }

        // GET: Auth/Home - Xử lý khi bấm vào logo
        public IActionResult Home()
        {
            return GetRedirectBasedOnLoginStatus();
        }

        // GET: Auth/GoogleLogin
        public IActionResult GoogleLogin()
        {
            _logger.LogInformation("[GOOGLE_DEBUG] Bắt đầu GoogleLogin");

            var properties = new AuthenticationProperties
            {
                RedirectUri = "http://localhost:5039/signin-google"
            };

            _logger.LogInformation("[GOOGLE_DEBUG] RedirectUri: {RedirectUri}", properties.RedirectUri);
            
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        // GET: Auth/GoogleCallback - Redirect to signin-google
        public IActionResult GoogleCallback()
        {
            return Redirect("/signin-google");
        }

        // GET: Auth/ExternalLoginCallback
        public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            _logger.LogInformation("[GOOGLE_DEBUG] Bắt đầu ExternalLoginCallback");

            if (remoteError != null)
            {
                _logger.LogError("[GOOGLE_ERROR] RemoteError: {RemoteError}", remoteError);
                ViewBag.ErrorMessage = $"Lỗi từ provider: {remoteError}";
                return RedirectToAction("Login");
            }

            try
            {
                _logger.LogInformation("[GOOGLE_DEBUG] Đang authenticate với Google...");

                var info = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
                
                if (!info.Succeeded)
                {
                    _logger.LogError("[GOOGLE_ERROR] AuthenticateAsync thất bại");
                    ViewBag.ErrorMessage = "Đăng nhập Google thất bại";
                    return RedirectToAction("Login");
                }

                _logger.LogInformation("[GOOGLE_DEBUG] AuthenticateAsync thành công");

                var claims = info.Principal.Claims;
                var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation("[GOOGLE_DEBUG] Email: {Email}, Name: {Name}, GoogleId: {GoogleId}", email, name, googleId);

                if (string.IsNullOrEmpty(email))
                {
                    _logger.LogError("[GOOGLE_ERROR] Không thể lấy email từ Google");
                    ViewBag.ErrorMessage = "Không thể lấy thông tin email từ Google";
                    return RedirectToAction("Login");
                }

                _logger.LogInformation("[GOOGLE_DEBUG] Đang tìm tài khoản trong database...");

                var existingAccount = await _context.TaiKhoans
                    .Include(t => t.NhanVien)
                    .Include(t => t.KhachHang)
                    .FirstOrDefaultAsync(t => t.Email == email);

                if (existingAccount != null)
                {
                    _logger.LogInformation("[GOOGLE_DEBUG] Tìm thấy tài khoản cũ: {MaTK}", existingAccount.MaTK);
                    return await LoginWithGoogleAccount(existingAccount);
                }
                else
                {
                    _logger.LogInformation("[GOOGLE_DEBUG] Không tìm thấy tài khoản, sẽ tạo mới");
                    return await CreateGoogleAccount(email, name, googleId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[GOOGLE_EXCEPTION] Exception: {Message}", ex.Message);
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi xử lý đăng nhập Google";
                return RedirectToAction("Login");
            }
        }

        private async Task<IActionResult> LoginWithGoogleAccount(TaiKhoan taiKhoan)
        {
            if (taiKhoan.TrangThai != "Hoạt động")
            {
                ViewBag.ErrorMessage = "Tài khoản đã bị khóa";
                return RedirectToAction("Login");
            }

            // Lưu thông tin vào session
            HttpContext.Session.SetString("MaTK", taiKhoan.MaTK);
            HttpContext.Session.SetString("Email", taiKhoan.Email);
            HttpContext.Session.SetString("Role", taiKhoan.Role);
            HttpContext.Session.SetString("VaiTro", taiKhoan.Role);

            // Ghi log đăng nhập
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_LOGIN] Email: {taiKhoan.Email}, Role: {taiKhoan.Role}\n";
            System.IO.File.AppendAllText(logPath, logLine);

            if (taiKhoan.NhanVien != null)
            {
                HttpContext.Session.SetString("MaNhanVien", taiKhoan.NhanVien.maNhanVien);
                HttpContext.Session.SetString("TenNhanVien", taiKhoan.NhanVien.TenNhanVien);
                HttpContext.Session.SetString("ChucVu", taiKhoan.NhanVien.ChucVu);
            }

            if (taiKhoan.KhachHang != null)
            {
                HttpContext.Session.SetString("MaKhachHang", taiKhoan.KhachHang.maKhachHang);
                HttpContext.Session.SetString("TenKhachHang", taiKhoan.KhachHang.HoTen);
            }

            // Redirect based on role
            if (taiKhoan.Role == "Quản lý")
            {
                return RedirectToAction("Index", "QuanLy");
            }
            else if (taiKhoan.Role == "Nhân viên")
            {
                return RedirectToAction("Index", "BanVe");
            }
            else if (taiKhoan.Role == "Khách hàng")
            {
                return RedirectToAction("Index", "KhachHang");
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        private async Task<IActionResult> CreateGoogleAccount(string email, string name, string googleId)
        {
            try
            {
                // Ghi log bắt đầu tạo tài khoản
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
                var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_CREATE] Bắt đầu tạo tài khoản cho email: {email}, name: {name}\n";
                System.IO.File.AppendAllText(logPath, logLine);

                // Tạo mã khách hàng mới
                var lastCustomer = await _context.KhachHangs.OrderByDescending(k => k.maKhachHang).FirstOrDefaultAsync();
                var newmaKhachHang = "KH001";
                if (lastCustomer != null)
                {
                    var lastNumber = int.Parse(lastCustomer.maKhachHang.Substring(2));
                    newmaKhachHang = $"KH{(lastNumber + 1):D3}";
                }

                // Tạo khách hàng mới
                var khachHang = new KhachHang
                {
                    maKhachHang = newmaKhachHang,
                    HoTen = name ?? "Khách hàng Google",
                    SDT = "0000000000", // Mặc định
                    DiemTichLuy = 0
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                // Ghi log đã tạo khách hàng
                logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_CREATE] Đã tạo khách hàng: {newmaKhachHang}\n";
                System.IO.File.AppendAllText(logPath, logLine);

                // Tạo mã tài khoản mới
                var lastAccount = await _context.TaiKhoans.OrderByDescending(t => t.MaTK).FirstOrDefaultAsync();
                var newMaTK = "TK001";
                if (lastAccount != null)
                {
                    var lastNumber = int.Parse(lastAccount.MaTK.Substring(2));
                    newMaTK = $"TK{(lastNumber + 1):D3}";
                }

                // Tạo tài khoản mới
                var taiKhoan = new TaiKhoan
                {
                    MaTK = newMaTK,
                    Email = email,
                    MatKhau = $"google_{googleId}", // Mật khẩu đặc biệt cho Google
                    Role = "Khách hàng",
                    TrangThai = "Hoạt động",
                    maKhachHang = newmaKhachHang,
                    maNhanVien = null
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                // Ghi log tạo tài khoản
                logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_CREATE] Đã tạo tài khoản: {newMaTK} cho email: {email}, Name: {name}\n";
                System.IO.File.AppendAllText(logPath, logLine);

                // Đăng nhập với tài khoản mới
                return await LoginWithGoogleAccount(taiKhoan);
            }
            catch (Exception ex)
            {
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
                var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [GOOGLE_CREATE_ERROR] {ex.Message}\n";
                System.IO.File.AppendAllText(logPath, logLine);

                ViewBag.ErrorMessage = "Có lỗi xảy ra khi tạo tài khoản Google";
                return RedirectToAction("Login");
            }
        }

        // POST: Auth/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            var taiKhoan = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .Include(t => t.KhachHang)
                .FirstOrDefaultAsync(t => t.Email == email && t.MatKhau == password);

            if (taiKhoan == null || taiKhoan.TrangThai != "Hoạt động")
            {
                ViewBag.ErrorMessage = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // Kiểm tra 2FA
            if (taiKhoan.TwoFactorEnabled)
            {
                // Lưu thông tin tạm thời vào session
                HttpContext.Session.SetString("UserEmail", taiKhoan.Email);
                HttpContext.Session.SetString("UserRole", taiKhoan.Role);
                HttpContext.Session.SetString("TempMaTK", taiKhoan.MaTK);
                HttpContext.Session.SetString("TempVaiTro", taiKhoan.Role);

                if (taiKhoan.NhanVien != null)
                {
                    HttpContext.Session.SetString("TempMaNhanVien", taiKhoan.NhanVien.maNhanVien);
                    HttpContext.Session.SetString("TempTenNhanVien", taiKhoan.NhanVien.TenNhanVien);
                    HttpContext.Session.SetString("TempChucVu", taiKhoan.NhanVien.ChucVu);
                }

                if (taiKhoan.KhachHang != null)
                {
                    HttpContext.Session.SetString("TempMaKhachHang", taiKhoan.KhachHang.maKhachHang);
                    HttpContext.Session.SetString("TempTenKhachHang", taiKhoan.KhachHang.HoTen);
                }

                // Chuyển hướng đến trang xác thực 2FA
                return RedirectToAction("TwoFactorVerify");
            }

            // Lưu thông tin vào session (nếu không có 2FA)
            HttpContext.Session.SetString("MaTK", taiKhoan.MaTK);
            HttpContext.Session.SetString("Email", taiKhoan.Email);
            HttpContext.Session.SetString("Role", taiKhoan.Role);
            HttpContext.Session.SetString("VaiTro", taiKhoan.Role);
            HttpContext.Session.SetString("UserEmail", taiKhoan.Email);
            HttpContext.Session.SetString("UserRole", taiKhoan.Role);

            // Ghi log đăng nhập
            _logger.LogInformation("[LOGIN] Email: {Email}, Role: {Role}", taiKhoan.Email, taiKhoan.Role);

            if (taiKhoan.NhanVien != null)
            {
                HttpContext.Session.SetString("MaNhanVien", taiKhoan.NhanVien.maNhanVien);
                HttpContext.Session.SetString("TenNhanVien", taiKhoan.NhanVien.TenNhanVien);
                HttpContext.Session.SetString("ChucVu", taiKhoan.NhanVien.ChucVu);
            }

            if (taiKhoan.KhachHang != null)
            {
                HttpContext.Session.SetString("MaKhachHang", taiKhoan.KhachHang.maKhachHang);
                HttpContext.Session.SetString("TenKhachHang", taiKhoan.KhachHang.HoTen);
            }

            // Redirect based on role
            if (taiKhoan.Role == "Quản lý")
            {
                return RedirectToAction("Index", "QuanLy"); // Chuyển đến dashboard quản lý
            }
            else if (taiKhoan.Role == "Nhân viên")
            {
                return RedirectToAction("Index", "BanVe"); // Nhân viên chỉ đi đến bán vé
            }
            else if (taiKhoan.Role == "Khách hàng")
            {
                return RedirectToAction("Index", "KhachHang"); // Khách hàng đi đến trang khách hàng
            }
            else
            {
                return RedirectToAction("Index", "Home"); // Fallback
            }
        }

        // GET: Auth/Logout
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // GET: Auth/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Auth/Register
        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword, string hoTen, string sdt)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword) || string.IsNullOrEmpty(hoTen) || string.IsNullOrEmpty(sdt))
            {
                ViewBag.ErrorMessage = "Vui lòng nhập đầy đủ thông tin";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.ErrorMessage = "Mật khẩu xác nhận không khớp";
                return View();
            }

            if (password.Length < 6)
            {
                ViewBag.ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự";
                return View();
            }

            // Kiểm tra email đã tồn tại
            var existingAccount = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == email);
            if (existingAccount != null)
            {
                ViewBag.ErrorMessage = "Email đã được sử dụng";
                return View();
            }

            try
            {
                // Tạo mã khách hàng mới
                var lastCustomer = await _context.KhachHangs.OrderByDescending(k => k.maKhachHang).FirstOrDefaultAsync();
                var newmaKhachHang = "KH001";
                if (lastCustomer != null)
                {
                    var lastNumber = int.Parse(lastCustomer.maKhachHang.Substring(2));
                    newmaKhachHang = $"KH{(lastNumber + 1):D3}";
                }

                // Tạo khách hàng mới
                var khachHang = new KhachHang
                {
                    maKhachHang = newmaKhachHang,
                    HoTen = hoTen,
                    SDT = sdt,
                    DiemTichLuy = 0
                };

                _context.KhachHangs.Add(khachHang);
                await _context.SaveChangesAsync();

                // Tạo mã tài khoản mới
                var lastAccount = await _context.TaiKhoans.OrderByDescending(t => t.MaTK).FirstOrDefaultAsync();
                var newMaTK = "TK001";
                if (lastAccount != null)
                {
                    var lastNumber = int.Parse(lastAccount.MaTK.Substring(2));
                    newMaTK = $"TK{(lastNumber + 1):D3}";
                }

                // Tạo tài khoản mới
                var taiKhoan = new TaiKhoan
                {
                    MaTK = newMaTK,
                    Email = email,
                    MatKhau = password, // Trong thực tế nên hash password
                    Role = "Khách hàng",
                    TrangThai = "Hoạt động",
                    maKhachHang = newmaKhachHang,
                    maNhanVien = null
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                ViewBag.SuccessMessage = "Đăng ký thành công! Bạn có thể đăng nhập ngay bây giờ.";
                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Có lỗi xảy ra khi đăng ký: {ex.Message}";
                return View();
            }
        }

        // GET: Auth/TestAccounts - Debug purpose only
        public async Task<IActionResult> TestAccounts()
        {
            var accounts = await _context.TaiKhoans
                .Include(t => t.NhanVien)
                .Include(t => t.KhachHang)
                .ToListAsync();
            
            return Json(accounts.Select(a => new {
                MaTK = a.MaTK,
                Email = a.Email,
                MatKhau = a.MatKhau,
                Role = a.Role,
                TrangThai = a.TrangThai,
                TenNhanVien = a.NhanVien?.TenNhanVien,
                TenKhachHang = a.KhachHang?.HoTen
            }));
        }

        // GET: Auth/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: Auth/ForgotPassword
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra email có tồn tại trong hệ thống không
                var taiKhoan = await _context.TaiKhoans
                    .Include(t => t.KhachHang)
                    .Include(t => t.NhanVien)
                    .FirstOrDefaultAsync(t => t.Email == model.Email);

                if (taiKhoan == null)
                {
                    // Không cho biết email có tồn tại hay không để bảo mật
                    ViewBag.SuccessMessage = "Nếu email tồn tại trong hệ thống, bạn sẽ nhận được email hướng dẫn đặt lại mật khẩu.";
                    return View();
                }

                // Tạo token reset password
                var token = await _passwordResetService.GenerateResetTokenAsync(model.Email);

                // Tạo link reset password
                var resetLink = Url.Action("ResetPassword", "Auth", new { email = model.Email, token = token }, Request.Scheme, Request.Host.Value);

                // Lấy tên người dùng
                var userName = taiKhoan.KhachHang?.HoTen ?? taiKhoan.NhanVien?.TenNhanVien ?? "Người dùng";

                // Gửi email
                var emailSent = await _emailService.SendPasswordResetEmailAsync(model.Email, resetLink, userName);

                if (emailSent)
                {
                    ViewBag.SuccessMessage = "Email hướng dẫn đặt lại mật khẩu đã được gửi. Vui lòng kiểm tra hộp thư của bạn.";
                }
                else
                {
                    ViewBag.ErrorMessage = "Có lỗi xảy ra khi gửi email. Vui lòng thử lại sau.";
                }

                return View();
            }
            catch (Exception ex)
            {
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
                var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [FORGOT_PASSWORD_ERROR] {ex.Message}\n";
                System.IO.File.AppendAllText(logPath, logLine);

                ViewBag.ErrorMessage = "Có lỗi xảy ra. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        // GET: Auth/ResetPassword
        public async Task<IActionResult> ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                ViewBag.ErrorMessage = "Link không hợp lệ.";
                return RedirectToAction("Login");
            }

            // Kiểm tra token có hợp lệ không
            var isValidToken = await _passwordResetService.ValidateResetTokenAsync(email, token);
            if (!isValidToken)
            {
                ViewBag.ErrorMessage = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordConfirmModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        // POST: Auth/ResetPassword
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordConfirmModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                // Kiểm tra token có hợp lệ không
                var isValidToken = await _passwordResetService.ValidateResetTokenAsync(model.Email, model.Token);
                if (!isValidToken)
                {
                    ViewBag.ErrorMessage = "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn.";
                    return RedirectToAction("Login");
                }

                // Tìm tài khoản
                var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == model.Email);
                if (taiKhoan == null)
                {
                    ViewBag.ErrorMessage = "Tài khoản không tồn tại.";
                    return RedirectToAction("Login");
                }

                // Cập nhật mật khẩu
                taiKhoan.MatKhau = model.NewPassword;
                await _context.SaveChangesAsync();

                // Đánh dấu token đã được sử dụng
                await _passwordResetService.UseResetTokenAsync(model.Email, model.Token);

                // Gửi email thông báo thành công
                var userName = taiKhoan.KhachHang?.HoTen ?? taiKhoan.NhanVien?.TenNhanVien ?? "Người dùng";
                await _emailService.SendPasswordResetSuccessEmailAsync(model.Email, userName);

                ViewBag.SuccessMessage = "Mật khẩu đã được đặt lại thành công. Bạn có thể đăng nhập với mật khẩu mới.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
                var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [RESET_PASSWORD_ERROR] {ex.Message}\n";
                System.IO.File.AppendAllText(logPath, logLine);

                ViewBag.ErrorMessage = "Có lỗi xảy ra khi đặt lại mật khẩu. Vui lòng thử lại sau.";
                return View(model);
            }
        }

        // 2FA Actions
        // GET: Auth/TwoFactorSetup
        public async Task<IActionResult> TwoFactorSetup()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == userEmail);
            if (taiKhoan == null)
            {
                return RedirectToAction("Login");
            }

            if (taiKhoan.TwoFactorEnabled)
            {
                return RedirectToAction("TwoFactorStatus");
            }

            var secret = await _twoFactorService.GenerateSecretAsync();
            var qrCodeImage = await _twoFactorService.GenerateQrCodeAsync(userEmail, secret);

            var model = new TwoFactorSetupViewModel
            {
                Secret = secret,
                QrCodeImage = qrCodeImage,
                Email = userEmail,
                ManualEntryKey = secret
            };

            return View(model);
        }

        // POST: Auth/TwoFactorEnable
        [HttpPost]
        public async Task<IActionResult> TwoFactorEnable(TwoFactorEnableViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("TwoFactorSetup");
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == userEmail);
            if (taiKhoan == null)
            {
                return RedirectToAction("Login");
            }

            var isValidCode = await _twoFactorService.ValidateCodeAsync(model.Secret, model.Code);
            if (!isValidCode)
            {
                ViewBag.ErrorMessage = "Mã xác thực không đúng. Vui lòng thử lại.";
                return RedirectToAction("TwoFactorSetup");
            }

            // Kích hoạt 2FA
            taiKhoan.TwoFactorSecret = model.Secret;
            taiKhoan.TwoFactorEnabled = true;
            taiKhoan.TwoFactorVerified = true;
            taiKhoan.TwoFactorSetupDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Xác thực hai yếu tố đã được kích hoạt thành công!";
            return RedirectToAction("TwoFactorStatus");
        }

        // GET: Auth/TwoFactorVerify
        public IActionResult TwoFactorVerify(string returnUrl = null)
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var model = new TwoFactorVerifyViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        // POST: Auth/TwoFactorVerify
        [HttpPost]
        public async Task<IActionResult> TwoFactorVerify(TwoFactorVerifyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == userEmail);
            if (taiKhoan == null)
            {
                return RedirectToAction("Login");
            }

            var isValidCode = await _twoFactorService.ValidateCodeAsync(taiKhoan.TwoFactorSecret, model.Code);

            if (!isValidCode)
            {
                ViewBag.ErrorMessage = "Mã xác thực không đúng. Vui lòng thử lại.";
                return View(model);
            }

            // Đánh dấu đã xác thực 2FA
            HttpContext.Session.SetString("TwoFactorVerified", "true");

            // Lưu thông tin chính thức vào session
            var tempMaTK = HttpContext.Session.GetString("TempMaTK");
            var tempVaiTro = HttpContext.Session.GetString("TempVaiTro");
            var tempmaNhanVien = HttpContext.Session.GetString("TempMaNhanVien");
            var tempTenNhanVien = HttpContext.Session.GetString("TempTenNhanVien");
            var tempChucVu = HttpContext.Session.GetString("TempChucVu");
            var tempmaKhachHang = HttpContext.Session.GetString("TempMaKhachHang");
            var tempTenKhachHang = HttpContext.Session.GetString("TempTenKhachHang");

            // Lưu thông tin cơ bản
            HttpContext.Session.SetString("MaTK", tempMaTK ?? "");
            HttpContext.Session.SetString("Email", userEmail);
            HttpContext.Session.SetString("Role", tempVaiTro ?? "");
            HttpContext.Session.SetString("VaiTro", tempVaiTro ?? "");
            HttpContext.Session.SetString("UserEmail", userEmail);
            HttpContext.Session.SetString("UserRole", tempVaiTro ?? "");

            if (!string.IsNullOrEmpty(tempmaNhanVien))
            {
                HttpContext.Session.SetString("MaNhanVien", tempmaNhanVien);
                HttpContext.Session.SetString("TenNhanVien", tempTenNhanVien);
                HttpContext.Session.SetString("ChucVu", tempChucVu);
            }

            if (!string.IsNullOrEmpty(tempmaKhachHang))
            {
                HttpContext.Session.SetString("MaKhachHang", tempmaKhachHang);
                HttpContext.Session.SetString("TenKhachHang", tempTenKhachHang);
            }

            // Xóa session tạm thời
            HttpContext.Session.Remove("TempMaTK");
            HttpContext.Session.Remove("TempVaiTro");
            HttpContext.Session.Remove("TempMaNhanVien");
            HttpContext.Session.Remove("TempTenNhanVien");
            HttpContext.Session.Remove("TempChucVu");
            HttpContext.Session.Remove("TempMaKhachHang");
            HttpContext.Session.Remove("TempTenKhachHang");

            if (!string.IsNullOrEmpty(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            // Chuyển hướng dựa trên role
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole == "Quản lý")
            {
                return RedirectToAction("Index", "QuanLy");
            }
            else if (userRole == "Nhân viên")
            {
                return RedirectToAction("Index", "BanVe");
            }
            else
            {
                return RedirectToAction("Index", "KhachHang");
            }
        }

        // GET: Auth/TwoFactorStatus
        public async Task<IActionResult> TwoFactorStatus()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == userEmail);
            if (taiKhoan == null)
            {
                return RedirectToAction("Login");
            }

            var model = new TwoFactorStatusViewModel
            {
                IsEnabled = taiKhoan.TwoFactorEnabled,
                IsVerified = taiKhoan.TwoFactorVerified,
                SetupDate = taiKhoan.TwoFactorSetupDate
            };

            return View(model);
        }

        // GET: Auth/TwoFactorDisable
        public IActionResult TwoFactorDisable()
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            return View(new TwoFactorDisableViewModel());
        }

        // POST: Auth/TwoFactorDisable
        [HttpPost]
        public async Task<IActionResult> TwoFactorDisable(TwoFactorDisableViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return RedirectToAction("Login");
            }

            var taiKhoan = await _context.TaiKhoans.FirstOrDefaultAsync(t => t.Email == userEmail);
            if (taiKhoan == null)
            {
                return RedirectToAction("Login");
            }

            var isValidCode = await _twoFactorService.ValidateCodeAsync(taiKhoan.TwoFactorSecret, model.Code);
            if (!isValidCode)
            {
                ViewBag.ErrorMessage = "Mã xác thực không đúng. Vui lòng thử lại.";
                return View(model);
            }

            // Tắt 2FA
            taiKhoan.TwoFactorEnabled = false;
            taiKhoan.TwoFactorVerified = false;
            taiKhoan.TwoFactorSecret = null;
            taiKhoan.TwoFactorSetupDate = null;

            await _context.SaveChangesAsync();

            ViewBag.SuccessMessage = "Xác thực hai yếu tố đã được tắt thành công!";
            return RedirectToAction("TwoFactorStatus");
        }

        // TwoFactorBackupCodes action đã được xóa
    }
}

