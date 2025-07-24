# 🎬 HỆ THỐNG QUẢN LÝ RẠP CHIẾU PHIM - CINEMA MANAGEMENT SYSTEM

## 📋 GIỚI THIỆU TỔNG QUAN

Hệ thống Quản lý Rạp Chiếu Phim là một ứng dụng web toàn diện, hiện đại, được phát triển bằng **ASP.NET Core 9.0** và **Entity Framework Core**. Hệ thống phục vụ đầy đủ các nghiệp vụ vận hành rạp chiếu phim, tối ưu trải nghiệm khách hàng, tự động hóa quy trình bán vé, quản lý nhân sự, phòng chiếu, lịch chiếu, thanh toán đa phương thức, và báo cáo thống kê chuyên sâu.

---

## 🏗️ KIẾN TRÚC & MODULE CHÍNH

- **Khách hàng (Customer):** Đặt vé, chọn ghế, thanh toán, quản lý lịch sử giao dịch, sử dụng voucher.
- **Nhân viên (Staff):** Bán vé tại quầy, xác nhận vé, hỗ trợ khách hàng, báo cáo ca làm việc.
- **Quản lý (Manager):** Quản trị hệ thống, quản lý phim, lịch chiếu, phòng chiếu, nhân viên, voucher, báo cáo doanh thu.
- **Hệ thống thanh toán:** Tích hợp chuyển khoản ngân hàng, xác thực tự động qua API, quản lý trạng thái đơn hàng.
- **Báo cáo & Thống kê:** Dashboard, biểu đồ, xuất báo cáo.

---

## 🔎 CHI TIẾT CHỨC NĂNG THEO MODULE

### 1. KHÁCH HÀNG (FRONTEND)

#### 1.1. Đặt vé trực tuyến
- Xem danh sách phim đang chiếu, chi tiết phim, trailer, lịch chiếu theo ngày/phòng.
- Chọn suất chiếu, phòng chiếu, sơ đồ ghế động (trạng thái: trống, đã đặt, đang chọn, hỏng).
- Thêm vé vào giỏ hàng tạm thời (session hoặc bảng tạm DB).
- Áp dụng voucher giảm giá (tự động kiểm tra điều kiện).

#### 1.2. Thanh toán đa phương thức
- **Chuyển khoản ngân hàng:**
  - Hiển thị thông tin chuyển khoản (số tiền, nội dung, mã hóa đơn).
  - Tích hợp API kiểm tra lịch sử giao dịch ngân hàng (`https://api.sieuthicode.net/historyapiacbv2/...`).
  - Sinh QR code động (VietQR.io) cho khách hàng quét chuyển khoản.
  - Hướng dẫn chi tiết, nút copy số tài khoản/nội dung chuyển khoản.
  - Giao diện hiện đại, thông báo trạng thái, toast notification.
- **Xác thực tự động:**
  - Hệ thống tự động kiểm tra giao dịch chuyển khoản qua cron job (API endpoint `/api/cron/check-banking`).
  - Nếu phát hiện giao dịch hợp lệ (đúng số tiền, đúng nội dung), tự động cập nhật trạng thái đơn hàng sang "Đã thanh toán".
  - Khách hàng được chuyển sang trang xác nhận thành công.
- **Trạng thái đơn hàng:**
  - "Chờ chuyển khoản", "Đã thanh toán", "Đã hủy".
  - Hiển thị trạng thái và nút "Thanh toán ngay" trong lịch sử đặt vé.

#### 1.3. Quản lý lịch sử giao dịch
- Xem danh sách đơn hàng đã đặt, trạng thái thanh toán, chi tiết vé.
- Có thể tiếp tục thanh toán các đơn hàng còn nợ.
- Xem chi tiết từng vé, thông tin phim, ghế, phòng, thời gian.

#### 1.4. Trải nghiệm người dùng
- Giao diện responsive, hiện đại, tối ưu cho mobile và desktop.
- Thông báo realtime trạng thái thanh toán, lỗi, thành công.
- Tự động làm mới trạng thái đơn hàng khi có thay đổi.
- Hỗ trợ copy nhanh thông tin chuyển khoản, QR code rõ nét.

---

### 2. NHÂN VIÊN (STAFF)

- Đăng nhập hệ thống với vai trò nhân viên.
- Bán vé trực tiếp tại quầy, chọn phim, suất chiếu, ghế.
- Xác nhận vé cho khách hàng đến rạp (quét mã, kiểm tra trạng thái).
- Hỗ trợ khách hàng xử lý các vấn đề về vé, đổi/hủy vé theo quy định.
- Báo cáo doanh thu theo ca làm việc, thống kê số vé bán ra.

---

### 3. QUẢN LÝ (MANAGER)

- Quản trị toàn bộ hệ thống, phân quyền người dùng.
- Thêm/sửa/xóa phim, cập nhật thông tin chi tiết, poster, trailer.
- Tạo và quản lý lịch chiếu, kiểm tra xung đột suất chiếu/phòng.
- Quản lý phòng chiếu, sơ đồ ghế, trạng thái ghế.
- Quản lý nhân viên, phân ca, theo dõi hiệu suất làm việc.
- Tạo, quản lý voucher, chương trình khuyến mại.
- Xem báo cáo doanh thu tổng hợp, chi tiết theo ngày/tuần/tháng/phim/phòng.
- Xuất báo cáo Excel, PDF.

---

### 4. HỆ THỐNG THANH TOÁN & TÍCH HỢP NGÂN HÀNG

- Tích hợp chuyển khoản ngân hàng (ACB, ...), sinh QR code động.
- Tự động kiểm tra lịch sử giao dịch qua API bên thứ 3.
- Đối chiếu nội dung chuyển khoản với mã hóa đơn, số tiền.
- Cập nhật trạng thái đơn hàng tự động, gửi thông báo cho khách hàng.
- Lưu log chi tiết các giao dịch, lỗi, trạng thái vào file `error_log.txt`.
- Hỗ trợ cron job (API endpoint) để hệ thống ngoài (scheduler) gọi kiểm tra định kỳ.

---

### 5. BÁO CÁO & THỐNG KÊ

- Dashboard tổng quan: doanh thu, số vé bán, suất chiếu hot, phòng chiếu hiệu quả.
- Biểu đồ trực quan (bar, line, pie chart).
- Thống kê phim được xem nhiều nhất, ít nhất.
- Thống kê hiệu suất phòng chiếu, tỷ lệ lấp đầy ghế.
- Thống kê khách hàng thân thiết, hành vi đặt vé.
- Xuất báo cáo chi tiết theo nhiều tiêu chí.

---

## 🔄 QUY TRÌNH NGHIỆP VỤ CHÍNH

### 1. Đặt vé & Thanh toán chuyển khoản
1. Khách hàng chọn phim, suất chiếu, ghế → thêm vào giỏ hàng.
2. Chọn phương thức thanh toán: chuyển khoản ngân hàng.
3. Hệ thống sinh mã hóa đơn, lưu đơn hàng trạng thái "Chờ chuyển khoản".
4. Hiển thị hướng dẫn chuyển khoản, QR code, nội dung chuyển khoản.
5. Khách hàng thực hiện chuyển khoản đúng số tiền, nội dung.
6. Cron job hoặc hệ thống tự động kiểm tra lịch sử giao dịch qua API:
   - Nếu phát hiện giao dịch hợp lệ: cập nhật đơn hàng sang "Đã thanh toán", phát hành vé.
   - Nếu quá hạn chưa thanh toán: đơn hàng có thể bị hủy.
7. Khách hàng nhận thông báo, có thể xem/trích xuất vé điện tử.

### 2. Quản lý trạng thái đơn hàng
- Đơn hàng luôn có trạng thái rõ ràng: "Chờ chuyển khoản", "Đã thanh toán", "Đã hủy".
- Lịch sử đơn hàng hiển thị trạng thái, cho phép thanh toán lại nếu chưa hoàn tất.
- Tự động đồng bộ trạng thái khi có giao dịch mới.

### 3. Cron job xác nhận giao dịch ngân hàng
- API endpoint `/api/cron/check-banking` cho phép hệ thống ngoài (scheduler) gọi định kỳ.
- Tự động kiểm tra các đơn hàng "Chờ chuyển khoản", đối chiếu giao dịch ngân hàng.
- Cập nhật trạng thái, ghi log, gửi thông báo cho khách hàng.

---

## 🛡️ KỸ THUẬT & BẢO MẬT
- Sử dụng Entity Framework Core, code-first, quản lý migration.
- Session bảo mật, timeout hợp lý, không lưu thông tin nhạy cảm phía client.
- Tích hợp API bên thứ 3 với timeout, retry, log lỗi chi tiết.
- Quản lý transaction khi tạo đơn hàng, phát hành vé.
- Kiểm tra, validate dữ liệu đầu vào ở cả backend và frontend.
- Phân quyền rõ ràng theo vai trò (khách hàng, nhân viên, quản lý).
- Lưu log lỗi, truy vết thao tác hệ thống.

---

## 🚀 HƯỚNG DẪN SỬ DỤNG NHANH THEO VAI TRÒ

### KHÁCH HÀNG
1. Đăng ký/đăng nhập tài khoản.
2. Xem phim, chọn suất chiếu, chọn ghế.
3. Thêm vé vào giỏ, áp dụng voucher nếu có.
4. Chọn thanh toán chuyển khoản, làm theo hướng dẫn, quét QR code.
5. Theo dõi trạng thái đơn hàng trong mục "Lịch sử đặt vé".
6. Nhận vé điện tử sau khi thanh toán thành công.

### NHÂN VIÊN
1. Đăng nhập tài khoản nhân viên.
2. Bán vé tại quầy, xác nhận vé khách hàng.
3. Hỗ trợ khách hàng xử lý sự cố vé.
4. Xem báo cáo doanh thu ca làm việc.

### QUẢN LÝ
1. Đăng nhập tài khoản quản lý.
2. Quản lý phim, lịch chiếu, phòng chiếu, nhân viên, voucher.
3. Xem báo cáo tổng hợp, chi tiết.
4. Cấu hình hệ thống, phân quyền người dùng.

---

## 🔧 CẤU TRÚC DỰ ÁN (TỔNG QUAN)

```text
quanlyRapChieuPhim/
├── Controllers/           # Xử lý request, nghiệp vụ
├── Models/                # Định nghĩa dữ liệu
├── Views/                 # Razor View giao diện
├── Data/                  # DbContext, cấu hình EF
├── ViewModels/            # ViewModel trung gian
├── wwwroot/               # Static files (CSS, JS, images)
├── error_log.txt          # Log lỗi hệ thống
├── SQLrapphim.sql         # Cấu trúc & seed dữ liệu mẫu
└── ...
```

---

## 🤝 ĐÓNG GÓP VÀO DỰ ÁN

### Quy trình đóng góp

1. **Fork repository**
2. **Tạo branch mới**

   ```bash
   git checkout -b feature/ten-tinh-nang-moi
   ```

3. **Commit changes**

   ```bash
   git commit -m "Thêm tính năng: [mô tả ngắn gọn]"
   ```

4. **Push to branch**

   ```bash
   git push origin feature/ten-tinh-nang-moi
   ```

5. **Tạo Pull Request**

## 🐛 BÁO CÁO LỖI

Nếu bạn gặp lỗi, hãy tạo issue mới với thông tin:

- **Mô tả lỗi**: Mô tả chi tiết lỗi xảy ra
- **Các bước tái hiện**: Liệt kê từng bước dẫn đến lỗi
- **Môi trường**: OS, trình duyệt, phiên bản .NET
- **Screenshots**: Đính kèm ảnh nếu có thể

## 📚 TÀI LIỆU THAM KHẢO

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/)
- [SQL Server Documentation](https://docs.microsoft.com/en-us/sql/sql-server/)

## 🔮 KẾ HOẠCH PHÁT TRIỂN

### Phase 1 (Đã hoàn thành) ✅

- ✅ Quản lý phim cơ bản
- ✅ Đặt vé trực tuyến
- ✅ Quản lý lịch chiếu
- ✅ Hệ thống thanh toán

### Phase 2 (Đang phát triển) 🚧

- 🚧 Tích hợp payment gateway
- 🚧 Mobile app
- 🚧 Hệ thống đánh giá phim
- 🚧 Tích hợp mạng xã hội

### Phase 3 (Kế hoạch) 📋

- 📋 AI recommendation system
- 📋 Loyalty program
- 📋 Multi-cinema management
- 📋 Advanced analytics

---

⭐ **Nếu dự án này hữu ích, hãy cho chúng tôi một star trên GitHub!** ⭐
