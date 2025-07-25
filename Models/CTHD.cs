using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CinemaManagement.Models
{
    public class CTHD
    {
        [Key]
        [StringLength(10)]
        public string? MaCTHD { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal DonGia { get; set; }

        [StringLength(10)]
        public string? MaVe { get; set; }

        // KHÔNG dùng MaHoaDon làm khóa ngoại nữa, chỉ để tra cứu
        [StringLength(10)]
        public string? MaHoaDon { get; set; }

        public int HoaDonId { get; set; } // FK mới

        [ForeignKey("HoaDonId")]
        public virtual HoaDon HoaDon { get; set; } = null!;

        [ForeignKey("MaVe")]
        public virtual Ve Ve { get; set; } = null!;
    }
}
