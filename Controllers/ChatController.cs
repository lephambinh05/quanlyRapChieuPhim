using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using CinemaManagement.Data;
using Microsoft.Extensions.Logging;

namespace CinemaManagement.Controllers
{
    public class ChatController : Controller
    {
        private readonly CinemaDbContext _context;
        private readonly ILogger<ChatController> _logger;

        public ChatController(CinemaDbContext context, ILogger<ChatController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Trang chat cho khách hàng
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("MaKhachHang");
            var userName = HttpContext.Session.GetString("TenKhachHang");
            var userRole = HttpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(userId) || userRole != "Khách hàng")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Tìm hoặc tạo phòng chat hỗ trợ cho khách hàng này
            var supportRoom = await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.CustomerId == userId && r.RoomType == "support");

            if (supportRoom == null)
            {
                supportRoom = new ChatRoom
                {
                    RoomId = Guid.NewGuid().ToString(),
                    RoomName = $"Hỗ trợ {userName}",
                    RoomType = "support",
                    CustomerId = userId,
                    CreatedAt = DateTime.Now,
                    LastActivity = DateTime.Now,
                    IsActive = true
                };
                _context.ChatRooms.Add(supportRoom);
                await _context.SaveChangesAsync();
            }

            var messages = await _context.ChatMessages
                .Where(m => m.RoomId == supportRoom.RoomId)
                .OrderBy(m => m.Timestamp)
                .Take(50)
                .ToListAsync();

            var viewModel = new ChatViewModel
            {
                CurrentRoomId = supportRoom.RoomId,
                CurrentRoomName = supportRoom.RoomName,
                Messages = messages,
                UserId = userId,
                UserName = userName,
                UserRole = userRole,
                IsOnline = true
            };

            return View(viewModel);
        }

        // Trang quản lý chat cho nhân viên/quản lý
        public async Task<IActionResult> Manage()
        {
            var userId = HttpContext.Session.GetString("MaNhanVien");
            var userRole = HttpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(userId) || (userRole != "Nhân viên" && userRole != "Quản lý"))
            {
                return RedirectToAction("Login", "Auth");
            }

            var rooms = await _context.ChatRooms
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.LastActivity)
                .ToListAsync();

            var viewModel = new ChatViewModel
            {
                AvailableRooms = rooms,
                UserId = userId,
                UserRole = userRole
            };

            return View(viewModel);
        }

        // Trang chat nội bộ cho nhân viên
        public async Task<IActionResult> Internal()
        {
            var userId = HttpContext.Session.GetString("MaNhanVien");
            var userRole = HttpContext.Session.GetString("VaiTro");

            if (string.IsNullOrEmpty(userId) || (userRole != "Nhân viên" && userRole != "Quản lý"))
            {
                return RedirectToAction("Login", "Auth");
            }

            // Tìm hoặc tạo phòng chat nội bộ
            var internalRoom = await _context.ChatRooms
                .FirstOrDefaultAsync(r => r.RoomType == "internal");

            if (internalRoom == null)
            {
                internalRoom = new ChatRoom
                {
                    RoomId = Guid.NewGuid().ToString(),
                    RoomName = "Chat nội bộ",
                    RoomType = "internal",
                    CreatedAt = DateTime.Now,
                    LastActivity = DateTime.Now,
                    IsActive = true
                };
                _context.ChatRooms.Add(internalRoom);
                await _context.SaveChangesAsync();
            }

            var messages = await _context.ChatMessages
                .Where(m => m.RoomId == internalRoom.RoomId)
                .OrderBy(m => m.Timestamp)
                .Take(50)
                .ToListAsync();

            var viewModel = new ChatViewModel
            {
                CurrentRoomId = internalRoom.RoomId,
                CurrentRoomName = internalRoom.RoomName,
                Messages = messages,
                UserId = userId,
                UserRole = userRole
            };

            return View(viewModel);
        }

        // API để lấy tin nhắn của phòng
        [HttpGet]
        public async Task<IActionResult> GetMessages(string roomId)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.RoomId == roomId)
                .OrderBy(m => m.Timestamp)
                .Take(50)
                .ToListAsync();

            return Json(messages);
        }

        // API để lấy danh sách phòng chat
        [HttpGet]
        public async Task<IActionResult> GetRooms()
        {
            var rooms = await _context.ChatRooms
                .Where(r => r.IsActive)
                .OrderByDescending(r => r.LastActivity)
                .ToListAsync();

            var roomViewModels = new List<ChatRoomViewModel>();
            
            foreach (var room in rooms)
            {
                string? customerName = null;
                string? staffName = null;
                
                // Lấy tên khách hàng nếu có
                if (!string.IsNullOrEmpty(room.CustomerId))
                {
                    var khachHang = await _context.KhachHangs
                        .FirstOrDefaultAsync(kh => kh.maKhachHang == room.CustomerId);
                    customerName = khachHang?.HoTen;
                }
                
                // Lấy tên nhân viên nếu có
                if (!string.IsNullOrEmpty(room.StaffId))
                {
                    var nhanVien = await _context.NhanViens
                        .FirstOrDefaultAsync(nv => nv.maNhanVien == room.StaffId);
                    staffName = nhanVien?.TenNhanVien;
                }
                
                roomViewModels.Add(new ChatRoomViewModel
                {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    RoomType = room.RoomType,
                    CustomerId = room.CustomerId,
                    CustomerName = customerName,
                    StaffId = room.StaffId,
                    StaffName = staffName,
                    CreatedAt = room.CreatedAt,
                    LastActivity = room.LastActivity,
                    IsActive = room.IsActive
                });
            }

            return Json(roomViewModels);
        }

        // API để tạo phòng chat mới
        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] CreateRoomViewModel model)
        {
            if (string.IsNullOrEmpty(model.RoomName))
            {
                return BadRequest("Tên phòng không được để trống");
            }

            var roomId = Guid.NewGuid().ToString();
            var chatRoom = new ChatRoom
            {
                RoomId = roomId,
                RoomName = model.RoomName,
                RoomType = model.RoomType ?? "private",
                CustomerId = model.CustomerId,
                StaffId = model.StaffId,
                CreatedAt = DateTime.Now,
                LastActivity = DateTime.Now,
                IsActive = true
            };

            _context.ChatRooms.Add(chatRoom);
            await _context.SaveChangesAsync();

            return Json(new { success = true, roomId = roomId });
        }

        // API để đánh dấu tin nhắn đã đọc
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(string roomId)
        {
            var userId = HttpContext.Session.GetString("MaNhanVien");
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var unreadMessages = await _context.ChatMessages
                .Where(m => m.RoomId == roomId && !m.IsRead && m.SenderId != userId)
                .ToListAsync();

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, count = unreadMessages.Count });
        }

        // API để lấy số tin nhắn chưa đọc
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = HttpContext.Session.GetString("MaNhanVien");
            
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var unreadCount = await _context.ChatMessages
                .Where(m => !m.IsRead && m.SenderId != userId)
                .CountAsync();

            return Json(new { count = unreadCount });
        }

        public IActionResult Demo()
        {
            return View();
        }

        // Test connection page
        public IActionResult TestConnection()
        {
            return View();
        }

        // API để test session data
        [HttpGet]
        public IActionResult GetSessionInfo()
        {
            var userId = HttpContext.Session.GetString("MaKhachHang") ?? HttpContext.Session.GetString("MaNhanVien");
            var userName = HttpContext.Session.GetString("TenKhachHang") ?? HttpContext.Session.GetString("TenNhanVien");
            var userRole = HttpContext.Session.GetString("VaiTro");

            return Json(new
            {
                userId = userId,
                userName = userName,
                userRole = userRole,
                isLoggedIn = !string.IsNullOrEmpty(userId)
            });
        }

        // API để test database connection
        [HttpGet]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                // Test kết nối database
                var roomCount = await _context.ChatRooms.CountAsync();
                var messageCount = await _context.ChatMessages.CountAsync();

                return Json(new
                {
                    success = true,
                    message = "Database connection successful",
                    roomCount = roomCount,
                    messageCount = messageCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    error = ex.ToString()
                });
            }
        }

        // API để kiểm tra và tạo bảng chat
        [HttpGet]
        public async Task<IActionResult> EnsureChatTables()
        {
            try
            {
                // Kiểm tra kết nối database
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return Json(new { success = false, message = "Không thể kết nối database" });
                }

                // Kiểm tra xem bảng có tồn tại không
                var roomCount = await _context.ChatRooms.CountAsync();
                var messageCount = await _context.ChatMessages.CountAsync();

                return Json(new
                {
                    success = true,
                    message = "Database connection successful",
                    roomCount = roomCount,
                    messageCount = messageCount,
                    tablesExist = true
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message,
                    error = ex.ToString(),
                    tablesExist = false
                });
            }
        }

        [HttpGet]
        public IActionResult TestLogging()
        {
            try
            {
                _logger.LogInformation("🧪 Test logging from ChatController");
                _logger.LogWarning("⚠️ Test warning message");
                _logger.LogError("❌ Test error message");
                
                return Json(new { 
                    success = true, 
                    message = "Logging test completed",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in TestLogging");
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }

        [HttpPost]
        public IActionResult AddTestLog(string message, string level = "Information")
        {
            try
            {
                switch (level.ToLower())
                {
                    case "error":
                        _logger.LogError("🧪 TEST LOG: {Message}", message);
                        break;
                    case "warning":
                        _logger.LogWarning("🧪 TEST LOG: {Message}", message);
                        break;
                    case "debug":
                        _logger.LogDebug("🧪 TEST LOG: {Message}", message);
                        break;
                    default:
                        _logger.LogInformation("🧪 TEST LOG: {Message}", message);
                        break;
                }
                
                return Json(new { 
                    success = true, 
                    message = $"Log added with level: {level}",
                    timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AddTestLog");
                return Json(new { 
                    success = false, 
                    error = ex.Message 
                });
            }
        }
    }
} 

