# 🎬 HỆ THỐNG QUẢN LÝ RẠP CHIẾU PHIM - CINEMA MANAGEMENT SYSTEM

## 📋 GIỚI THIỆU TỔNG QUAN

Hệ thống Quản lý Rạp Chiếu Phim là một ứng dụng web hiện đại, xây dựng trên nền tảng **ASP.NET Core MVC** và **Entity Framework Core**. Hệ thống phục vụ toàn diện các nghiệp vụ vận hành rạp chiếu phim, tối ưu trải nghiệm khách hàng, tự động hóa quy trình bán vé, quản lý nhân sự, phòng chiếu, lịch chiếu, thanh toán đa phương thức, phát hành vé, báo cáo thống kê chuyên sâu và tích hợp xác thực thanh toán ngân hàng tự động.

---

## 🏗️ KIẾN TRÚC & MODULE CHÍNH

### 1. **Khách hàng (Customer)**
- Đăng ký/đăng nhập, quản lý tài khoản cá nhân.
- Xem danh sách phim, chi tiết phim, trailer, lịch chiếu.
- Đặt vé trực tuyến: chọn suất chiếu, phòng chiếu, sơ đồ ghế động (trạng thái: trống, đã đặt, đang chọn, hỏng).
- Thêm vé vào giỏ hàng tạm thời (session hoặc bảng tạm DB).
- Áp dụng voucher giảm giá (tự động kiểm tra điều kiện).
- Thanh toán chuyển khoản ngân hàng (tích hợp QR code, hướng dẫn chi tiết, xác thực tự động qua API).
- Quản lý lịch sử giao dịch, trạng thái đơn hàng, tiếp tục thanh toán các đơn hàng còn nợ.
- Xem chi tiết từng vé, thông tin phim, ghế, phòng, thời gian.
- Nhận thông báo realtime trạng thái thanh toán, lỗi, thành công.
- Trải nghiệm UI/UX hiện đại, responsive, tối ưu cho mobile và desktop.

### 2. **Nhân viên (Staff)**
- Đăng nhập hệ thống với vai trò nhân viên.
- Bán vé trực tiếp tại quầy, chọn phim, suất chiếu, ghế.
- Xác nhận vé cho khách hàng đến rạp (quét mã, kiểm tra trạng thái).
- Hỗ trợ khách hàng xử lý các vấn đề về vé, đổi/hủy vé theo quy định.
- Báo cáo doanh thu theo ca làm việc, thống kê số vé bán ra.

### 3. **Quản lý (Manager)**
- Quản trị toàn bộ hệ thống, phân quyền người dùng.
- Thêm/sửa/xóa phim, cập nhật thông tin chi tiết, poster, trailer.
- Tạo và quản lý lịch chiếu, kiểm tra xung đột suất chiếu/phòng.
- Quản lý phòng chiếu, sơ đồ ghế, trạng thái ghế.
- Quản lý nhân viên, phân ca, theo dõi hiệu suất làm việc.
- Tạo, quản lý voucher, chương trình khuyến mại.
- Xem báo cáo doanh thu tổng hợp, chi tiết theo ngày/tuần/tháng/phim/phòng.
- Dashboard, biểu đồ, xuất báo cáo Excel, PDF.

### 4. **Hệ thống thanh toán & tích hợp ngân hàng**
- Tích hợp chuyển khoản ngân hàng (ACB, ...), sinh QR code động (VietQR.io).
- Tự động kiểm tra lịch sử giao dịch qua API bên thứ 3 (`/api/cron/check-banking`).
- Đối chiếu nội dung chuyển khoản với mã hóa đơn, số tiền.
- Cập nhật trạng thái đơn hàng tự động, gửi thông báo cho khách hàng.
- Lưu log chi tiết các giao dịch, lỗi, trạng thái vào file `error_log.txt`.
- Hỗ trợ cron job (API endpoint) để hệ thống ngoài (scheduler) gọi kiểm tra định kỳ.

### 5. **Báo cáo & thống kê**
- Dashboard tổng quan: doanh thu, số vé bán, suất chiếu hot, phòng chiếu hiệu quả.
- Biểu đồ trực quan (bar, line, pie chart).
- Thống kê phim được xem nhiều nhất, ít nhất.
- Thống kê hiệu suất phòng chiếu, tỷ lệ lấp đầy ghế.
- Thống kê khách hàng thân thiết, hành vi đặt vé.
- Xuất báo cáo chi tiết theo nhiều tiêu chí.

---

## 🔎 PHÂN TÍCH CHI TIẾT CHỨC NĂNG & NGHIỆP VỤ

### 1. **Luồng đặt vé & thanh toán chuyển khoản (Khách hàng)**

#### a. Đặt vé trực tuyến
- Chọn phim, suất chiếu, phòng chiếu, sơ đồ ghế động (hiển thị trạng thái từng ghế: trống, đã đặt, đang chọn, hỏng).
- Thêm vé vào giỏ hàng tạm thời (session hoặc bảng tạm DB).
- Áp dụng voucher giảm giá (tự động kiểm tra điều kiện, thời gian, số lượng, trạng thái voucher).

#### b. Thanh toán chuyển khoản ngân hàng
- Hiển thị thông tin chuyển khoản (số tiền, nội dung, mã hóa đơn).
- Sinh QR code động (VietQR.io) cho khách hàng quét chuyển khoản.
- Hướng dẫn chi tiết, nút copy số tài khoản/nội dung chuyển khoản.
- Giao diện hiện đại, thông báo trạng thái, toast notification.
- Đơn hàng được lưu với trạng thái "Chờ chuyển khoản".
- Hệ thống tự động kiểm tra giao dịch chuyển khoản qua cron job (API endpoint `/api/cron/check-banking`).
- Nếu phát hiện giao dịch hợp lệ (đúng số tiền, đúng nội dung), tự động cập nhật trạng thái đơn hàng sang "Đã thanh toán", phát hành vé.
- Khách hàng được chuyển sang trang xác nhận thành công, nhận vé điện tử.
- Lịch sử đơn hàng hiển thị trạng thái, cho phép thanh toán lại nếu chưa hoàn tất.

#### c. Quản lý lịch sử giao dịch
- Xem danh sách đơn hàng đã đặt, trạng thái thanh toán, chi tiết vé.
- Có thể tiếp tục thanh toán các đơn hàng còn nợ.
- Xem chi tiết từng vé, thông tin phim, ghế, phòng, thời gian.

#### d. Trải nghiệm người dùng
- Giao diện responsive, hiện đại, tối ưu cho mobile và desktop.
- Thông báo realtime trạng thái thanh toán, lỗi, thành công.
- Tự động làm mới trạng thái đơn hàng khi có thay đổi.
- Hỗ trợ copy nhanh thông tin chuyển khoản, QR code rõ nét.

### 2. **Bán vé tại quầy (Nhân viên)**
- Đăng nhập hệ thống với vai trò nhân viên.
- Chọn phim, suất chiếu, ghế cho khách hàng.
- Kiểm tra trạng thái ghế, tránh bán trùng ghế đã đặt.
- Áp dụng voucher cho khách hàng nếu có.
- Xác nhận thanh toán, in hóa đơn, xuất vé điện tử.
- Cập nhật điểm tích lũy cho khách hàng.
- Báo cáo doanh thu theo ca làm việc.

### 3. **Quản lý hệ thống (Quản lý)**
- Quản trị toàn bộ hệ thống, phân quyền người dùng.
- Thêm/sửa/xóa phim, cập nhật thông tin chi tiết, poster, trailer.
- Tạo và quản lý lịch chiếu, kiểm tra xung đột suất chiếu/phòng.
- Quản lý phòng chiếu, sơ đồ ghế, trạng thái ghế.
- Quản lý nhân viên, phân ca, theo dõi hiệu suất làm việc.
- Tạo, quản lý voucher, chương trình khuyến mại.
- Xem báo cáo doanh thu tổng hợp, chi tiết theo ngày/tuần/tháng/phim/phòng.
- Dashboard, biểu đồ, xuất báo cáo Excel, PDF.

### 4. **Phát hành vé & quản lý vé**
- Phát hành vé hàng loạt cho từng suất chiếu, từng phòng.
- Quản lý trạng thái vé: "Chưa đặt", "Còn hạn", "Đã bán", "Hết hạn", "Đã hủy".
- Xem danh sách vé, chi tiết vé, cập nhật trạng thái vé.
- Xác nhận vé cho khách hàng đến rạp (quét mã, kiểm tra trạng thái).
- Thống kê vé theo phim, phòng, trạng thái, doanh thu.

### 5. **Báo cáo & thống kê**
- Dashboard tổng quan: doanh thu, số vé bán, suất chiếu hot, phòng chiếu hiệu quả.
- Biểu đồ trực quan (bar, line, pie chart).
- Thống kê phim được xem nhiều nhất, ít nhất.
- Thống kê hiệu suất phòng chiếu, tỷ lệ lấp đầy ghế.
- Thống kê khách hàng thân thiết, hành vi đặt vé.
- Xuất báo cáo chi tiết theo nhiều tiêu chí.

### 6. **Tích hợp ngân hàng & cron job xác thực thanh toán**
- Tích hợp chuyển khoản ngân hàng (ACB, ...), sinh QR code động (VietQR.io).
- Tự động kiểm tra lịch sử giao dịch qua API bên thứ 3 (`/api/cron/check-banking`).
- Đối chiếu nội dung chuyển khoản với mã hóa đơn, số tiền.
- Cập nhật trạng thái đơn hàng tự động, gửi thông báo cho khách hàng.
- Lưu log chi tiết các giao dịch, lỗi, trạng thái vào file `error_log.txt`.
- Hỗ trợ cron job (API endpoint) để hệ thống ngoài (scheduler) gọi kiểm tra định kỳ.

### 7. **Bảo mật & phân quyền**
- Sử dụng Entity Framework Core, code-first, quản lý migration.
- Session bảo mật, timeout hợp lý, không lưu thông tin nhạy cảm phía client.
- Tích hợp API bên thứ 3 với timeout, retry, log lỗi chi tiết.
- Quản lý transaction khi tạo đơn hàng, phát hành vé.
- Kiểm tra, validate dữ liệu đầu vào ở cả backend và frontend.
- Phân quyền rõ ràng theo vai trò (khách hàng, nhân viên, quản lý).
- Lưu log lỗi, truy vết thao tác hệ thống.

---

## 🗂️ PHÂN TÍCH CẤU TRÚC DỮ LIỆU & NGHIỆP VỤ

### 1. **Các thực thể dữ liệu chính**
- **Phim**: Mã phim, tên phim, thể loại, thời lượng, độ tuổi, mô tả, poster, nhân viên quản lý.
- **Lịch chiếu**: Mã lịch chiếu, thời gian bắt đầu/kết thúc, giá, phòng chiếu, phim, nhân viên tạo.
- **Phòng chiếu**: Mã phòng, tên phòng, số chỗ ngồi, loại phòng, trạng thái, nhân viên quản lý.
- **Ghế ngồi**: Mã ghế, số ghế, giá ghế, loại ghế, trạng thái, phòng chiếu.
- **Vé**: Mã vé, trạng thái, số ghế, tên phim, hạn sử dụng, giá, phòng, lịch chiếu, phim, phòng, các chi tiết hóa đơn.
- **Khách hàng**: Mã khách hàng, họ tên, SĐT, điểm tích lũy, tài khoản, hóa đơn.
- **Nhân viên**: Mã nhân viên, tên, chức vụ, SĐT, ngày sinh, phòng chiếu, tài khoản, phim, lịch chiếu, hóa đơn, voucher.
- **Tài khoản**: Mã TK, email, mật khẩu (hash), vai trò, trạng thái, liên kết khách hàng/nhân viên.
- **Hóa đơn**: Id, mã hóa đơn, tổng tiền, thời gian tạo, số lượng vé, mã khách hàng, mã nhân viên, trạng thái ("Chờ chuyển khoản", "Đã thanh toán", "Đã hủy"), chi tiết hóa đơn, voucher.
- **Chi tiết hóa đơn (CTHD)**: Mã CTHD, đơn giá, mã vé, mã hóa đơn, HoaDonId (FK), vé.
- **Voucher**: Mã giảm giá, tên, phần trăm giảm, mô tả, thời gian hiệu lực, nhân viên tạo.
- **HDVoucher**: Mã hóa đơn, mã giảm giá, số lượng, tổng tiền, HoaDonId (FK).
- **TempGioHangItem**: Bảng tạm lưu giỏ hàng cho đơn hàng chuyển khoản, gồm mã hóa đơn, mã ghế, số ghế, giá, lịch chiếu, phim, phòng, thời gian chiếu.

### 2. **Các trạng thái quan trọng**
- **Trạng thái đơn hàng (Hóa đơn)**: "Chờ chuyển khoản", "Đã thanh toán", "Đã hủy".
- **Trạng thái vé**: "Chưa đặt", "Còn hạn", "Đã bán", "Hết hạn", "Đã hủy".
- **Trạng thái ghế**: "Trống", "Đã đặt", "Đang chọn", "Hỏng".
- **Trạng thái tài khoản**: "Hoạt động", "Khóa".

### 3. **Các ViewModel trung gian**
- **DashboardViewModel**: Thống kê tổng quan, doanh thu, số vé, số phim, lịch chiếu, top phim bán chạy, biểu đồ doanh thu.
- **BanVeViewModel**: Danh sách phim, lịch chiếu, ghế, voucher, tổng tiền, khách hàng.
- **ChonGheViewModel**: Lịch chiếu, danh sách ghế, vé đã bán, vé đã phát hành, ghế được chọn, tổng tiền.
- **ThanhToanViewModel**: Lịch chiếu, danh sách ghế được chọn, tổng tiền, khách hàng, voucher, tiền giảm giá, thành tiền.
- **HoaDonViewModel**: Hóa đơn, chi tiết hóa đơn, khách hàng, nhân viên, voucher, tiền giảm giá.
- **KhachHangThanhToanViewModel**: Giỏ hàng, voucher, tổng tiền, mã hóa đơn, flag thanh toán trực tiếp.
- **PhatHanhHangLoatViewModel**: Lịch chiếu, danh sách ghế, ghế có vé, ghế chọn.
- **SoDoGheViewModel**: Lịch chiếu, danh sách ghế, ghế có vé.
- **ThongKeVeViewModel**: Thống kê vé, doanh thu, thống kê theo phim.
- **HuongDanChuyenKhoanViewModel**: Mã hóa đơn, số tiền.

---

## 🖥️ PHÂN TÍCH GIAO DIỆN & TRẢI NGHIỆM NGƯỜI DÙNG

### 1. **Trang khách hàng**
- Trang chủ: Danh sách phim, filter thể loại, tìm kiếm, chi tiết phim, trailer.
- Đặt vé: Chọn suất chiếu, phòng, sơ đồ ghế động, chọn ghế, thêm vào giỏ hàng.
- Thanh toán: Hiển thị thông tin đơn hàng, voucher, tổng tiền, chọn phương thức thanh toán, xác nhận.
- Hướng dẫn chuyển khoản: QR code, thông tin ngân hàng, nội dung chuyển khoản, nút copy, cảnh báo, hướng dẫn chi tiết.
- Lịch sử đặt vé: Danh sách đơn hàng, trạng thái, chi tiết vé, nút thanh toán lại nếu chưa hoàn tất.
- Trang xác nhận thành công: Thông báo, điểm tích lũy, hướng dẫn sử dụng vé, in vé, quay lại trang chủ.
- Trang tài khoản: Thông tin cá nhân, đổi mật khẩu, hỗ trợ khách hàng.

### 2. **Trang nhân viên**
- Trang bán vé: Chọn phim, lịch chiếu, ghế, xác nhận thanh toán, in hóa đơn.
- Trang xác nhận vé: Quét mã, kiểm tra trạng thái vé, xác nhận khách đến rạp.
- Báo cáo ca làm việc: Thống kê số vé bán, doanh thu.

### 3. **Trang quản lý**
- Dashboard: Thống kê tổng quan, biểu đồ, top phim, lịch chiếu gần nhất.
- Quản lý phim: Thêm/sửa/xóa phim, cập nhật thông tin, poster, trailer.
- Quản lý lịch chiếu: Tạo, sửa, xóa lịch chiếu, kiểm tra xung đột.
- Quản lý phòng chiếu: Thêm/sửa/xóa phòng, sơ đồ ghế, trạng thái.
- Quản lý nhân viên: Thêm/sửa/xóa nhân viên, phân ca, theo dõi hiệu suất.
- Quản lý voucher: Tạo, sửa, xóa voucher, chương trình khuyến mại.
- Báo cáo: Doanh thu, số vé, thống kê theo phim, phòng, thời gian.

### 4. **Trang phát hành vé**
- Phát hành vé hàng loạt cho từng suất chiếu, từng phòng.
- Quản lý danh sách vé, chi tiết vé, cập nhật trạng thái vé.
- Thống kê vé theo phim, phòng, trạng thái, doanh thu.

---

## 🔄 QUY TRÌNH NGHIỆP VỤ CHÍNH (SƠ ĐỒ & MÔ TẢ)

### 1. **Đặt vé & thanh toán chuyển khoản**
1. Khách hàng chọn phim, suất chiếu, ghế → thêm vào giỏ hàng.
2. Chọn phương thức thanh toán: chuyển khoản ngân hàng.
3. Hệ thống sinh mã hóa đơn, lưu đơn hàng trạng thái "Chờ chuyển khoản".
4. Hiển thị hướng dẫn chuyển khoản, QR code, nội dung chuyển khoản.
5. Khách hàng thực hiện chuyển khoản đúng số tiền, nội dung.
6. Cron job hoặc hệ thống tự động kiểm tra lịch sử giao dịch qua API:
   - Nếu phát hiện giao dịch hợp lệ: cập nhật đơn hàng sang "Đã thanh toán", phát hành vé.
   - Nếu quá hạn chưa thanh toán: đơn hàng có thể bị hủy.
7. Khách hàng nhận thông báo, có thể xem/trích xuất vé điện tử.

### 2. **Quản lý trạng thái đơn hàng**
- Đơn hàng luôn có trạng thái rõ ràng: "Chờ chuyển khoản", "Đã thanh toán", "Đã hủy".
- Lịch sử đơn hàng hiển thị trạng thái, cho phép thanh toán lại nếu chưa hoàn tất.
- Tự động đồng bộ trạng thái khi có giao dịch mới.

### 3. **Cron job xác nhận giao dịch ngân hàng**
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

---

## ⚠️ LƯU Ý VỀ ENTITY FRAMEWORK & NULLABLE

- **Tất cả các property kiểu `string` trong các model phải để nullable (`string?`)**, trừ property dùng làm khóa chính (`[Key]`).
- Nếu property string không nullable mà dữ liệu trong database có NULL, Entity Framework sẽ ném lỗi `SqlNullValueException` khi truy vấn.
- Các property số (`int`, `decimal`, `DateTime`, ...) nếu cột trong DB có thể NULL thì cũng nên để nullable (`int?`, `decimal?`, `DateTime?`).
- Khi sửa model, hãy build lại project để cập nhật Entity Framework.
- Nếu dùng Code First, sau khi sửa model cần tạo migration mới và update database nếu muốn đồng bộ schema.
- Nếu dùng Database First, cần cập nhật lại các entity từ DB nếu có thay đổi.
- Khi gặp lỗi `SqlNullValueException`, hãy kiểm tra lại các property string trong model và đảm bảo chúng là nullable.

---

## ⏰ HƯỚNG DẪN TÍCH HỢP CRON JOB & API TỰ ĐỘNG HÓA

### 1. Cron kiểm tra thanh toán chuyển khoản
- **Endpoint:** `GET /api/cron/check-banking`
- **Chức năng:** Tự động kiểm tra các hóa đơn trạng thái "Chờ chuyển khoản" với lịch sử giao dịch ngân hàng (qua API bên thứ 3). Nếu phát hiện giao dịch hợp lệ (đúng mã hóa đơn, đúng số tiền), hệ thống sẽ tự động cập nhật trạng thái hóa đơn sang "Đã thanh toán".
- **Cách sử dụng:**
  - Gọi định kỳ bằng cron job server hoặc dịch vụ scheduler (ví dụ: mỗi 1-2 phút).
  - Ví dụ lệnh curl:
    ```bash
    curl -X GET http://<your-domain>/api/cron/check-banking
    ```
- **Kết quả trả về:**
  - Số lượng hóa đơn được cập nhật, danh sách hóa đơn khớp giao dịch.
- **Lưu ý:**
  - Đảm bảo endpoint này chỉ được gọi từ server tin cậy (có thể giới hạn IP hoặc auth nếu cần).
  - API banking bên thứ 3 có thể rate limit, nên cần xử lý retry và log lỗi.

### 2. Cron hủy đơn hàng quá hạn chưa thanh toán
- **Endpoint:** `GET /api/cron/cancel-expired-orders`
- **Chức năng:** Tự động hủy các hóa đơn "Chờ chuyển khoản" quá thời gian cho phép (mặc định 2 phút), giải phóng ghế, xóa bản ghi tạm giữ ghế, cập nhật trạng thái vé chưa bán về "Chưa đặt".
- **Cách sử dụng:**
  - Gọi định kỳ bằng cron job server hoặc dịch vụ scheduler (ví dụ: mỗi 1-2 phút).
  - Ví dụ lệnh curl:
    ```bash
    curl -X GET http://<your-domain>/api/cron/cancel-expired-orders
    ```
- **Kết quả trả về:**
  - Số lượng hóa đơn bị hủy.
- **Lưu ý:**
  - Nên chạy cron này song song với cron kiểm tra thanh toán để đảm bảo hệ thống luôn "sạch" các đơn hàng quá hạn.
  - Có thể điều chỉnh thời gian timeout trong code nếu muốn thay đổi logic hủy đơn.

---