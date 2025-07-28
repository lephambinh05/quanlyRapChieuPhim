# 🎬 THIẾT LẬP LỊCH CHIẾU PHIM HOÀN CHỈNH

## 📅 Thông tin tổng quan
- **Thời gian**: 28/7/2025 - 10/8/2025 (14 ngày)
- **Số phim**: 9 phim
- **Tổng suất chiếu**: ~126 suất chiếu
- **Phòng chiếu**: 5 phòng hoạt động

## 🎯 Lịch chiếu chi tiết

### 📅 Lịch chiếu thường ngày (Thứ 2 - Thứ 6)
- **Giờ chiếu**: 9:00, 12:00, 15:00, 18:00, 21:00
- **Giá vé**:
  - 2D: 120,000 VNĐ
  - 3D: 180,000 VNĐ
  - IMAX: 250,000 VNĐ

### 🎯 Lịch chiếu cuối tuần (Thứ 7, Chủ nhật)
- **Giờ chiếu**: 8:00, 10:00, 13:00, 16:00, 19:00, 22:00
- **Giá vé**:
  - 2D: 130,000 VNĐ
  - 3D: 190,000 VNĐ
  - IMAX: 260,000 VNĐ

## 🎬 Danh sách phim
1. **PH001** - Nhà tù Shawshank – The Shawshank Redemption (142 phút)
2. **PH002** - Bố già – The Godfather (175 phút)
3. **PH003** - Kỵ sĩ bóng đêm – The Dark Knight (152 phút)
4. **PH004** - 12 người đàn ông giận dữ – 12 Angry Men (96 phút)
5. **PH005** - Chúa tể của những chiếc nhẫn: Sự trở lại của nhà vua (201 phút)
6. **PH006** - Chuyện tào lao – Pulp Fiction (154 phút)
7. **PH007** - Bản danh sách của Schindler – Schindler's List (195 phút)
8. **PH008** - Kẻ đánh cắp giấc mơ – Inception (148 phút)
9. **PH009** - Vua sư tử – The Lion King (88 phút)

## 🏢 Phòng chiếu
- **PC001** - Phòng 1 (2D, 60 ghế)
- **PC002** - Phòng 2 (3D, 50 ghế)
- **PC003** - Phòng 3 (IMAX, 75 ghế)
- **PC004** - Phòng 4 (2D, 65 ghế)
- **PC006** - Phòng 6 (IMAX, 70 ghế)

## 📋 Files đã tạo

### 1. `complete_schedule_setup.sql`
Script chính để thiết lập toàn bộ lịch chiếu
- Xóa lịch chiếu cũ
- Thêm lịch chiếu thường ngày
- Thêm lịch chiếu cuối tuần
- Kiểm tra kết quả

### 2. `add_schedule_28jul_10aug.sql`
Script thêm lịch chiếu thường ngày
- 5 suất chiếu mỗi ngày
- Giá vé chuẩn

### 3. `add_weekend_schedules.sql`
Script thêm lịch chiếu cuối tuần
- 6 suất chiếu mỗi ngày
- Giá vé cao hơn

### 4. `run_schedule_setup.ps1`
Script PowerShell để chạy tự động
- Kiểm tra file SQL
- Chạy script
- Kiểm tra kết quả

## 🚀 Cách sử dụng

### Phương pháp 1: Chạy tự động
```powershell
.\run_schedule_setup.ps1
```

### Phương pháp 2: Chạy thủ công
1. Mở SQL Server Management Studio
2. Kết nối đến database `RapChieuPhim`
3. Mở file `complete_schedule_setup.sql`
4. Chạy script

### Phương pháp 3: Chạy từng phần
1. Chạy `add_schedule_28jul_10aug.sql` cho lịch thường ngày
2. Chạy `add_weekend_schedules.sql` cho cuối tuần

## 📊 Kiểm tra kết quả

### Query kiểm tra tổng quan
```sql
SELECT 
    COUNT(*) as TotalSchedules,
    MIN(thoiGianBatDau) as EarliestShow,
    MAX(thoiGianBatDau) as LatestShow
FROM LichChieu 
WHERE thoiGianBatDau >= '2025-07-28';
```

### Query kiểm tra theo ngày
```sql
SELECT 
    CAST(thoiGianBatDau AS DATE) as NgayChieu,
    DATENAME(WEEKDAY, thoiGianBatDau) as Thu,
    COUNT(*) as SoSuatChieu,
    MIN(FORMAT(thoiGianBatDau, 'HH:mm')) as SuatSomNhat,
    MAX(FORMAT(thoiGianBatDau, 'HH:mm')) as SuatMuonNhat
FROM LichChieu 
WHERE thoiGianBatDau >= '2025-07-28'
GROUP BY CAST(thoiGianBatDau AS DATE)
ORDER BY NgayChieu;
```

### Query kiểm tra chi tiết ngày đầu tiên
```sql
SELECT 
    lc.maLichChieu,
    ph.tenPhim,
    FORMAT(lc.thoiGianBatDau, 'dd/MM/yyyy HH:mm') as ThoiGianChieu,
    FORMAT(lc.thoiGianKetThuc, 'HH:mm') as ThoiGianKetThuc,
    pc.tenPhong,
    pc.loaiPhong,
    FORMAT(lc.gia, '#,##0') + ' VNĐ' as GiaVe,
    DATENAME(WEEKDAY, lc.thoiGianBatDau) as Thu
FROM LichChieu lc
JOIN Phim ph ON lc.maPhim = ph.maPhim
JOIN PhongChieu pc ON lc.maPhong = pc.maPhong
WHERE CAST(lc.thoiGianBatDau AS DATE) = '2025-07-28'
ORDER BY lc.thoiGianBatDau;
```

## 🌐 Truy cập ứng dụng
- **Khách hàng**: http://localhost:7158
- **Quản lý**: http://localhost:7158/QuanLy
- **Chat hỗ trợ**: http://localhost:7158/Chat/Index

## ✅ Kết quả mong đợi
- Tổng cộng ~126 suất chiếu
- Phân bổ đều cho 9 phim
- Lịch chiếu từ 28/7/2025 đến 10/8/2025
- Giá vé phù hợp với loại phòng và ngày trong tuần

## 🎉 Hoàn thành
Lịch chiếu phim đã được thiết lập hoàn chỉnh và sẵn sàng cho khách hàng đặt vé! 