using System.ComponentModel.DataAnnotations;

namespace WebBoTo.Models
{
    public class GopY
    {
        [Key]
        public int Id_GopY { get; set; }

        [Required]
        [StringLength(100)]
        public required string HoTen { get; set; }

        [Required]
        [StringLength(20)]
        public required string SoDienThoai { get; set; }

        [StringLength(100)]
        public required string Email { get; set; }

        [Required]
        public required string NoiDung { get; set; }

        public DateTime NgayGui { get; set; }

        // Cột này để sau này Admin biết tin nhắn nào đã đọc, tin nào mới
        public bool DaDoc { get; set; }
    }
}