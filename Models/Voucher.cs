using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class Voucher
    {
        [Key]
        [StringLength(10)]
        public string? MaGiamGia { get; set; }

        [StringLength(100)]
        public string? TenGiamGia { get; set; }

        public int PhanTramGiam { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? MoTa { get; set; }

        public DateTime ThoiGianBatDau { get; set; }

        public DateTime ThoiGianKetThuc { get; set; }

        [StringLength(10)]
        public string? MaNhanVien { get; set; }

        // Navigation properties
        [ForeignKey("MaNhanVien")]
        public virtual NhanVien NhanVien { get; set; } = null!;
        public virtual ICollection<HDVoucher> HDVouchers { get; set; } = new List<HDVoucher>();
    }
}
