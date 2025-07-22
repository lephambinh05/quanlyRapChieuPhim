using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    [Table("TempGioHangItems")]
    public class TempGioHangItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(10)]
        public string MaHoaDon { get; set; } = string.Empty;
        [Required]
        [StringLength(10)]
        public string MaGhe { get; set; } = string.Empty;
        [Required]
        [StringLength(30)]
        public string SoGhe { get; set; } = string.Empty;
        [Required]
        public decimal Gia { get; set; }
        [Required]
        [StringLength(10)]
        public string MaLichChieu { get; set; } = string.Empty;
        [Required]
        [StringLength(10)]
        public string MaPhim { get; set; } = string.Empty;
        [Required]
        [StringLength(255)]
        public string TenPhim { get; set; } = string.Empty;
        [Required]
        [StringLength(10)]
        public string MaPhong { get; set; } = string.Empty;
        [Required]
        [StringLength(50)]
        public string TenPhong { get; set; } = string.Empty;
        [Required]
        public DateTime ThoiGianChieu { get; set; }
    }
} 