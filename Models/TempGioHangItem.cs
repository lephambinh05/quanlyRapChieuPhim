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
        public string? MaHoaDon { get; set; }
        public string? MaGhe { get; set; }
        public string? SoGhe { get; set; }
        [Required]
        public decimal Gia { get; set; }
        public string? MaLichChieu { get; set; }
        public string? MaPhim { get; set; }
        public string? TenPhim { get; set; }
        public string? MaPhong { get; set; }
        public string? TenPhong { get; set; }
        [Required]
        public DateTime ThoiGianChieu { get; set; }
    }
} 