using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class KhachHang
    {
        [Key]
        [StringLength(10)]
        public string? MaKhachHang { get; set; }

        [StringLength(100)]
        public string? HoTen { get; set; }

        [StringLength(15)]
        public string? SDT { get; set; }

        public int DiemTichLuy { get; set; }

        // Navigation properties
        public virtual ICollection<TaiKhoan> TaiKhoans { get; set; } = new List<TaiKhoan>();
        public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
        public virtual ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
    }
}
