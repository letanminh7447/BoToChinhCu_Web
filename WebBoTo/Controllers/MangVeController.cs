using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBoTo.Data;
using WebBoTo.Models;
using System.Security.Claims;

namespace WebBoTo.Controllers
{
    public class MangVeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MangVeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ====================== TRANG ĐẶT MÓN MANG VỀ ======================
        public IActionResult DatMonMangVe()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.DanhMucs = _context.DanhMuc.ToList();

            var userEmail = User.Identity.Name;
            ViewBag.KhachHang = _context.TaiKhoan.FirstOrDefault(u => u.Email == userEmail);

            var danhSachMon = _context.MonAn.ToList();
            return View(danhSachMon);
        }

        // ====================== XÁC NHẬN ĐƠN MANG VỀ ======================
        [HttpPost]
        // ====================== XÁC NHẬN ĐƠN MANG VỀ ======================
        [HttpPost]
        // ====================== XÁC NHẬN ĐƠN MANG VỀ ======================
        [HttpPost]
        public async Task<IActionResult> XacNhanDon([FromBody] DonMangVeRequest request)
        {
            if (request?.Items == null || !request.Items.Any())
                return Json(new { success = false, message = "Không có món ăn nào!" });

            // 1. Lấy thông tin định danh từ Cookie (Có thể là Email, có thể là Họ Tên)
            var claimName = User.Identity?.Name?.Trim();
            var claimEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value?.Trim();

            if (string.IsNullOrEmpty(claimName) && string.IsNullOrEmpty(claimEmail))
                return Json(new { success = false, message = "Hệ thống bị mất phiên đăng nhập. Vui lòng F5 tải lại trang!" });

            // 2. Tìm vét cạn trong Database: Khớp Email HOẶC khớp Họ Tên
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t =>
                (claimEmail != null && t.Email == claimEmail) ||
                (claimName != null && (t.Email == claimName || t.HoTen == claimName))
            );

            // Nếu vẫn không tìm thấy, in hẳn cái tên nó đang tìm ra màn hình để bắt bệnh
            if (tk == null)
                return Json(new { success = false, message = $"Lỗi DB: Không tìm thấy tài khoản mang tên/email là '{claimName}'" });

            // 3. Tiến hành lưu đơn hàng
            var don = new DonMangVe
            {
                Id_TK = tk.Id_TK,
                HinhThucNhan = request.HinhThucNhan,
                DiaChiGiaoHang = request.DiaChiGiaoHang,
                KhoangCachKm = request.KhoangCachKm,
                PhiGiaoHang = request.PhiGiaoHang,
                TongTienMon = request.TongTienMon,
                TongThanhToan = request.TongTienMon + request.PhiGiaoHang,
                TrangThai = 0,           // 0 = Chờ xác nhận
                NgayDat = DateTime.Now
            };

            _context.DonMangVe.Add(don);
            await _context.SaveChangesAsync();

            foreach (var item in request.Items)
            {
                _context.ChiTietDonMangVe.Add(new ChiTietDonMangVe
                {
                    Id_Don = don.Id_Don,
                    Id_MonAn = item.IdMonAn,
                    SoLuong = item.SoLuong,
                    DonGia = item.DonGia,
                    ThanhTien = item.SoLuong * item.DonGia
                });
            }
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đặt hàng mang về thành công!" });
        }

        // ====================== LỊCH SỬ MANG VỀ ======================
        public IActionResult LichSuMangVe()
        {
            // 1. Kiểm tra xem có cookie đăng nhập không
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                TempData["Error"] = "Bạn chưa đăng nhập hoặc phiên đăng nhập đã hết hạn sau khi Restart web.";
                return RedirectToAction("Login", "Account");
            }

            // 2. Lấy Email an toàn hơn (Tránh trường hợp User.Identity.Name bị null)
            var userEmail = User.Identity.Name ?? User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            var user = _context.TaiKhoan.FirstOrDefault(u => u.Email == userEmail || u.HoTen == userEmail);

            // 3. Nếu đăng nhập rồi nhưng không tìm thấy tài khoản trong Database
            if (user == null)
            {
                TempData["Error"] = $"Tài khoản {userEmail} bị lỗi đồng bộ, không tìm thấy trong Database.";
                return RedirectToAction("Login", "Account");
            }

            // Lấy danh sách đơn hàng bình thường
            var dsDonHang = _context.DonMangVe
                                    .Where(d => d.Id_TK == user.Id_TK)
                                    .OrderByDescending(d => d.NgayDat)
                                    .ToList();

            var donIds = dsDonHang.Select(d => d.Id_Don).ToList();
            var chiTietDon = _context.ChiTietDonMangVe
                                     .Where(c => donIds.Contains(c.Id_Don))
                                     .ToList();

            foreach (var ct in chiTietDon)
            {
                ct.MonAn = _context.MonAn.Find(ct.Id_MonAn);
            }

            ViewBag.ChiTietDon = chiTietDon;

            return View(dsDonHang);
        }
    }

    // ====================== CLASS NHẬN JSON ======================
    public class DonMangVeRequest
    {
        public int HinhThucNhan { get; set; }
        public string? DiaChiGiaoHang { get; set; }
        public double KhoangCachKm { get; set; }
        public decimal PhiGiaoHang { get; set; }
        public decimal TongTienMon { get; set; }
        public List<DonMangVeItem> Items { get; set; } = new();
    }

    public class DonMangVeItem
    {
        public int IdMonAn { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}