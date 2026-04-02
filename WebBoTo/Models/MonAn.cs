using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBoTo.Models
{
    public class MonAn
    {
        [Key]
        public int Id_MonAn { get; set; }
        [Required]
        public required string TenMon { get; set; }
        public string? MoTa { get; set; }
        [Required]
        public decimal Gia { get; set; }
        public required string HinhAnh { get; set; }

        [Required]
        public int DanhMucId { get; set; }

        [ForeignKey("DanhMucId")]
        public DanhMuc? DanhMuc { get; set; }

        // Khai báo thêm trường này để đánh dấu món đặc trưng
        public bool IsDacTrung { get; set; }
    }

}