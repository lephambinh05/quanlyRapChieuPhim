using CinemaManagement.Models;

namespace CinemaManagement.ViewModels
{
    public class GioHangItem
    {
        public string MaLichChieu { get; set; } = string.Empty;
        public string MaGhe { get; set; } = string.Empty;
        public string TenPhim { get; set; } = string.Empty;
        public string TenPhong { get; set; } = string.Empty;
        public string PhongChieu { get; set; } = string.Empty; // Thêm property này
        public string SoGhe { get; set; } = string.Empty;
        public DateTime ThoiGianChieu { get; set; }
        public decimal Gia { get; set; }
    }

    public class KhachHangChonGheViewModel
    {
        public LichChieu LichChieu { get; set; } = null!;
        public List<GheNgoi> DanhSachGhe { get; set; } = new List<GheNgoi>();
        public List<Ve> DanhSachVeDaBan { get; set; } = new List<Ve>();
        public List<Ve> DanhSachVeDaPhatHanh { get; set; } = new List<Ve>();
        public List<string> GheDaDat { get; set; } = new List<string>();
    }

    public class KhachHangThanhToanViewModel
    {
        public List<GioHangItem> GioHang { get; set; } = new List<GioHangItem>();
        public List<Voucher> Vouchers { get; set; } = new List<Voucher>();
        public decimal TongTien { get; set; }
        public string? MaVoucherChon { get; set; }
        public bool IsDirectPayment { get; set; } = false; // Thêm flag cho thanh toán trực tiếp
        public string? MaHoaDon { get; set; } // Thêm thuộc tính này để truyền mã hóa đơn khi thanh toán lại
        public int ThoiGianConLai { get; set; }
        public HoaDon? HoaDon { get; set; }
        public bool IsExpired { get; set; } = false;
    }

    public class ThanhToanThanhCongViewModel
    {
        public HoaDon HoaDon { get; set; } = new HoaDon();
        public List<VeChiTietViewModel> ChiTietVe { get; set; } = new List<VeChiTietViewModel>();
        public Voucher? VoucherSuDung { get; set; }
        public decimal TienGiamGia { get; set; }
        public KhachHang? KhachHang { get; set; }
        public int DiemTichLuyNhan { get; set; }
    }

    public class VeChiTietViewModel
    {
        public string MaVe { get; set; } = string.Empty;
        public string TenPhim { get; set; } = string.Empty;
        public string TenPhong { get; set; } = string.Empty;
        public string SoGhe { get; set; } = string.Empty;
        public DateTime ThoiGianChieu { get; set; }
        public DateTime HanSuDung { get; set; }
        public decimal Gia { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }

    public class KhachHangViewModels
    {
        public class PhimViewModel
        {
            public List<Phim> Phims { get; set; } = new List<Phim>();
            public List<string> TheLoais { get; set; } = new List<string>();
            public string? CurrentTheLoai { get; set; }
            public string? CurrentSearch { get; set; }
        }

        public class LichSuDatVeViewModel
        {
            public List<HoaDon> LichSuHoaDons { get; set; } = new List<HoaDon>();
        }
    }

    public class SelectedSeatViewModel
    {
        public string MaGhe { get; set; } = string.Empty;
        public string SoGhe { get; set; } = string.Empty;
        public decimal GiaGhe { get; set; }
        public string LoaiGhe { get; set; } = string.Empty;
    }

    public class PhimSortViewModel
    {
        public string? SortBy { get; set; } = "name"; // name, price, popularity, rating, duration
        public string? SortOrder { get; set; } = "asc"; // asc, desc
        public string? TheLoai { get; set; }
        public string? SearchTerm { get; set; }
        
        // Các tùy chọn sắp xếp
        public static readonly Dictionary<string, string> SortOptions = new Dictionary<string, string>
        {
            { "name", "Tên phim" },
            { "price", "Giá vé" },
            { "popularity", "Độ phổ biến" },
            { "duration", "Thời lượng" },
            { "rating", "Đánh giá" }
        };
        
        public static readonly Dictionary<string, string> SortOrders = new Dictionary<string, string>
        {
            { "asc", "Tăng dần" },
            { "desc", "Giảm dần" }
        };
    }
}
