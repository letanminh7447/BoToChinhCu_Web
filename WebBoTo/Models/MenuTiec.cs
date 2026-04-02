using System.ComponentModel.DataAnnotations;

namespace WebBoTo.Models
{
    public class MenuTiec
    {
        [Key]
        public int Id_MenuTiec { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên menu")]
        public required string TenMenu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá")]
        public decimal Gia { get; set; }

        // Danh sách món ăn sẽ lưu dạng chuỗi, mỗi món xuống dòng 1 lần
        [Required(ErrorMessage = "Vui lòng nhập danh sách món ăn")]
        public string? DanhSachMonAn { get; set; }

        public string? HinhAnh1 { get; set; }
        public string? HinhAnh2 { get; set; }
    }
}
