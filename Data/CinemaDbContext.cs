using Microsoft.EntityFrameworkCore;
using CinemaManagement.Models;

namespace CinemaManagement.Data
{
    public class CinemaDbContext : DbContext
    {
        public CinemaDbContext(DbContextOptions<CinemaDbContext> options) : base(options)
        {
        }

        public DbSet<NhanVien> NhanViens { get; set; }
        public DbSet<PhongChieu> PhongChieus { get; set; }
        public DbSet<GheNgoi> GheNgois { get; set; }
        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<TaiKhoan> TaiKhoans { get; set; }
        public DbSet<Phim> Phims { get; set; }
        public DbSet<LichChieu> LichChieus { get; set; }
        public DbSet<Ve> Ves { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<HoaDon> HoaDons { get; set; }
        public DbSet<CTHD> CTHDs { get; set; }
        public DbSet<HDVoucher> HDVouchers { get; set; }
        public DbSet<DanhGia> DanhGias { get; set; }
        public DbSet<CinemaManagement.Models.TempGioHangItem> TempGioHangItems { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure composite key for HDVoucher
            modelBuilder.Entity<HDVoucher>()
                .HasKey(h => new { h.MaHoaDon, h.MaGiamGia });

            // Configure table names to match database
            modelBuilder.Entity<NhanVien>().ToTable("NhanVien");
            modelBuilder.Entity<PhongChieu>().ToTable("PhongChieu");
            modelBuilder.Entity<GheNgoi>().ToTable("GheNgoi");
            modelBuilder.Entity<KhachHang>().ToTable("KhachHang");
            modelBuilder.Entity<TaiKhoan>().ToTable("TaiKhoan");
            modelBuilder.Entity<Phim>().ToTable("Phim");
            modelBuilder.Entity<LichChieu>().ToTable("LichChieu");
            modelBuilder.Entity<Ve>().ToTable("Ve");
            modelBuilder.Entity<Voucher>().ToTable("Voucher");
            modelBuilder.Entity<HoaDon>().ToTable("HoaDon");
            modelBuilder.Entity<CTHD>().ToTable("CTHD");
            modelBuilder.Entity<HDVoucher>().ToTable("HD_voucher");
            modelBuilder.Entity<DanhGia>().ToTable("DanhGia");
            modelBuilder.Entity<ChatMessage>().ToTable("ChatMessage");
            modelBuilder.Entity<ChatRoom>().ToTable("ChatRoom");

            // Configure TaiKhoan relationships with explicit foreign keys
            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.NhanVien)
                .WithMany(n => n.TaiKhoans)
                .HasForeignKey(t => t.maNhanVien)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TaiKhoan>()
                .HasOne(t => t.KhachHang)
                .WithMany(k => k.TaiKhoans)
                .HasForeignKey(t => t.maKhachHang)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure relationships to prevent cascading deletes where appropriate
            modelBuilder.Entity<Ve>()
                .HasOne(v => v.PhongChieu)
                .WithMany(p => p.Ves)
                .HasForeignKey(v => v.MaPhong)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Ve>()
                .HasOne(v => v.Phim)
                .WithMany(p => p.Ves)
                .HasForeignKey(v => v.MaPhim)
                .OnDelete(DeleteBehavior.NoAction);

            // Configure ChatMessage relationship with ChatRoom
            modelBuilder.Entity<ChatMessage>()
                .HasOne<ChatRoom>()
                .WithMany(cr => cr.Messages)
                .HasForeignKey(cm => cm.RoomId)
                .HasPrincipalKey(cr => cr.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure ChatMessage relationship with ChatRoom
            modelBuilder.Entity<ChatMessage>()
                .HasOne<ChatRoom>()
                .WithMany(cr => cr.Messages)
                .HasForeignKey(cm => cm.RoomId)
                .HasPrincipalKey(cr => cr.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            // Không cần ignore vì đã xóa navigation properties trong model
        }
    }
}
