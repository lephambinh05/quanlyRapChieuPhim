using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class LichChieu
    {
        [Key]
        [StringLength(10)]
        public string MaLichChieu { get; set; } = string.Empty;

        public DateTime ThoiGianBatDau { get; set; }

        public DateTime ThoiGianKetThuc { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Gia { get; set; }

        public string? MaPhong { get; set; }

        public string? MaPhim { get; set; }

        public string? MaNhanVien { get; set; }

        // Navigation properties
        [ForeignKey("MaPhong")]
        public virtual PhongChieu PhongChieu { get; set; } = null!;

        [ForeignKey("MaPhim")]
        public virtual Phim Phim { get; set; } = null!;

        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;

        public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
    }
}
