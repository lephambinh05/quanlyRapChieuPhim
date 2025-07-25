using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class HDVoucher
    {
        public string? MaHoaDon { get; set; }

        public string? MaGiamGia { get; set; }

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
