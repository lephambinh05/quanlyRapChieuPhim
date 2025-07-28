using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.Models
{
    public class ChatRoom
    {
        [Key]
        public string? RoomId { get; set; }
        
        [Required]
        [StringLength(100)]
        public string? RoomName { get; set; }
        
        [StringLength(20)]
        public string? RoomType { get; set; } // "support", "internal", "private"
        
        [StringLength(10)]
        public string? CustomerId { get; set; } // Nếu là chat hỗ trợ khách hàng
        
        [StringLength(10)]
        public string? StaffId { get; set; } // Nhân viên phụ trách
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime LastActivity { get; set; } = DateTime.Now;
        
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public virtual ICollection<ChatMessage>? Messages { get; set; }
    }
} 