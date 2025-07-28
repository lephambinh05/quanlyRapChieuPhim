use master
if exists (select * from sysdatabases where name = 'RapChieuPhim')
	drop database RapChieuPhim
create database RapChieuPhim
go
use RapChieuPhim




-- Bảng KhachHang
CREATE TABLE KhachHang (
    maKhachHang VARCHAR(10) PRIMARY KEY,
    hoTen NVARCHAR(100),
    SDT VARCHAR(15),
    diemTichLuy INT
);
-- Bảng NhanVien
CREATE TABLE NhanVien (
    maNhanVien VARCHAR(10) PRIMARY KEY,
    tenNhanVien NVARCHAR(100),
    chucVu NVARCHAR(50),
    SDT VARCHAR(15),
    ngaySinh DATE
);

-- Bảng TaiKhoan
CREATE TABLE TaiKhoan (
    maTK VARCHAR(10) PRIMARY KEY,
    Email VARCHAR(100) UNIQUE,
    matKhau VARCHAR(255),
    role NVARCHAR(50),
    trangThai NVARCHAR(20),
    maNhanVien VARCHAR(10),
	maKhachHang VARCHAR(10),
	FOREIGN KEY (makhachhang) REFERENCES khachhang(makhachhang),
    FOREIGN KEY (maNhanVien) REFERENCES NhanVien(maNhanVien)
);


-- Bảng PhongChieu
CREATE TABLE PhongChieu (
    maPhong VARCHAR(10) PRIMARY KEY,
    tenPhong NVARCHAR(50),
    soChoNgoi INT,
    loaiPhong NVARCHAR(50),
	trangThai nvarchar(50),
	maNhanVien VARCHAR(10),
	foreign key (manhanvien) references nhanvien(manhanvien)
);

-- Bảng GheNgoi
CREATE TABLE GheNgoi (
    maGhe VARCHAR(10) PRIMARY KEY,
	soGhe varchar(30),
    giaGhe DECIMAL(10, 2),
    loaiGhe NVARCHAR(50),
    trangThai NVARCHAR(20),
    maPhong VARCHAR(10),
    FOREIGN KEY (maPhong) REFERENCES PhongChieu(maPhong)
);
-- Bảng Phim
CREATE TABLE Phim (
    maPhim VARCHAR(10) PRIMARY KEY,
    tenPhim NVARCHAR(255),
    theLoai NVARCHAR(100),
    thoiLuong INT, -- đơn vị phút
    doTuoiPhanAnh NVARCHAR(10),
    moTa NVARCHAR(MAX),
    viTriFilePhim VARCHAR(255),
	maNhanVien VARCHAR(10),
	FOREIGN KEY (maNhanVien) REFERENCES NhanVien(maNhanVien)
    -- maNhanVien VARCHAR(10), -- Có thể là nhân viên quản lý thông tin phim, nhưng ERD không rõ ràng
    -- FOREIGN KEY (maNhanVien) REFERENCES NhanVien(maNhanVien)
);

-- Bảng LichChieu
CREATE TABLE LichChieu (
    maLichChieu VARCHAR(10) PRIMARY KEY,
    thoiGianBatDau DATETIME,
	thoiGianKetThuc DATETIME,
    gia DECIMAL(10, 2),
    maPhong VARCHAR(10),
    maPhim VARCHAR(10),
    maNhanVien VARCHAR(10), -- Nhân viên lập lịch chiếu
    FOREIGN KEY (maPhong) REFERENCES PhongChieu(maPhong),
    FOREIGN KEY (maPhim) REFERENCES Phim(maPhim),
    FOREIGN KEY (maNhanVien) REFERENCES NhanVien(maNhanVien)
);

-- Bảng Ve
CREATE TABLE Ve (
    maVe VARCHAR(10) PRIMARY KEY,
    trangthai nvarchar(20), -- Ngày tạo/mua vé
    soGhe NVARCHAR(10), -- Có thể là tên ghế (A1, B2,...) hoặc mã ghế
    tenPhim NVARCHAR(255),
    hanSuDung DATETIME,
    gia DECIMAL(10, 2),
    tenPhong NVARCHAR(50),
    maGhe VARCHAR(10),
    maLichChieu VARCHAR(10),
    maPhim VARCHAR(10), -- Duplicate từ LichChieu nhưng giữ lại theo ERD nếu có lý do riêng
    maPhong VARCHAR(10), -- Duplicate từ LichChieu nhưng giữ lại theo ERD nếu có lý do riêng
FOREIGN KEY (maGhe) REFERENCES GheNgoi(maGhe),
    FOREIGN KEY (maLichChieu) REFERENCES LichChieu(maLichChieu),
    FOREIGN KEY (maPhim) REFERENCES Phim(maPhim),
    FOREIGN KEY (maPhong) REFERENCES PhongChieu(maPhong)
);

-- Bảng Voucher
CREATE TABLE Voucher (
    maGiamGia VARCHAR(10) PRIMARY KEY,
    tenGiamGia NVARCHAR(100),
    phanTramGiam INT, -- ví dụ: 10, 20
    moTa NVARCHAR(MAX),
    thoiGianBatDau DATETIME,
    thoiGianKetThuc DATETIME,
    maNhanVien VARCHAR(10), -- Nhân viên tạo voucher
    FOREIGN KEY (maNhanVien) REFERENCES NhanVien(maNhanVien)
);

-- Bảng HoaDon
CREATE TABLE HoaDon (
    maHoaDon VARCHAR(10) PRIMARY KEY,
    tongTien DECIMAL(10, 2),
	thoiGianTao DATETIME,
    soLuong INT,
    maKhachHang VARCHAR(10),
    maNhanVien VARCHAR(10), -- Nhân viên xử lý hóa đơn
    -- maCTHD VARCHAR(10), -- Theo ERD là khóa ngoại của CTHD, nhưng ở đây HoaDon tham chiếu tới CTHD qua mối quan hệ 1-nhiều.
                        -- CTHD nên có khóa ngoại tới HoaDon. Sẽ điều chỉnh ở bảng CTHD.
    FOREIGN KEY (maKhachHang) REFERENCES KhachHang(maKhachHang),
    FOREIGN KEY (maNhanVien) REFERENCES NhanVien(maNhanVien)
);

-- Bảng CTHH (Chi Tiet Hoa Don)
CREATE TABLE CTHD (
    maCTHD VARCHAR(10) PRIMARY KEY,
    donGia DECIMAL(10, 2),
    maVe VARCHAR(10), -- Chi tiết hóa đơn cho một vé
    maHoaDon VARCHAR(10), -- Khóa ngoại tới bảng HoaDon
    FOREIGN KEY (maVe) REFERENCES Ve(maVe),
    FOREIGN KEY (maHoaDon) REFERENCES HoaDon(maHoaDon)
);

-- Bảng HD_voucher (Hóa đơn - Voucher)
CREATE TABLE HD_voucher (
    maHoaDon VARCHAR(10),
    maGiamGia VARCHAR(10),
    soLuongVoucher INT, -- Số lượng voucher áp dụng cho hóa đơn này (nếu có thể áp dụng nhiều)
    tongTien DECIMAL(10, 2), -- Tổng tiền sau khi áp dụng voucher
    PRIMARY KEY (maHoaDon, maGiamGia),
    FOREIGN KEY (maHoaDon) REFERENCES HoaDon(maHoaDon),
    FOREIGN KEY (maGiamGia) REFERENCES Voucher(maGiamGia)
);

USE RapChieuPhim
GO

-- Thêm dữ liệu cho bảng NhanVien
INSERT INTO NhanVien (maNhanVien, tenNhanVien, chucVu, SDT, ngaySinh) VALUES
('NV001', N'Nguyễn Văn An', N'Quản lý', '0901234567', '1990-05-15'),
('NV002', N'Trần Thị Bình', N'Nhân viên bán vé', '0912345678', '1995-08-20'),
('NV003', N'Lê Văn Cường', N'Nhân viên kỹ thuật', '0923456789', '1992-03-10');

-- Thêm dữ liệu cho bảng KhachHang
INSERT INTO KhachHang (maKhachHang, hoTen, SDT, diemTichLuy) VALUES
('KH001', N'Phạm Văn Dũng', '0931234567', 100),
('KH002', N'Ngô Thị Hoa', '0942345678', 50),
('KH003', N'Hoàng Minh Khang', '0953456789', 200);
select * from khachhang
-- Thêm dữ liệu cho bảng TaiKhoan
INSERT INTO TaiKhoan (maTK, Email, matKhau, role, trangThai, maNhanVien, maKhachHang) VALUES
('TK001', 'an.nv@gmail.com', 'hashed_password1', N'Quản lý', N'Hoạt động', 'NV001', NULL),
('TK002', 'binh.tt@gmail.com', 'hashed_password2', N'Nhân viên', N'Hoạt động', 'NV002', NULL),
('TK003', 'dung.pv@gmail.com', 'hashed_password3', N'Khách hàng', N'Hoạt động', NULL, 'KH001'),
('TK004', 'hoa.nt@gmail.com', 'hashed_password4', N'Khách hàng', N'Hoạt động', NULL, 'KH002')
-- Thêm dữ liệu cho bảng Phim
INSERT INTO Phim (maPhim, tenPhim, theLoai, thoiLuong, doTuoiPhanAnh, moTa, viTriFilePhim, maNhanVien) VALUES
('PH001', N'Nhà tù Shawshank – The Shawshank Redemption', N'Chính kịch, Tội phạm', 142, 'R', N'Câu chuyện về hy vọng và sự tự do trong hoàn cảnh khắc nghiệt của nhà tù.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/07/nhung-bo-phim-vuot-nguc-hay-nhat-moi-thoi-dai-Shawshank.jpeg', 'NV001'),
('PH002', N'Bố già – The Godfather', N'Tội phạm, Gia đình', 175, 'R', N'Hành trình của gia đình mafia Corleone đầy quyền lực và bi kịch.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/08/top-100-phim-hay-nhat-moi-thoi-dai-14-e1627828854983.jpg', 'NV001'),
('PH003', N'Kỵ sĩ bóng đêm – The Dark Knight', N'Hành động, Siêu anh hùng', 152, 'PG-13', N'Batman đối đầu với Joker trong cuộc chiến định mệnh tại Gotham.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/08/top-100-phim-hay-nhat-moi-thoi-dai-5.jpg', 'NV001'),
('PH004', N'12 người đàn ông giận dữ – 12 Angry Men', N'Chính kịch, Pháp lý', 96, 'PG', N'12 bồi thẩm viên tranh luận về số phận của một bị cáo trong vụ án giết người.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/08/top-100-phim-hay-nhat-moi-thoi-dai-1.jpeg', 'NV001'),
('PH005', N'Chúa tể của những chiếc nhẫn: Sự trở lại của nhà vua – The Lord of the Rings: The Return of the King', N'Giả tưởng, Phiêu lưu', 201, 'PG-13', N'Kết thúc sử thi của cuộc chiến chống lại Sauron để bảo vệ Trung Địa.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/08/top-100-phim-hay-nhat-moi-thoi-dai-2.jpg', 'NV001'),
('PH006', N'Chuyện tào lao – Pulp Fiction', N'Tội phạm, Hài đen', 154, 'R', N'Những câu chuyện đan xen đầy bất ngờ trong thế giới tội phạm.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/08/top-100-phim-hay-nhat-moi-thoi-dai-4.jpg', 'NV001'),
('PH007', N'Bản danh sách của Schindler – Schindler’s List', N'Lịch sử, Chiến tranh', 195, 'R', N'Câu chuyện có thật về Oskar Schindler cứu hàng ngàn người Do Thái trong Thế chiến II.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/06/nhung-bo-phim-hay-nhat-ve-chien-tranh-the-gioi-thu-2-Schindlers-List-e1624177277140.jpeg', 'NV001'),
('PH008', N'Kẻ đánh cắp giấc mơ – Inception', N'Khoa học viễn tưởng, Hành động', 148, 'PG-13', N'Một tên trộm lành nghề xâm nhập giấc mơ để đánh cắp bí mật.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/07/phim-doat-giai-oscar-hay-nhat-moi-thoi-dai-9-e1627741349691.jpeg', 'NV001'),
('PH009', N'Vua sư tử – The Lion King', N'Hoạt hình, Gia đình', 88, 'G', N'Hành trình của Simba để trở thành vua của Vùng đất Kiêu hãnh.', 'https://bazaarvietnam.vn/wp-content/uploads/2021/10/nhung-bo-phim-hoat-hinh-gan-lien-voi-tuoi-tho-7-scaled-e1633595461435.jpg', 'NV001');

-- Thêm dữ liệu mới cho bảng PhongChieu (6 phòng, mỗi phòng 50-80 ghế)
INSERT INTO PhongChieu (maPhong, tenPhong, soChoNgoi, loaiPhong, trangThai, maNhanVien) VALUES
('PC001', N'Phòng 1', 60, N'2D', N'Hoạt động', 'NV001'),
('PC002', N'Phòng 2', 50, N'3D', N'Hoạt động', 'NV001'),
('PC003', N'Phòng 3', 75, N'IMAX', N'Hoạt động', 'NV001'),
('PC004', N'Phòng 4', 65, N'2D', N'Hoạt động', 'NV001'),
('PC005', N'Phòng 5', 80, N'3D', N'Bảo trì', 'NV001'),
('PC006', N'Phòng 6', 70, N'IMAX', N'Hoạt động', 'NV001');

-- Thêm dữ liệu cho bảng GheNgoi (đầy đủ ghế cho mỗi phòng)
-- Phòng 1: 60 ghế (40 Thường, 20 VIP)
WITH Ghe AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.objects a CROSS JOIN sys.objects b
)
INSERT INTO GheNgoi (maGhe, soGhe, giaGhe, loaiGhe, trangThai, maPhong)
SELECT 
    'G1' + RIGHT('000' + CAST(rn AS VARCHAR(3)), 3),
    CHAR(65 + (rn - 1) / 10) + CAST((rn - 1) % 10 + 1 AS VARCHAR(2)),
    CASE WHEN rn <= 40 THEN 100000 ELSE 150000 END,
    CASE WHEN rn <= 40 THEN N'Thường' ELSE N'VIP' END,
    N'Trống',
    'PC001'
FROM Ghe
WHERE rn <= 60;

-- Phòng 2: 50 ghế (35 Thường, 15 VIP)
WITH Ghe AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.objects a CROSS JOIN sys.objects b
)
INSERT INTO GheNgoi (maGhe, soGhe, giaGhe, loaiGhe, trangThai, maPhong)
SELECT 
    'G2' + RIGHT('000' + CAST(rn AS VARCHAR(3)), 3),
    CHAR(65 + (rn - 1) / 10) + CAST((rn - 1) % 10 + 1 AS VARCHAR(2)),
    CASE WHEN rn <= 35 THEN 120000 ELSE 180000 END,
    CASE WHEN rn <= 35 THEN N'Thường' ELSE N'VIP' END,
    N'Trống',
    'PC002'
FROM Ghe
WHERE rn <= 50;

-- Phòng 3: 75 ghế (50 Thường, 25 VIP)
WITH Ghe AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.objects a CROSS JOIN sys.objects b
)
INSERT INTO GheNgoi (maGhe, soGhe, giaGhe, loaiGhe, trangThai, maPhong)
SELECT 
    'G3' + RIGHT('000' + CAST(rn AS VARCHAR(3)), 3),
    CHAR(65 + (rn - 1) / 10) + CAST((rn - 1) % 10 + 1 AS VARCHAR(2)),
    CASE WHEN rn <= 50 THEN 150000 ELSE 200000 END,
    CASE WHEN rn <= 50 THEN N'Thường' ELSE N'VIP' END,
    N'Trống',
    'PC003'
FROM Ghe
WHERE rn <= 75;

-- Phòng 4: 65 ghế (45 Thường, 20 VIP)
WITH Ghe AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.objects a CROSS JOIN sys.objects b
)
INSERT INTO GheNgoi (maGhe, soGhe, giaGhe, loaiGhe, trangThai, maPhong)
SELECT 
    'G4' + RIGHT('000' + CAST(rn AS VARCHAR(3)), 3),
    CHAR(65 + (rn - 1) / 10) + CAST((rn - 1) % 10 + 1 AS VARCHAR(2)),
    CASE WHEN rn <= 45 THEN 110000 ELSE 160000 END,
    CASE WHEN rn <= 45 THEN N'Thường' ELSE N'VIP' END,
    N'Trống',
    'PC004'
FROM Ghe
WHERE rn <= 65;

-- Phòng 5: 80 ghế (55 Thường, 25 VIP)
WITH Ghe AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.objects a CROSS JOIN sys.objects b
)
INSERT INTO GheNgoi (maGhe, soGhe, giaGhe, loaiGhe, trangThai, maPhong)
SELECT 
    'G5' + RIGHT('000' + CAST(rn AS VARCHAR(3)), 3),
    CHAR(65 + (rn - 1) / 10) + CAST((rn - 1) % 10 + 1 AS VARCHAR(2)),
    CASE WHEN rn <= 55 THEN 130000 ELSE 190000 END,
    CASE WHEN rn <= 55 THEN N'Thường' ELSE N'VIP' END,
    N'Trống',
    'PC005'
FROM Ghe
WHERE rn <= 80;

-- Phòng 6: 70 ghế (50 Thường, 20 VIP)
WITH Ghe AS (
    SELECT 
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn
    FROM sys.objects a CROSS JOIN sys.objects b
)
INSERT INTO GheNgoi (maGhe, soGhe, giaGhe, loaiGhe, trangThai, maPhong)
SELECT 
    'G6' + RIGHT('000' + CAST(rn AS VARCHAR(3)), 3),
    CHAR(65 + (rn - 1) / 10) + CAST((rn - 1) % 10 + 1 AS VARCHAR(2)),
    CASE WHEN rn <= 50 THEN 140000 ELSE 200000 END,
    CASE WHEN rn <= 50 THEN N'Thường' ELSE N'VIP' END,
    N'Trống',
    'PC006'
FROM Ghe
WHERE rn <= 70;

SELECT * FROM PhongChieu;
SELECT maPhong, COUNT(*) AS soGhe FROM GheNgoi GROUP BY maPhong;
SELECT * FROM GheNgoi WHERE maPhong = 'PC001'; -- Kiểm tra ghế của Phòng 1
--------------------------------------------------------------------------------
DECLARE @PhimTemp TABLE (
    maPhim VARCHAR(10),
    thoiLuong INT
);

INSERT INTO @PhimTemp (maPhim, thoiLuong)
VALUES
('PH001', 142), ('PH002', 175), ('PH003', 152), ('PH004', 96),
('PH005', 201), ('PH006', 154), ('PH007', 195), ('PH008', 148), ('PH009', 88);

-- Tạo lịch chiếu
INSERT INTO LichChieu (maLichChieu, thoiGianBatDau, thoiGianKetThuc, gia, maPhong, maPhim, maNhanVien)
SELECT 
    'LC' + RIGHT('000' + CAST(ROW_NUMBER() OVER (ORDER BY p.maPhim, n.n) AS VARCHAR(3)), 3) AS maLichChieu,
    DATEADD(MINUTE, 
        CASE 
            WHEN n.n = 1 THEN 9*60
            WHEN n.n = 2 THEN 12*60
            WHEN n.n = 3 THEN 15*60
            WHEN n.n = 4 THEN 18*60
            WHEN n.n = 5 THEN 21*60
            WHEN n.n = 6 THEN 9*60
            WHEN n.n = 7 THEN 12*60
            WHEN n.n = 8 THEN 15*60
            WHEN n.n = 9 THEN 18*60
            WHEN n.n = 10 THEN 21*60
        END,
        DATEADD(DAY, 
            CASE 
                WHEN n.n <= 5 THEN (n.n - 1) / 3
                ELSE (n.n - 6) / 3 + 3
            END, 
            '2025-07-23')
    ) AS thoiGianBatDau,
    DATEADD(MINUTE, p.thoiLuong + 15, 
        DATEADD(MINUTE, 
            CASE 
                WHEN n.n = 1 THEN 9*60
                WHEN n.n = 2 THEN 12*60
                WHEN n.n = 3 THEN 15*60
                WHEN n.n = 4 THEN 18*60
                WHEN n.n = 5 THEN 21*60
                WHEN n.n = 6 THEN 9*60
                WHEN n.n = 7 THEN 12*60
                WHEN n.n = 8 THEN 15*60
                WHEN n.n = 9 THEN 18*60
                WHEN n.n = 10 THEN 21*60
            END,
            DATEADD(DAY, 
                CASE 
                    WHEN n.n <= 5 THEN (n.n - 1) / 3
                    ELSE (n.n - 6) / 3 + 3
                END, 
                '2025-07-23')
        )
    ) AS thoiGianKetThuc,
    CASE 
        WHEN pc.loaiPhong = '2D' THEN 120000
        WHEN pc.loaiPhong = '3D' THEN 180000
        WHEN pc.loaiPhong = 'IMAX' THEN 250000
    END AS gia,
    pc.maPhong,
    p.maPhim,
    'NV001' AS maNhanVien
FROM @PhimTemp p
CROSS JOIN (
    SELECT n FROM (VALUES (1),(2),(3),(4),(5),(6),(7),(8),(9),(10)) AS Numbers(n)
) n
JOIN (
    SELECT maPhong, loaiPhong
    FROM PhongChieu
    WHERE trangThai = N'Hoạt động'
) pc ON 1=1
WHERE 
    -- Phân bổ phòng cho từng phim dựa trên số thứ tự lịch chiếu
    (p.maPhim = 'PH001' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC001' WHEN n.n % 5 + 1 = 2 THEN 'PC002' WHEN n.n % 5 + 1 = 3 THEN 'PC003' WHEN n.n % 5 + 1 = 4 THEN 'PC004' ELSE 'PC006' END)
    OR (p.maPhim = 'PH002' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC002' WHEN n.n % 5 + 1 = 2 THEN 'PC003' WHEN n.n % 5 + 1 = 3 THEN 'PC004' WHEN n.n % 5 + 1 = 4 THEN 'PC006' ELSE 'PC001' END)
    OR (p.maPhim = 'PH003' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC003' WHEN n.n % 5 + 1 = 2 THEN 'PC004' WHEN n.n % 5 + 1 = 3 THEN 'PC006' WHEN n.n % 5 + 1 = 4 THEN 'PC001' ELSE 'PC002' END)
    OR (p.maPhim = 'PH004' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC004' WHEN n.n % 5 + 1 = 2 THEN 'PC006' WHEN n.n % 5 + 1 = 3 THEN 'PC001' WHEN n.n % 5 + 1 = 4 THEN 'PC002' ELSE 'PC003' END)
    OR (p.maPhim = 'PH005' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC006' WHEN n.n % 5 + 1 = 2 THEN 'PC001' WHEN n.n % 5 + 1 = 3 THEN 'PC002' WHEN n.n % 5 + 1 = 4 THEN 'PC003' ELSE 'PC004' END)
    OR (p.maPhim = 'PH006' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC001' WHEN n.n % 5 + 1 = 2 THEN 'PC002' WHEN n.n % 5 + 1 = 3 THEN 'PC003' WHEN n.n % 5 + 1 = 4 THEN 'PC004' ELSE 'PC006' END)
    OR (p.maPhim = 'PH007' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC002' WHEN n.n % 5 + 1 = 2 THEN 'PC003' WHEN n.n % 5 + 1 = 3 THEN 'PC004' WHEN n.n % 5 + 1 = 4 THEN 'PC006' ELSE 'PC001' END)
    OR (p.maPhim = 'PH008' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC003' WHEN n.n % 5 + 1 = 2 THEN 'PC004' WHEN n.n % 5 + 1 = 3 THEN 'PC006' WHEN n.n % 5 + 1 = 4 THEN 'PC001' ELSE 'PC002' END)
    OR (p.maPhim = 'PH009' AND pc.maPhong = CASE WHEN n.n % 5 + 1 = 1 THEN 'PC004' WHEN n.n % 5 + 1 = 2 THEN 'PC006' WHEN n.n % 5 + 1 = 3 THEN 'PC001' WHEN n.n % 5 + 1 = 4 THEN 'PC002' ELSE 'PC003' END)
ORDER BY p.maPhim, n.n;

CREATE TABLE TempGioHangItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    MaHoaDon VARCHAR(10) NOT NULL,
    MaGhe VARCHAR(10) NOT NULL,
    SoGhe NVARCHAR(30) NOT NULL,
    Gia DECIMAL(10,2) NOT NULL,
    MaLichChieu VARCHAR(10) NOT NULL,
    MaPhim VARCHAR(10) NOT NULL,
    TenPhim NVARCHAR(255) NOT NULL,
    MaPhong VARCHAR(10) NOT NULL,
    TenPhong NVARCHAR(50) NOT NULL,
    ThoiGianChieu DATETIME NOT NULL
);

ALTER TABLE HoaDon
ADD TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Chờ chuyển khoản'

ALTER TABLE CTHD
ADD CONSTRAINT FK_CTHD_maHoaDon FOREIGN KEY (maHoaDon) REFERENCES HoaDon(MaHoaDon);
ALTER TABLE HoaDon ALTER COLUMN MaHoaDon NVARCHAR(14) NOT NULL;

  ALTER TABLE HoaDon DROP COLUMN Id;
  ALTER TABLE HoaDon ADD Id INT IDENTITY(1,1) PRIMARY KEY;
UPDATE CTHD
SET HoaDonId = h.Id
FROM CTHD c
JOIN HoaDon h ON c.MaHoaDon = h.MaHoaDon;

UPDATE HDVoucher
SET HoaDonId = h.Id
FROM HDVoucher v
JOIN HoaDon h ON v.MaHoaDon = h.MaHoaDon;
ALTER TABLE HD_voucher ADD HoaDonId INT;
ALTER TABLE HD_voucher
ADD CONSTRAINT FK_HD_voucher_HoaDon FOREIGN KEY (HoaDonId) REFERENCES HoaDon(Id);

    UPDATE CTHD
    SET HoaDonId = h.Id
    FROM CTHD c
    JOIN HoaDon h ON c.MaHoaDon = h.MaHoaDon;
	    ALTER TABLE CTHD
    ADD CONSTRAINT FK_CTHD_HoaDon FOREIGN KEY (HoaDonId) REFERENCES HoaDon(Id);

	ALTER TABLE CTHD DROP CONSTRAINT FK_CTHD_maHoaDon;
	ALTER TABLE CTHD
ADD CONSTRAINT FK_CTHD_HoaDon FOREIGN KEY (HoaDonId) REFERENCES HoaDon(Id);

UPDATE CTHD
SET HoaDonId = h.Id
FROM CTHD c
JOIN HoaDon h ON c.MaHoaDon = h.MaHoaDon;

SELECT * FROM CTHD WHERE HoaDonId IS NULL OR HoaDonId NOT IN (SELECT Id FROM HoaDon);

   EXEC sp_help 'CTHD';
   select * from TempGioHangItems

   DELETE FROM CTHD;
DELETE FROM HD_voucher;
DELETE FROM Ve;
DELETE FROM HoaDon;
DELETE FROM TempGioHangItems;

  SELECT * FROM CTHD WHERE maHoaDon IS NULL OR HoaDonId IS NULL;
  SELECT * FROM HD_voucher WHERE maHoaDon IS NULL OR HoaDonId IS NULL;

  -- Xóa chi tiết hóa đơn liên quan trước (nếu chưa xóa)
DELETE FROM CTHD WHERE maHoaDon = 'HD052';

-- Xóa voucher hóa đơn liên quan (nếu có)
DELETE FROM HD_voucher WHERE maHoaDon = 'HD052';

-- Xóa hóa đơn
DELETE FROM HoaDon WHERE maHoaDon = 'HD052';

-- Lấy danh sách ghế đã bán cho lịch chiếu LC093
SELECT maGhe FROM Ve WHERE maLichChieu = 'LC093' AND trangThai = N'Đã bán';

-- Lấy danh sách ghế đang giữ tạm cho lịch chiếu LC093
SELECT MaGhe FROM TempGioHangItems WHERE MaLichChieu = 'LC093';

-- Lấy tất cả ghế của phòng chiếu
SELECT maGhe, soGhe FROM GheNgoi WHERE maPhong = 'PC001';

-- Kiểm tra vé chưa bán cho ghế G1002
SELECT * FROM Ve WHERE maGhe = 'G1002' AND trangThai != N'Đã bán';

-- Kiểm tra bản ghi tạm giữ ghế cho G1002
SELECT * FROM TempGioHangItems WHERE MaGhe = 'G1002';

ALTER TABLE HoaDon ADD TrangThai NVARCHAR(50) NULL;
ALTER TABLE HoaDon ADD Id INT IDENTITY(1,1) PRIMARY KEY;

ALTER TABLE HoaDon ADD TrangThai NVARCHAR(50) NULL;

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'HoaDon' AND COLUMN_NAME = 'TrangThai';

  SELECT COLUMN_NAME
  FROM INFORMATION_SCHEMA.COLUMNS
  WHERE TABLE_NAME = 'HoaDon' AND COLUMN_NAME = 'Id';

  ALTER TABLE HoaDon ADD Id INT IDENTITY(1,1);

  ALTER TABLE TempGioHangItems ADD HoaDonId INT NULL;

    ALTER TABLE TempGioHangItems ADD CONSTRAINT FK_TempGioHangItems_HoaDon FOREIGN KEY (HoaDonId) REFERENCES HoaDon(Id);

	ALTER TABLE TempGioHangItems ADD HoaDonId INT NULL;

	ALTER TABLE TempGioHangItems ADD HoaDonId INT NULL;

	   SELECT COLUMN_NAME
   FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_NAME = 'TempGioHangItems' AND COLUMN_NAME = 'HoaDonId';

   SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TempGioHangItems';
select * from TempGioHangItems
sp_help TempGioHangItems
ALTER TABLE TempGioHangItems ADD HoaDonId INT NULL;

SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'TempGioHangItems' AND COLUMN_NAME = 'HoaDonId';

ALTER TABLE CTHD ADD HoaDonId INT NULL;
ALTER TABLE HD_Voucher ADD HoaDonId INT NULL;


SELECT COLUMN_NAME
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'HD_Voucher' AND COLUMN_NAME = 'HoaDonId';

ALTER TABLE HD_Voucher ADD HoaDonId INT NULL;

ALTER TABLE HD_Voucher
ADD CONSTRAINT FK_HD_Voucher_HoaDon FOREIGN KEY (HoaDonId) REFERENCES HoaDon(Id);

  SELECT * FROM CTHD WHERE HoaDonId IS NULL;
  SELECT * FROM HD_Voucher WHERE HoaDonId IS NULL;

  -- Cập nhật cho CTHD
UPDATE CTHD
SET HoaDonId = h.Id
FROM CTHD c
JOIN HoaDon h ON c.MaHoaDon = h.MaHoaDon
WHERE c.HoaDonId IS NULL;

-- Cập nhật cho HD_Voucher
UPDATE HD_Voucher
SET HoaDonId = h.Id
FROM HD_Voucher v
JOIN HoaDon h ON v.MaHoaDon = h.MaHoaDon
WHERE v.HoaDonId IS NULL;

select * from HoaDon

  SELECT MaGhe, TrangThai FROM GheNgoi WHERE MaGhe = 'G1001'; -- hoặc mã ghế bạn muốn kiểm tra

  SELECT MaVe, MaGhe, TrangThai FROM Ve WHERE MaVe IN ('VE002', 'VE003', 'VE004', 'VE005', 'VE006', 'VE007', 'VE008');
UPDATE Ve
SET TrangThai = 'Chưa đặt'
WHERE MaVe IN ('VE002', 'VE003', 'VE004', 'VE005', 'VE006', 'VE007', 'VE008');

-- Xem tất cả tài khoản
SELECT * FROM TaiKhoan;

-- Xem tài khoản có email Google
SELECT * FROM TaiKhoan WHERE Email LIKE '%@gmail.com%';

-- Xem tài khoản khách hàng
SELECT t.*, k.HoTen, k.SDT 
FROM TaiKhoan t 
LEFT JOIN KhachHang k ON t.MaKhachHang = k.MaKhachHang 
WHERE t.Role = 'Khách hàng';

ALTER TABLE TaiKhoan 
ADD TwoFactorSecret NVARCHAR(32) NULL,
    TwoFactorEnabled BIT NOT NULL DEFAULT 0,
    TwoFactorVerified BIT NOT NULL DEFAULT 0,
    BackupCodes NVARCHAR(200) NULL,
    TwoFactorSetupDate DATETIME NULL;

-- 2. Tạo bảng lưu trữ password reset tokens
CREATE TABLE PasswordResetTokens (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(100) NOT NULL,
    Token NVARCHAR(100) NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- 3. Tạo index cho bảng PasswordResetTokens
CREATE INDEX IX_PasswordResetTokens_Email ON PasswordResetTokens(Email);
CREATE INDEX IX_PasswordResetTokens_Token ON PasswordResetTokens(Token);

-- 4. Kiểm tra kết quả
SELECT 'Migration hoàn thành!' as Status;

-- 5. Kiểm tra cấu trúc bảng TaiKhoan sau khi cập nhật
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'TaiKhoan' 
ORDER BY ORDINAL_POSITION;

-- 6. Kiểm tra bảng PasswordResetTokens
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PasswordResetTokens' 
ORDER BY ORDINAL_POSITION; 

-- Tạo bảng DanhGia với kiểu dữ liệu chính xác
CREATE TABLE DanhGia (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    maKhachHang CHAR(50) NOT NULL,
    maPhim CHAR(50) NOT NULL,
    SoSao INT NOT NULL CHECK (SoSao BETWEEN 1 AND 5),
    NoiDungBinhLuan NVARCHAR(1000) NULL,
    NgayDanhGia DATETIME NOT NULL DEFAULT GETDATE(),
    DaXemPhim BIT NOT NULL DEFAULT 0
);

-- Thêm foreign key constraints
ALTER TABLE DanhGia 
ADD CONSTRAINT FK_DanhGia_KhachHang 
FOREIGN KEY (maKhachHang) REFERENCES KhachHang(maKhachHang);

ALTER TABLE DanhGia 
ADD CONSTRAINT FK_DanhGia_Phim 
FOREIGN KEY (maPhim) REFERENCES Phim(maPhim);

select * from danhgia

SELECT MaPhim, LEN(MaPhim) as Length, LEN(LTRIM(RTRIM(MaPhim))) as TrimmedLength
FROM Phim
WHERE MaPhim LIKE '%PH007%'
drop table chatroom
drop table ChatMessage

-- =============================================
-- SCRIPT HOÀN CHỈNH SỬA LỖI CHAT SYSTEM
-- =============================================

-- Bước 1: Kiểm tra cấu trúc hiện tại
PRINT '=== KIỂM TRA CẤU TRÚC HIỆN TẠI ===';
SELECT 'ChatMessage' as TableName, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ChatMessage'
UNION ALL
SELECT 'ChatRoom' as TableName, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ChatRoom';

-- Bước 2: Xóa foreign key constraints cũ
PRINT '=== XÓA FOREIGN KEY CONSTRAINTS CŨ ===';
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatMessage_ChatRoom')
BEGIN
    ALTER TABLE ChatMessage DROP CONSTRAINT FK_ChatMessage_ChatRoom;
    PRINT 'Đã xóa FK_ChatMessage_ChatRoom';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatRoom_KhachHang')
BEGIN
    ALTER TABLE ChatRoom DROP CONSTRAINT FK_ChatRoom_KhachHang;
    PRINT 'Đã xóa FK_ChatRoom_KhachHang';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatRoom_NhanVien')
BEGIN
    ALTER TABLE ChatRoom DROP CONSTRAINT FK_ChatRoom_NhanVien;
    PRINT 'Đã xóa FK_ChatRoom_NhanVien';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatMessage_KhachHang')
BEGIN
    ALTER TABLE ChatMessage DROP CONSTRAINT FK_ChatMessage_KhachHang;
    PRINT 'Đã xóa FK_ChatMessage_KhachHang';
END

IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatMessage_NhanVien')
BEGIN
    ALTER TABLE ChatMessage DROP CONSTRAINT FK_ChatMessage_NhanVien;
    PRINT 'Đã xóa FK_ChatMessage_NhanVien';
END

-- Bước 3: Xóa bảng cũ
PRINT '=== XÓA BẢNG CŨ ===';
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatMessage')
BEGIN
    DROP TABLE ChatMessage;
    PRINT 'Đã xóa bảng ChatMessage cũ';
END

IF EXISTS (SELECT * FROM sys.tables WHERE name = 'ChatRoom')
BEGIN
    DROP TABLE ChatRoom;
    PRINT 'Đã xóa bảng ChatRoom cũ';
END

-- Bước 4: Tạo lại bảng ChatRoom với cấu trúc đúng
PRINT '=== TẠO BẢNG CHATROOM ===';
CREATE TABLE ChatRoom (
    RoomId nvarchar(50) NOT NULL PRIMARY KEY,
    RoomName nvarchar(100) NOT NULL,
    RoomType nvarchar(20) NULL,
    CustomerId nvarchar(10) NULL,
    StaffId nvarchar(10) NULL,
    CreatedAt datetime2 NOT NULL DEFAULT GETDATE(),
    LastActivity datetime2 NOT NULL DEFAULT GETDATE(),
    IsActive bit NOT NULL DEFAULT 1
);
PRINT 'Đã tạo bảng ChatRoom';

-- Bước 5: Tạo lại bảng ChatMessage với cấu trúc đúng
PRINT '=== TẠO BẢNG CHATMESSAGE ===';
CREATE TABLE ChatMessage (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Content nvarchar(1000) NOT NULL,
    Timestamp datetime2 NOT NULL DEFAULT GETDATE(),
    SenderId nvarchar(10) NOT NULL,
    SenderName nvarchar(50) NOT NULL,
    SenderRole nvarchar(20) NOT NULL,
    MessageType nvarchar(20) NULL DEFAULT 'text',
    RoomId nvarchar(50) NULL,
    IsRead bit NOT NULL DEFAULT 0
);
PRINT 'Đã tạo bảng ChatMessage';

-- Bước 6: Tạo indexes để tối ưu hiệu suất
PRINT '=== TẠO INDEXES ===';
CREATE INDEX IX_ChatMessage_RoomId ON ChatMessage (RoomId);
CREATE INDEX IX_ChatMessage_Timestamp ON ChatMessage (Timestamp);
CREATE INDEX IX_ChatMessage_SenderId ON ChatMessage (SenderId);
CREATE INDEX IX_ChatRoom_CustomerId ON ChatRoom (CustomerId);
CREATE INDEX IX_ChatRoom_StaffId ON ChatRoom (StaffId);
PRINT 'Đã tạo tất cả indexes';

-- Bước 7: Thêm dữ liệu mẫu
PRINT '=== THÊM DỮ LIỆU MẪU ===';
INSERT INTO ChatRoom (RoomId, RoomName, RoomType, CreatedAt, LastActivity, IsActive)
VALUES 
('support-general', 'Hỗ trợ chung', 'support', GETDATE(), GETDATE(), 1),
('internal-staff', 'Chat nội bộ nhân viên', 'internal', GETDATE(), GETDATE(), 1),
('management', 'Chat quản lý', 'internal', GETDATE(), GETDATE(), 1);
PRINT 'Đã thêm 3 phòng chat mẫu';

INSERT INTO ChatMessage (Content, Timestamp, SenderId, SenderName, SenderRole, MessageType, RoomId, IsRead)
VALUES 
('Chào mừng đến với hệ thống hỗ trợ!', GETDATE(), 'NV001', 'Nhân viên hỗ trợ', 'Nhân viên', 'text', 'support-general', 0),
('Có ai cần hỗ trợ không?', GETDATE(), 'NV001', 'Nhân viên hỗ trợ', 'Nhân viên', 'text', 'support-general', 0),
('Chào các bạn!', GETDATE(), 'NV002', 'Nguyễn Văn A', 'Nhân viên', 'text', 'internal-staff', 0);
PRINT 'Đã thêm 3 tin nhắn mẫu';

-- Bước 8: Kiểm tra kết quả
PRINT '=== KIỂM TRA KẾT QUẢ ===';
SELECT 'ChatRoom' as TableName, COUNT(*) as RecordCount FROM ChatRoom
UNION ALL
SELECT 'ChatMessage' as TableName, COUNT(*) as RecordCount FROM ChatMessage;

SELECT 'Indexes' as Info, name as IndexName, object_name(object_id) as TableName
FROM sys.indexes 
WHERE object_id IN (OBJECT_ID('ChatMessage'), OBJECT_ID('ChatRoom'))
AND name LIKE 'IX_%';

PRINT '=== HOÀN THÀNH ===';
PRINT 'Hệ thống chat đã được tạo thành công!';
PRINT 'Bạn có thể truy cập:';
PRINT '- Demo: https://localhost:7158/Chat/Demo';
PRINT '- Customer Chat: https://localhost:7158/Chat/Index';
PRINT '- Staff Management: https://localhost:7158/Chat/Manage';
