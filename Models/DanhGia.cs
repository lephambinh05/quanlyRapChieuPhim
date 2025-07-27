using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class DanhGia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string MaKhachHang { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string MaPhim { get; set; } = string.Empty;

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int SoSao { get; set; }

        [StringLength(1000)]
        public string? NoiDungBinhLuan { get; set; }

        [Required]
        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        public bool DaXemPhim { get; set; } = false;

        // Navigation properties
        [ForeignKey("MaKhachHang")]
        public virtual KhachHang KhachHang { get; set; } = null!;

        [ForeignKey("MaPhim")]
        public virtual Phim Phim { get; set; } = null!;
    }
} 