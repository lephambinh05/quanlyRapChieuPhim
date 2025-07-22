using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class HDVoucher
    {
        [StringLength(10)]
        public string MaHoaDon { get; set; } = string.Empty;

        [StringLength(10)]
        public string MaGiamGia { get; set; } = string.Empty;

        public int SoLuongVoucher { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal TongTien { get; set; }

        public int HoaDonId { get; set; } // FK mới

        // Navigation properties
        [ForeignKey("HoaDonId")]
        public virtual HoaDon HoaDon { get; set; } = null!;

        [ForeignKey("MaGiamGia")]
        public virtual Voucher Voucher { get; set; } = null!;
    }
}
