using System.ComponentModel.DataAnnotations;

namespace CinemaManagement.ViewModels
{
    public class DanhGiaViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn số sao")]
        [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1 đến 5 sao")]
        public int SoSao { get; set; }

        [StringLength(1000, ErrorMessage = "Bình luận không được quá 1000 ký tự")]
        public string? NoiDungBinhLuan { get; set; }

        public string MaPhim { get; set; } = string.Empty;
        public string TenPhim { get; set; } = string.Empty;
        public string maKhachHang { get; set; } = string.Empty;
        public string HoTenKhachHang { get; set; } = string.Empty;
    }

    public class DanhGiaListViewModel
    {
        public string MaPhim { get; set; } = string.Empty;
        public string TenPhim { get; set; } = string.Empty;
        public double DiemTrungBinh { get; set; }
        public int TongSoDanhGia { get; set; }
        public List<DanhGiaItemViewModel> DanhGias { get; set; } = new List<DanhGiaItemViewModel>();
    }

    public class DanhGiaItemViewModel
    {
        public int Id { get; set; }
        public int SoSao { get; set; }
        public string? NoiDungBinhLuan { get; set; }
        public DateTime NgayDanhGia { get; set; }
        public string HoTenKhachHang { get; set; } = string.Empty;
        public bool DaXemPhim { get; set; }
    }

    public class PhimRatingViewModel
    {
        public string MaPhim { get; set; } = string.Empty;
        public string TenPhim { get; set; } = string.Empty;
        public double DiemTrungBinh { get; set; }
        public int TongSoDanhGia { get; set; }
        public int SoSaoCuaBan { get; set; }
        public bool DaDanhGia { get; set; }
        public bool DaXemPhim { get; set; }
    }
} 
