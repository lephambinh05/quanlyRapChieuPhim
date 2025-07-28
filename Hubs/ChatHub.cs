using Microsoft.AspNetCore.SignalR;
using CinemaManagement.Data;
using CinemaManagement.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using CinemaManagement.Services;

namespace CinemaManagement.Hubs
{
    public class ChatHub : Hub
    {
        private readonly CinemaDbContext _context;
        private readonly ILogger<ChatHub> _logger;
        private readonly IChatLogService _chatLogService;
        private static readonly Dictionary<string, string> _userConnections = new();

        public ChatHub(CinemaDbContext context, ILogger<ChatHub> logger, IChatLogService chatLogService)
        {
            _context = context;
            _logger = logger;
            _chatLogService = chatLogService;
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                // Lấy thông tin user từ session thay vì claims
                var httpContext = Context.GetHttpContext();
                var userId = httpContext?.Session.GetString("MaKhachHang") ?? httpContext?.Session.GetString("MaNhanVien");
                var userName = httpContext?.Session.GetString("TenKhachHang") ?? httpContext?.Session.GetString("TenNhanVien");
                var userRole = httpContext?.Session.GetString("VaiTro");
                
                _logger.LogInformation($"User connecting: {userId}, Name: {userName}, Role: {userRole}");
                
                if (!string.IsNullOrEmpty(userId))
                {
                    _userConnections[userId] = Context.ConnectionId;
                    
                    // Log connection
                    _chatLogService.LogConnection(userId, userName ?? "Unknown", userRole ?? "Unknown");
                    
                    // Thêm user vào group theo role
                    if (userRole == "Khách hàng")
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "customers");
                        _logger.LogInformation($"User {userId} added to customers group");
                    }
                    else if (userRole == "Nhân viên" || userRole == "Quản lý")
                    {
                        await Groups.AddToGroupAsync(Context.ConnectionId, "staff");
                        _logger.LogInformation($"User {userId} added to staff group");
                    }
                }
                else
                {
                    _logger.LogWarning("User connecting without session data");
                }

                await base.OnConnectedAsync();
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("OnConnectedAsync", ex);
                await base.OnConnectedAsync();
            }
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userId = httpContext?.Session.GetString("MaKhachHang") ?? httpContext?.Session.GetString("MaNhanVien");
                if (!string.IsNullOrEmpty(userId))
                {
                    _userConnections.Remove(userId);
                    _chatLogService.LogDisconnection(userId);
                    _logger.LogInformation($"User {userId} disconnected");
                }

                if (exception != null)
                {
                    _chatLogService.LogError("OnDisconnectedAsync", exception);
                    _logger.LogError(exception, "Error during disconnect");
                }

                await base.OnDisconnectedAsync(exception);
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("OnDisconnectedAsync", ex);
                await base.OnDisconnectedAsync(ex);
            }
        }

        // Gửi tin nhắn đến phòng chat cụ thể
        public async Task SendMessage(string roomId, string message, string? messageType = null)
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userId = httpContext?.Session.GetString("MaKhachHang") ?? httpContext?.Session.GetString("MaNhanVien");
                var userName = httpContext?.Session.GetString("TenKhachHang") ?? httpContext?.Session.GetString("TenNhanVien");
                var userRole = httpContext?.Session.GetString("VaiTro");

                _logger.LogInformation($"SendMessage called by {userId} ({userName}) in room {roomId}");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userName))
                {
                    _logger.LogWarning("User not logged in, sending error");
                    await Clients.Caller.SendAsync("ReceiveError", "Bạn chưa đăng nhập");
                    return;
                }

                // Log message
                _chatLogService.LogMessage(roomId, userId, userName, message);

                // Lưu tin nhắn vào database
                var chatMessage = new ChatMessage
                {
                    Content = message,
                    SenderId = userId,
                    SenderName = userName,
                    SenderRole = userRole ?? "Khách hàng",
                    MessageType = messageType ?? "text",
                    RoomId = roomId,
                    Timestamp = DateTime.Now
                };

                _context.ChatMessages.Add(chatMessage);
                await _context.SaveChangesAsync();
                _chatLogService.LogDatabaseOperation("SaveMessage", true, $"Message ID: {chatMessage.Id}");
                _logger.LogInformation($"Message saved to database with ID: {chatMessage.Id}");

                // Cập nhật thời gian hoạt động của phòng chat
                var chatRoom = await _context.ChatRooms.FindAsync(roomId);
                if (chatRoom != null)
                {
                    chatRoom.LastActivity = DateTime.Now;
                    await _context.SaveChangesAsync();
                }

                // Gửi tin nhắn đến tất cả user trong phòng
                await Clients.Group(roomId).SendAsync("ReceiveMessage", new
                {
                    id = chatMessage.Id,
                    content = chatMessage.Content,
                    senderId = chatMessage.SenderId,
                    senderName = chatMessage.SenderName,
                    senderRole = chatMessage.SenderRole,
                    messageType = chatMessage.MessageType,
                    timestamp = chatMessage.Timestamp,
                    roomId = chatMessage.RoomId
                });

                _chatLogService.LogSignalROperation("SendMessage", true, $"Sent to group {roomId}");
                _logger.LogInformation($"Message sent to group {roomId}");
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("SendMessage", ex);
                _logger.LogError(ex, "Error in SendMessage");
                await Clients.Caller.SendAsync("ReceiveError", "Có lỗi xảy ra khi gửi tin nhắn");
            }
        }

        // Tham gia phòng chat
        public async Task JoinRoom(string roomId)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
                await Clients.Caller.SendAsync("JoinedRoom", roomId);
                _chatLogService.LogSignalROperation("JoinRoom", true, $"User joined room: {roomId}");
                _logger.LogInformation($"User joined room: {roomId}");
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("JoinRoom", ex);
                _logger.LogError(ex, "Error in JoinRoom");
            }
        }

        // Rời phòng chat
        public async Task LeaveRoom(string roomId)
        {
            try
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, roomId);
                await Clients.Caller.SendAsync("LeftRoom", roomId);
                _chatLogService.LogSignalROperation("LeaveRoom", true, $"User left room: {roomId}");
                _logger.LogInformation($"User left room: {roomId}");
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("LeaveRoom", ex);
                _logger.LogError(ex, "Error in LeaveRoom");
            }
        }

        // Gửi tin nhắn hệ thống
        public async Task SendSystemMessage(string roomId, string message)
        {
            try
            {
                var systemMessage = new ChatMessage
                {
                    Content = message,
                    SenderId = "SYSTEM",
                    SenderName = "Hệ thống",
                    SenderRole = "System",
                    MessageType = "system",
                    RoomId = roomId,
                    Timestamp = DateTime.Now
                };

                _context.ChatMessages.Add(systemMessage);
                await _context.SaveChangesAsync();
                _chatLogService.LogDatabaseOperation("SaveSystemMessage", true, $"System message saved");

                await Clients.Group(roomId).SendAsync("ReceiveSystemMessage", new
                {
                    content = systemMessage.Content,
                    timestamp = systemMessage.Timestamp
                });

                _chatLogService.LogSignalROperation("SendSystemMessage", true, $"System message sent to room {roomId}");
                _logger.LogInformation($"System message sent to room {roomId}: {message}");
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("SendSystemMessage", ex);
                _logger.LogError(ex, "Error in SendSystemMessage");
            }
        }

        // Typing indicator
        public async Task Typing(string roomId, bool isTyping)
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userName = httpContext?.Session.GetString("TenKhachHang") ?? httpContext?.Session.GetString("TenNhanVien");
                await Clients.Group(roomId).SendAsync("UserTyping", userName, isTyping);
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("Typing", ex);
                _logger.LogError(ex, "Error in Typing");
            }
        }

        // Đánh dấu tin nhắn đã đọc
        public async Task MarkAsRead(string roomId)
        {
            try
            {
                var httpContext = Context.GetHttpContext();
                var userId = httpContext?.Session.GetString("MaKhachHang") ?? httpContext?.Session.GetString("MaNhanVien");
                if (!string.IsNullOrEmpty(userId))
                {
                    var unreadMessages = await _context.ChatMessages
                        .Where(m => m.RoomId == roomId && !m.IsRead && m.SenderId != userId)
                        .ToListAsync();

                    foreach (var message in unreadMessages)
                    {
                        message.IsRead = true;
                    }

                    await _context.SaveChangesAsync();
                    await Clients.Group(roomId).SendAsync("MessagesMarkedAsRead", roomId);
                    _chatLogService.LogDatabaseOperation("MarkAsRead", true, $"Marked {unreadMessages.Count} messages as read");
                    _logger.LogInformation($"Messages marked as read in room {roomId} by user {userId}");
                }
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("MarkAsRead", ex);
                _logger.LogError(ex, "Error in MarkAsRead");
            }
        }

        // Lấy danh sách tin nhắn của phòng
        public async Task GetRoomMessages(string roomId, int? limit = null)
        {
            try
            {
                _logger.LogInformation($"GetRoomMessages called for room: {roomId}, limit: {limit}");
                
                // Kiểm tra kết nối database
                var canConnect = await _context.Database.CanConnectAsync();
                _logger.LogInformation($"Database connection test: {canConnect}");
                
                if (!canConnect)
                {
                    _chatLogService.LogDatabaseOperation("GetRoomMessages", false, "Database connection failed");
                    _logger.LogError("Database connection failed");
                    await Clients.Caller.SendAsync("ReceiveError", "Không thể kết nối database");
                    return;
                }

                // Kiểm tra xem bảng ChatMessages có tồn tại không
                try
                {
                    var tableExists = await _context.ChatMessages.AnyAsync();
                    _logger.LogInformation($"ChatMessages table exists: {tableExists}");
                }
                catch (Exception tableEx)
                {
                    _logger.LogError(tableEx, "ChatMessages table does not exist or is not accessible");
                    await Clients.Caller.SendAsync("ReceiveError", "Bảng chat chưa được tạo. Vui lòng chạy script SQL.");
                    return;
                }

                // Kiểm tra xem có tin nhắn nào trong phòng không
                var messageCount = await _context.ChatMessages
                    .Where(m => m.RoomId == roomId)
                    .CountAsync();
                
                _logger.LogInformation($"Found {messageCount} messages in room {roomId}");

                var actualLimit = limit ?? 50;
                var messages = await _context.ChatMessages
                    .Where(m => m.RoomId == roomId)
                    .OrderByDescending(m => m.Timestamp)
                    .Take(actualLimit)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {messages.Count} messages for room {roomId}");
                
                // Gửi tin nhắn về client
                await Clients.Caller.SendAsync("ReceiveRoomMessages", roomId, messages);
                
                _chatLogService.LogDatabaseOperation("GetRoomMessages", true, $"Retrieved {messages.Count} messages for room {roomId}");
                _logger.LogInformation($"Successfully sent {messages.Count} messages to client for room {roomId}");
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("GetRoomMessages", ex);
                _logger.LogError(ex, $"Error in GetRoomMessages for room {roomId}: {ex.Message}");
                await Clients.Caller.SendAsync("ReceiveError", $"Lỗi khi tải tin nhắn: {ex.Message}");
            }
        }

        // Tạo phòng chat mới
        public async Task CreateRoom(string roomName, string roomType, string? customerId = null, string? staffId = null)
        {
            try
            {
                var roomId = Guid.NewGuid().ToString();
                var chatRoom = new ChatRoom
                {
                    RoomId = roomId,
                    RoomName = roomName,
                    RoomType = roomType,
                    CustomerId = customerId,
                    StaffId = staffId,
                    CreatedAt = DateTime.Now,
                    LastActivity = DateTime.Now,
                    IsActive = true
                };

                _context.ChatRooms.Add(chatRoom);
                await _context.SaveChangesAsync();
                _chatLogService.LogDatabaseOperation("CreateRoom", true, $"Created room: {roomId} - {roomName}");

                await Clients.All.SendAsync("RoomCreated", new
                {
                    roomId = chatRoom.RoomId,
                    roomName = chatRoom.RoomName,
                    roomType = chatRoom.RoomType,
                    customerId = chatRoom.CustomerId,
                    staffId = chatRoom.StaffId
                });

                _chatLogService.LogSignalROperation("CreateRoom", true, $"Room created: {roomId}");
                _logger.LogInformation($"New room created: {roomId} - {roomName}");
            }
            catch (Exception ex)
            {
                _chatLogService.LogError("CreateRoom", ex);
                _logger.LogError(ex, "Error in CreateRoom");
            }
        }
    }
} 