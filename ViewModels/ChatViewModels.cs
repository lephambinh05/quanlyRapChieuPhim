using CinemaManagement.Models;

namespace CinemaManagement.ViewModels
{
    public class ChatViewModel
    {
        public string? CurrentRoomId { get; set; }
        public string? CurrentRoomName { get; set; }
        public List<ChatRoom>? AvailableRooms { get; set; }
        public List<ChatMessage>? Messages { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserRole { get; set; }
        public bool IsOnline { get; set; }
    }

    public class ChatRoomViewModel
    {
        public string? RoomId { get; set; }
        public string? RoomName { get; set; }
        public string? RoomType { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? StaffId { get; set; }
        public string? StaffName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsActive { get; set; }
        public int UnreadCount { get; set; }
        public ChatMessage? LastMessage { get; set; }
    }

    public class ChatMessageViewModel
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SenderId { get; set; }
        public string? SenderName { get; set; }
        public string? SenderRole { get; set; }
        public string? MessageType { get; set; }
        public string? RoomId { get; set; }
        public bool IsRead { get; set; }
        public bool IsOwnMessage { get; set; }
        public string? FormattedTime { get; set; }
    }

    public class CreateRoomViewModel
    {
        public string? RoomName { get; set; }
        public string? RoomType { get; set; }
        public string? CustomerId { get; set; }
        public string? StaffId { get; set; }
        public List<KhachHang>? AvailableCustomers { get; set; }
        public List<NhanVien>? AvailableStaff { get; set; }
    }

    public class ChatSupportViewModel
    {
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? RoomId { get; set; }
        public List<ChatMessage>? Messages { get; set; }
        public bool IsNewConversation { get; set; }
    }
} 