using System.ComponentModel.DataAnnotations;

namespace WebBoTo.Models
{
    public class DatBan
    {
        [Key]
        public int Id_DatBan { get; set; }

        // Liên kết với tài khoản người đặt
        public int Id_TK { get; set; }

        public int SoLuongNguoi { get; set; }
        public DateTime NgayDat { get; set; }
        public TimeSpan GioDat { get; set; }
        public string? GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;
        public int TrangThai { get; set; } = 0; // 0: Chờ xác nhận, 1: Đã xác nhận, 2: Hoàn thành, 3: Đã hủy
        // Thêm dòng này vào dưới cùng của class DatBan
        public decimal TienCoc { get; set; } = 0;
        public int? Id_NhanVienDuyet { get; set; }
        public string? TenNhanVienDuyet { get; set; }
    }
}