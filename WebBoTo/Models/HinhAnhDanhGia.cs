using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBoTo.Models
{
    public class HinhAnhDanhGia
    {
        [Key]
        public int Id_HinhAnh { get; set; }
        public int Id_DanhGia { get; set; }
        [ForeignKey("Id_DanhGia")]
        public virtual DanhGia DanhGia { get; set; }

        public string DuongDan { get; set; } // Tên file ảnh
    }
}