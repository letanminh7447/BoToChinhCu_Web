using System.ComponentModel.DataAnnotations;

namespace WebBoTo.Models
{
    public class DatTiec
    {
        [Key]
        public int Id_DatTiec { get; set; }
        public int Id_TK { get; set; }        // Lấy từ tài khoản đang đăng nhập
        public int Id_MenuTiec { get; set; }  // Menu khách chọn
        public int SoLuongBan { get; set; }   // Đặc thù đặt tiệc
        public required string DiaChi { get; set; }    // Đặc thù đặt tiệc (địa chỉ nhà khách)
        public DateTime ThoiGian { get; set; }
        public string? LoaiTiec { get; set; }
        public string? GhiChu { get; set; }
        public int STrangThai { get; set; }   // 0: Chờ xác nhận
        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int TienCocTiec { get; set; } = 0; // Thêm dòng này để lưu tiền cọc
    }
}