using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBoTo.Models
{
    public class DanhMuc
    {
        [Key]
        [Column("Id_DanhMuc")]
        public int Id_DM { get; set; }

        [Required]
        public string? TenDanhMuc { get; set; }

        // Mối quan hệ 1 Danh mục có nhiều Món ăn
        public ICollection<MonAn>? MonAns { get; set; }
    }
}
