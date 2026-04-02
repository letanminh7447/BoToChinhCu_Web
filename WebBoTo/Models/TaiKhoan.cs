using System.ComponentModel.DataAnnotations;

namespace WebBoTo.Models
{
    public class TaiKhoan
    {
        [Key]
        public int Id_TK { get; set; }
        [Required]
        public required string HoTen { get; set; }
        public required string SoDienThoai { get; set; }
        [Required, EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string MatKhau { get; set; }
        public string VaiTro { get; set; } = "Customer";

        public string? Avatar { get; set; }

        // true = Đang hoạt động, false = Bị khóa
        public bool TrangThai { get; set; } = true;



    }
}