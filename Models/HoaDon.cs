using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class HoaDon
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? MaHoaDon { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TongTien { get; set; }

        public DateTime ThoiGianTao { get; set; }

        public int SoLuong { get; set; }

        public string? MaKhachHang { get; set; }

        public string? MaNhanVien { get; set; }

        public string? TrangThai { get; set; } = "Chờ chuyển khoản";

        // Navigation properties
        [ForeignKey("MaKhachHang")]
        public virtual KhachHang? KhachHang { get; set; }

        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;

        public virtual ICollection<CTHD> CTHDs { get; set; } = new List<CTHD>();
        public virtual ICollection<HDVoucher> HDVouchers { get; set; } = new List<HDVoucher>();
    }
}
