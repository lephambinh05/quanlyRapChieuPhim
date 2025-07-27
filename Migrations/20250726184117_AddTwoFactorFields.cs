using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CinemaManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddTwoFactorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KhachHang",
                columns: table => new
                {
                    MaKhachHang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SDT = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    DiemTichLuy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KhachHang", x => x.MaKhachHang);
                });

            migrationBuilder.CreateTable(
                name: "NhanVien",
                columns: table => new
                {
                    MaNhanVien = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenNhanVien = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ChucVu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SDT = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhanVien", x => x.MaNhanVien);
                });

            migrationBuilder.CreateTable(
                name: "TempGioHangItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaGhe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SoGhe = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gia = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MaLichChieu = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaPhim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenPhim = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaPhong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenPhong = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianChieu = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempGioHangItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HoaDon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaHoaDon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TongTien = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ThoiGianTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    MaKhachHang = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    MaNhanVien = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoaDon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoaDon_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang");
                    table.ForeignKey(
                        name: "FK_HoaDon_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "Phim",
                columns: table => new
                {
                    MaPhim = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenPhim = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    TheLoai = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ThoiLuong = table.Column<int>(type: "int", nullable: false),
                    DoTuoiPhanAnh = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViTriFilePhim = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    MaNhanVien = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Phim", x => x.MaPhim);
                    table.ForeignKey(
                        name: "FK_Phim_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "PhongChieu",
                columns: table => new
                {
                    MaPhong = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SoChoNgoi = table.Column<int>(type: "int", nullable: false),
                    LoaiPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaNhanVien = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhongChieu", x => x.MaPhong);
                    table.ForeignKey(
                        name: "FK_PhongChieu_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "TaiKhoan",
                columns: table => new
                {
                    MaTK = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    MatKhau = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaNhanVien = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaKhachHang = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TwoFactorSecret = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorVerified = table.Column<bool>(type: "bit", nullable: false),
                    BackupCodes = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    TwoFactorSetupDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiKhoan", x => x.MaTK);
                    table.ForeignKey(
                        name: "FK_TaiKhoan_KhachHang_MaKhachHang",
                        column: x => x.MaKhachHang,
                        principalTable: "KhachHang",
                        principalColumn: "MaKhachHang",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TaiKhoan_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Voucher",
                columns: table => new
                {
                    MaGiamGia = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TenGiamGia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhanTramGiam = table.Column<int>(type: "int", nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaNhanVien = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voucher", x => x.MaGiamGia);
                    table.ForeignKey(
                        name: "FK_Voucher_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien");
                });

            migrationBuilder.CreateTable(
                name: "GheNgoi",
                columns: table => new
                {
                    MaGhe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SoGhe = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    GiaGhe = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    LoaiGhe = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MaPhong = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GheNgoi", x => x.MaGhe);
                    table.ForeignKey(
                        name: "FK_GheNgoi_PhongChieu_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "PhongChieu",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "LichChieu",
                columns: table => new
                {
                    MaLichChieu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    ThoiGianBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ThoiGianKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaPhong = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    MaPhim = table.Column<string>(type: "nvarchar(10)", nullable: true),
                    MaNhanVien = table.Column<string>(type: "nvarchar(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LichChieu", x => x.MaLichChieu);
                    table.ForeignKey(
                        name: "FK_LichChieu_NhanVien_MaNhanVien",
                        column: x => x.MaNhanVien,
                        principalTable: "NhanVien",
                        principalColumn: "MaNhanVien");
                    table.ForeignKey(
                        name: "FK_LichChieu_Phim_MaPhim",
                        column: x => x.MaPhim,
                        principalTable: "Phim",
                        principalColumn: "MaPhim");
                    table.ForeignKey(
                        name: "FK_LichChieu_PhongChieu_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "PhongChieu",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "HD_voucher",
                columns: table => new
                {
                    MaHoaDon = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MaGiamGia = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    SoLuongVoucher = table.Column<int>(type: "int", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    HoaDonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HD_voucher", x => new { x.MaHoaDon, x.MaGiamGia });
                    table.ForeignKey(
                        name: "FK_HD_voucher_HoaDon_HoaDonId",
                        column: x => x.HoaDonId,
                        principalTable: "HoaDon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HD_voucher_Voucher_MaGiamGia",
                        column: x => x.MaGiamGia,
                        principalTable: "Voucher",
                        principalColumn: "MaGiamGia",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ve",
                columns: table => new
                {
                    MaVe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SoGhe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TenPhim = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    HanSuDung = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gia = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TenPhong = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MaGhe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaLichChieu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaPhim = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaPhong = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ve", x => x.MaVe);
                    table.ForeignKey(
                        name: "FK_Ve_GheNgoi_MaGhe",
                        column: x => x.MaGhe,
                        principalTable: "GheNgoi",
                        principalColumn: "MaGhe");
                    table.ForeignKey(
                        name: "FK_Ve_LichChieu_MaLichChieu",
                        column: x => x.MaLichChieu,
                        principalTable: "LichChieu",
                        principalColumn: "MaLichChieu");
                    table.ForeignKey(
                        name: "FK_Ve_Phim_MaPhim",
                        column: x => x.MaPhim,
                        principalTable: "Phim",
                        principalColumn: "MaPhim");
                    table.ForeignKey(
                        name: "FK_Ve_PhongChieu_MaPhong",
                        column: x => x.MaPhong,
                        principalTable: "PhongChieu",
                        principalColumn: "MaPhong");
                });

            migrationBuilder.CreateTable(
                name: "CTHD",
                columns: table => new
                {
                    MaCTHD = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaVe = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    MaHoaDon = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    HoaDonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CTHD", x => x.MaCTHD);
                    table.ForeignKey(
                        name: "FK_CTHD_HoaDon_HoaDonId",
                        column: x => x.HoaDonId,
                        principalTable: "HoaDon",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CTHD_Ve_MaVe",
                        column: x => x.MaVe,
                        principalTable: "Ve",
                        principalColumn: "MaVe");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CTHD_HoaDonId",
                table: "CTHD",
                column: "HoaDonId");

            migrationBuilder.CreateIndex(
                name: "IX_CTHD_MaVe",
                table: "CTHD",
                column: "MaVe");

            migrationBuilder.CreateIndex(
                name: "IX_GheNgoi_MaPhong",
                table: "GheNgoi",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_HD_voucher_HoaDonId",
                table: "HD_voucher",
                column: "HoaDonId");

            migrationBuilder.CreateIndex(
                name: "IX_HD_voucher_MaGiamGia",
                table: "HD_voucher",
                column: "MaGiamGia");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaKhachHang",
                table: "HoaDon",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_HoaDon_MaNhanVien",
                table: "HoaDon",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_LichChieu_MaNhanVien",
                table: "LichChieu",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_LichChieu_MaPhim",
                table: "LichChieu",
                column: "MaPhim");

            migrationBuilder.CreateIndex(
                name: "IX_LichChieu_MaPhong",
                table: "LichChieu",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_Phim_MaNhanVien",
                table: "Phim",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_PhongChieu_MaNhanVien",
                table: "PhongChieu",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaKhachHang",
                table: "TaiKhoan",
                column: "MaKhachHang");

            migrationBuilder.CreateIndex(
                name: "IX_TaiKhoan_MaNhanVien",
                table: "TaiKhoan",
                column: "MaNhanVien");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaGhe",
                table: "Ve",
                column: "MaGhe");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaLichChieu",
                table: "Ve",
                column: "MaLichChieu");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaPhim",
                table: "Ve",
                column: "MaPhim");

            migrationBuilder.CreateIndex(
                name: "IX_Ve_MaPhong",
                table: "Ve",
                column: "MaPhong");

            migrationBuilder.CreateIndex(
                name: "IX_Voucher_MaNhanVien",
                table: "Voucher",
                column: "MaNhanVien");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CTHD");

            migrationBuilder.DropTable(
                name: "HD_voucher");

            migrationBuilder.DropTable(
                name: "TaiKhoan");

            migrationBuilder.DropTable(
                name: "TempGioHangItems");

            migrationBuilder.DropTable(
                name: "Ve");

            migrationBuilder.DropTable(
                name: "HoaDon");

            migrationBuilder.DropTable(
                name: "Voucher");

            migrationBuilder.DropTable(
                name: "GheNgoi");

            migrationBuilder.DropTable(
                name: "LichChieu");

            migrationBuilder.DropTable(
                name: "KhachHang");

            migrationBuilder.DropTable(
                name: "Phim");

            migrationBuilder.DropTable(
                name: "PhongChieu");

            migrationBuilder.DropTable(
                name: "NhanVien");
        }
    }
}
