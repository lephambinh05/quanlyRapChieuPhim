using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class TaiKhoan
    {
        [Key]
        [StringLength(10)]
        public string? MaTK { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? MatKhau { get; set; }

        [StringLength(50)]
        public string? Role { get; set; }

        [StringLength(20)]
        public string? TrangThai { get; set; }

        [StringLength(10)]
        public string? MaNhanVien { get; set; }

        [StringLength(10)]
        public string? MaKhachHang { get; set; }

        // Navigation properties
        public virtual NhanVien? NhanVien { get; set; }
        public virtual KhachHang? KhachHang { get; set; }
    }
}
