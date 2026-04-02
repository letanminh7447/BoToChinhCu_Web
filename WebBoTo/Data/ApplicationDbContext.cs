using Microsoft.EntityFrameworkCore;
using WebBoTo.Models;

namespace WebBoTo.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<MonAn> MonAn { get; set; }
        public DbSet<TaiKhoan> TaiKhoan { get; set; }
        public DbSet<DanhMuc> DanhMuc { get; set; }
        public DbSet<DatBan> DatBan { get; set; }
        public DbSet<ChiTietDatBan> ChiTietDatBan { get; set; }

        public DbSet<MenuTiec> MenuTiec { get; set; }

        public DbSet<ThongBao> ThongBao { get; set; }
        public DbSet<DanhGia> DanhGia { get; set; }
        public DbSet<HinhAnhDanhGia> HinhAnhDanhGia { get; set; }
        public DbSet<DatTiec> DatTiec { get; set; }

        public DbSet<GopY> GopY { get; set; }

        public DbSet<DonMangVe> DonMangVe { get; set; }
        public DbSet<ChiTietDonMangVe> ChiTietDonMangVe { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            foreach (var property in modelBuilder.Model.GetEntityTypes()
        .SelectMany(t => t.GetProperties())
        .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetColumnType("decimal(18,2)");
            }
            // Code tạo tài khoản Admin cũ của bạn giữ nguyên...
            modelBuilder.Entity<TaiKhoan>().HasData(
                new TaiKhoan { Id_TK = 1, HoTen = "Admin", SoDienThoai = "0123456789", Email = "admin@webboto.vn", MatKhau = "Admin@123", VaiTro = "Admin" }
            );

            // --- THÊM DATA MẪU CHO BẢNG DANH MỤC ---
            modelBuilder.Entity<DanhMuc>().HasData(
                new DanhMuc { Id_DM = 1, TenDanhMuc = "Luộc" },
                new DanhMuc { Id_DM = 2, TenDanhMuc = "Hấp" },
                new DanhMuc { Id_DM = 3, TenDanhMuc = "Nướng" },
                new DanhMuc { Id_DM = 4, TenDanhMuc = "Nhúng" },
                new DanhMuc { Id_DM = 5, TenDanhMuc = "Lẩu" },
                new DanhMuc { Id_DM = 6, TenDanhMuc = "Cháo" },
                new DanhMuc { Id_DM = 7, TenDanhMuc = "Gà" },
                new DanhMuc { Id_DM = 8, TenDanhMuc = "Tôm" },
                new DanhMuc { Id_DM = 9, TenDanhMuc = "Mực" },
                new DanhMuc { Id_DM = 10, TenDanhMuc = "Chiên" },
                new DanhMuc { Id_DM = 11, TenDanhMuc = "Xào" },
                new DanhMuc { Id_DM = 12, TenDanhMuc = "Nước Ngọt" },
                new DanhMuc { Id_DM = 13, TenDanhMuc = "Bia - Rượu" },
                new DanhMuc { Id_DM = 14, TenDanhMuc = "Tráng miệng" }
            );
        }
    }
}