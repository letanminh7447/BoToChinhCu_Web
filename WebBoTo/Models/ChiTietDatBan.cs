using System.ComponentModel.DataAnnotations;

namespace WebBoTo.Models
{
    public class ChiTietDatBan
    {
        [Key]
        public int Id_CTDB { get; set; }
        public int Id_DatBan { get; set; }
        public int Id_MonAn { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; } // Lưu lại giá tại thời điểm đặt
    }
}