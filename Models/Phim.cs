using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class Phim
    {
        [Key]
        [StringLength(10)]
        public string? MaPhim { get; set; }

        [StringLength(255)]
        public string? TenPhim { get; set; }

        [StringLength(100)]
        public string? TheLoai { get; set; }

        public int ThoiLuong { get; set; } // đơn vị phút

        [StringLength(10)]
        public string? DoTuoiPhanAnh { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? MoTa { get; set; }

        [StringLength(255)]
        public string? ViTriFilePhim { get; set; }

        [StringLength(10)]
        public string? MaNhanVien { get; set; }

        // Navigation properties
        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;
        public virtual ICollection<LichChieu> LichChieus { get; set; } = new List<LichChieu>();
        public virtual ICollection<Ve> Ves { get; set; } = new List<Ve>();
    }
}
