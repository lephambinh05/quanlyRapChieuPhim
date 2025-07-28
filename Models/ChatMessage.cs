using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models
{
    public class ChatMessage
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(1000)]
        public string? Content { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        [Required]
        [StringLength(10)]
        public string? SenderId { get; set; }
        
        [Required]
        [StringLength(50)]
        public string? SenderName { get; set; }
        
        [Required]
        [StringLength(20)]
        public string? SenderRole { get; set; } // "Khách hàng", "Nhân viên", "Quản lý"
        
        [StringLength(20)]
        public string? MessageType { get; set; } = "text"; // "text", "image", "file", "system"
        
        [StringLength(50)]
        public string? RoomId { get; set; } // Để phân biệt các phòng chat khác nhau
        
        public bool IsRead { get; set; } = false;
        
        // Không cần navigation properties vì đã xử lý thủ công trong controller
    }
} 