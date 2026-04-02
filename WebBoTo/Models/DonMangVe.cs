using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBoTo.Models
{
    public class DonMangVe
    {
        [Key]
        public int Id_Don { get; set; }

        public int Id_TK { get; set; }

        [Required]
        public int HinhThucNhan { get; set; }   // 1 = Lấy tại quán, 2 = Giao tận nơi

        public string? DiaChiGiaoHang { get; set; }
        public double KhoangCachKm { get; set; }
        public decimal PhiGiaoHang { get; set; }

        public decimal TongTienMon { get; set; }
        public decimal TongThanhToan { get; set; }

        public int TrangThai { get; set; } = 0;   // 0: Chờ, 1: Chuẩn bị, 2: Đang giao, 3: Hoàn thành, -1: Hủy
        public DateTime NgayDat { get; set; } = DateTime.Now;

        // Navigation Properties
        [ForeignKey("Id_TK")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        public virtual ICollection<ChiTietDonMangVe> ChiTietDonMangVes { get; set; } = new List<ChiTietDonMangVe>();
    }
}