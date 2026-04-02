using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBoTo.Models
{
    public class ChiTietDonMangVe
    {
        [Key]
        public int Id_ChiTiet { get; set; }

        public int Id_Don { get; set; }
        public int Id_MonAn { get; set; }

        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien { get; set; }

        // Navigation Properties
        [ForeignKey("Id_Don")]
        public virtual DonMangVe DonMangVe { get; set; }

        [ForeignKey("Id_MonAn")]
        public virtual MonAn MonAn { get; set; }
    }
}