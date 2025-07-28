namespace CinemaManagement.ViewModels
{
    public class ScheduleDto
    {
        public string MaLichChieu { get; set; } = string.Empty;
        public DateTime ThoiGianBatDau { get; set; }
        public DateTime ThoiGianKetThuc { get; set; }
        public decimal Gia { get; set; }
        public string MaPhim { get; set; } = string.Empty;
        public PhimDto? Phim { get; set; }
        public PhongChieuDto? PhongChieu { get; set; }
    }

    public class PhimDto
    {
        public string MaPhim { get; set; } = string.Empty;
        public string TenPhim { get; set; } = string.Empty;
        public string TheLoai { get; set; } = string.Empty;
        public int ThoiLuong { get; set; }
        public string DoTuoiPhanAnh { get; set; } = string.Empty;
    }

    public class PhongChieuDto
    {
        public string MaPhong { get; set; } = string.Empty;
        public string TenPhong { get; set; } = string.Empty;
        public int SoChoNgoi { get; set; }
    }
}
