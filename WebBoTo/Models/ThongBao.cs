using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBoTo.Models
{
    public class ThongBao
    {
        [Key]
        public int Id_ThongBao { get; set; }
        public string? NoiDung { get; set; }
        public DateTime NgayTao { get; set; }

        // Liên kết để biết Admin nào đăng
        public int Id_TK { get; set; }
        [ForeignKey("Id_TK")]
        public virtual TaiKhoan TaiKhoan { get; set; }
    }
}