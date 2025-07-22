use master
if exists (select * from sysdatabases where name = 'RapChieuPhim')
	drop database RapChieuPhim
create database RapChieuPhim
go
use RapChieuPhim

-- Bảng NhanVien
CREATE TABLE NhanVien (
    maNhanVien VARCHAR(10) PRIMARY KEY,
    tenNhanVien NVARCHAR(100),
    chucVu NVARCHAR(50),
    SDT VARCHAR(15),
    ngaySinh DATE
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

-- Bảng KhachHang
CREATE TABLE KhachHang (
    maKhachHang VARCHAR(10) PRIMARY KEY,
    hoTen NVARCHAR(100),
    SDT VARCHAR(15),
    diemTichLuy INT
);

-- Bảng TaiKhoan
CREATE TABLE TaiKhoan (
    maTK VARCHAR(10) PRIMARY KEY,
    Email VARCHAR(100) UNIQUE,
    matKhau VARCHAR(255),
    role NVARCHAR(50),
    trangThai NVARCHAR(20),
    maNhanVien VARCHAR(10) UNIQUE,
	maKhachHang VARCHAR(10),
	FOREIGN KEY (makhachhang) REFERENCES khachhang(makhachhang),
    FOREIGN KEY (maNhanVien) REFERENCES NhanVien(maNhanVien)
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
('TK003', 'dung.pv@gmail.com', 'hashed_password3', N'Khách hàng', N'Hoạt động', NULL, 'KH001')
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
delete from PhongChieu

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