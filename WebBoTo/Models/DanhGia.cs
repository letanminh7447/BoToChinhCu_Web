using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBoTo.Models
{
    public class DanhGia
    {
        [Key]
        public int Id_DanhGia { get; set; }
        public int Id_TK { get; set; }
        [ForeignKey("Id_TK")]
        public virtual TaiKhoan TaiKhoan { get; set; }

        public int SoSao { get; set; }
        public string? BinhLuan { get; set; }
        public DateTime NgayTao { get; set; }
        public bool IsNoiBat { get; set; } // Dành cho Admin tích sao
        public string? AdminReply { get; set; } // Phản hồi của quán

        // Liên kết 1 Đánh giá có nhiều Hình ảnh
        public virtual ICollection<HinhAnhDanhGia> HinhAnhDanhGias { get; set; }
    }
}