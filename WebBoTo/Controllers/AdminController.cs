using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBoTo.Data;
using WebBoTo.Models;
using System.Text;
using System.Text.RegularExpressions;

namespace WebBoTo.Controllers
{
    // ĐÃ SỬA: Bỏ dấu cách sau dấu phẩy để hệ thống nhận diện đúng tên Role
    [Authorize(Roles = "Admin,NhanVien")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // =========================================================================
        // PHẦN A: CÁC CHỨC NĂNG CHỈ DÀNH RIÊNG CHO ADMIN TỐI CAO
        // =========================================================================

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> QuanLyKhachHang()
        {
            var danhSachTaiKhoan = await _context.TaiKhoan.ToListAsync();
            return View(danhSachTaiKhoan);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CapNhatVaiTro(int Id_TK, string VaiTroMoi)
        {
            var taiKhoan = await _context.TaiKhoan.FindAsync(Id_TK);
            if (taiKhoan != null)
            {
                taiKhoan.VaiTro = VaiTroMoi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("QuanLyKhachHang");
        }

        // QUẢN LÝ MENU TIỆC (DÀNH CHO ADMIN)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> QuanLyMenuTiec()
        {
            var danhSachMenu = await _context.MenuTiec.ToListAsync();
            return View(danhSachMenu);
        }
        // ==========================================
        // 1. THÊM MENU TIỆC MỚI (CREATE)
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateMenuTiec()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMenuTiec(MenuTiec menuTiec, List<string> MonAnList, IFormFile? HinhAnh1Upload, IFormFile? HinhAnh2Upload)
        {
            // Lọc bỏ các ô món ăn bị bỏ trống và gộp chúng lại bằng dấu xuống dòng (\n)
            var danhSachHopLe = MonAnList.Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
            if (danhSachHopLe.Count == 0) ModelState.AddModelError("DanhSachMonAn", "Vui lòng nhập ít nhất 1 món ăn!");
            else menuTiec.DanhSachMonAn = string.Join("\n", danhSachHopLe);

            ModelState.Remove("DanhSachMonAn"); // Bỏ qua validate mặc định vì ta đã xử lý tay ở trên
            ModelState.Remove("HinhAnh1");
            ModelState.Remove("HinhAnh2");

            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string thumucImages = Path.Combine(wwwRootPath, "images");
                Directory.CreateDirectory(thumucImages);

                // Lưu Hình 1
                if (HinhAnh1Upload != null)
                {
                    menuTiec.HinhAnh1 = "Tiec1_" + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(HinhAnh1Upload.FileName);
                    using (var fileStream = new FileStream(Path.Combine(thumucImages, menuTiec.HinhAnh1), FileMode.Create))
                        await HinhAnh1Upload.CopyToAsync(fileStream);
                }

                // Lưu Hình 2
                if (HinhAnh2Upload != null)
                {
                    menuTiec.HinhAnh2 = "Tiec2_" + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(HinhAnh2Upload.FileName);
                    using (var fileStream = new FileStream(Path.Combine(thumucImages, menuTiec.HinhAnh2), FileMode.Create))
                        await HinhAnh2Upload.CopyToAsync(fileStream);
                }

                _context.Add(menuTiec);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(QuanLyMenuTiec));
            }
            return View(menuTiec);
        }

        // ==========================================
        // 2. SỬA MENU TIỆC (EDIT)
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditMenuTiec(int? id)
        {
            if (id == null) return NotFound();
            var menuTiec = await _context.MenuTiec.FindAsync(id);
            if (menuTiec == null) return NotFound();
            return View(menuTiec);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditMenuTiec(int id, MenuTiec menuTiec, List<string> MonAnList, IFormFile? HinhAnh1Upload, IFormFile? HinhAnh2Upload)
        {
            if (id != menuTiec.Id_MenuTiec) return NotFound();

            var danhSachHopLe = MonAnList.Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
            if (danhSachHopLe.Count == 0) ModelState.AddModelError("DanhSachMonAn", "Vui lòng nhập ít nhất 1 món ăn!");
            else menuTiec.DanhSachMonAn = string.Join("\n", danhSachHopLe);

            ModelState.Remove("DanhSachMonAn");
            ModelState.Remove("HinhAnh1");
            ModelState.Remove("HinhAnh2");

            if (ModelState.IsValid)
            {
                var menuCu = await _context.MenuTiec.AsNoTracking().FirstOrDefaultAsync(m => m.Id_MenuTiec == id);
                if (menuCu == null) return NotFound();

                string wwwRootPath = _hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string thumucImages = Path.Combine(wwwRootPath, "images");

                // Lưu Hình 1 (Nếu có chọn ảnh mới)
                if (HinhAnh1Upload != null)
                {
                    menuTiec.HinhAnh1 = "Tiec1_" + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(HinhAnh1Upload.FileName);
                    using (var fileStream = new FileStream(Path.Combine(thumucImages, menuTiec.HinhAnh1), FileMode.Create))
                        await HinhAnh1Upload.CopyToAsync(fileStream);
                }
                else { menuTiec.HinhAnh1 = menuCu.HinhAnh1; } // Giữ ảnh cũ

                // Lưu Hình 2 (Nếu có chọn ảnh mới)
                if (HinhAnh2Upload != null)
                {
                    menuTiec.HinhAnh2 = "Tiec2_" + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(HinhAnh2Upload.FileName);
                    using (var fileStream = new FileStream(Path.Combine(thumucImages, menuTiec.HinhAnh2), FileMode.Create))
                        await HinhAnh2Upload.CopyToAsync(fileStream);
                }
                else { menuTiec.HinhAnh2 = menuCu.HinhAnh2; }

                _context.Update(menuTiec);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(QuanLyMenuTiec));
            }
            return View(menuTiec);
        }


        // ==========================================
        // 3. XÓA MENU TIỆC (DELETE)
        // ==========================================
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> DeleteMenuTiec(int? id)
        {
            if (id == null) return NotFound();
            var menu = await _context.MenuTiec.FindAsync(id);
            if (menu == null) return NotFound();
            return View(menu);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteMenuTiec")]
        public async Task<IActionResult> DeleteMenuTiecConfirmed(int id)
        {
            var menu = await _context.MenuTiec.FindAsync(id);
            if (menu != null)
            {
                _context.MenuTiec.Remove(menu);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyMenuTiec));
        }


        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> KhoaMoTaiKhoan(int Id_TK)
        {
            var tk = await _context.TaiKhoan.FindAsync(Id_TK);
            if (tk != null)
            {
                tk.TrangThai = !tk.TrangThai;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("QuanLyKhachHang");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> QuanLyMenu(int? danhMucId)
        {
            ViewBag.DanhMucs = await _context.DanhMuc.ToListAsync();
            ViewBag.SelectedDanhMuc = danhMucId;
            var query = _context.MonAn.Include(m => m.DanhMuc).AsQueryable();
            if (danhMucId.HasValue)
            {
                query = query.Where(m => m.DanhMucId == danhMucId.Value);
            }
            var menu = await query.ToListAsync();
            return View(menu);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult CreateMenu()
        {
            ViewBag.DanhMucs = _context.DanhMuc.ToList();
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateMenu(MonAn monAn, IFormFile? HinhAnhUpload)
        {
            ModelState.Remove("HinhAnh");
            if (ModelState.IsValid)
            {
                string wwwRootPath = _hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                if (HinhAnhUpload != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(HinhAnhUpload.FileName);
                    string extension = Path.GetExtension(HinhAnhUpload.FileName);
                    monAn.HinhAnh = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                    string path = Path.Combine(wwwRootPath, "images", monAn.HinhAnh);

                    Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await HinhAnhUpload.CopyToAsync(fileStream);
                    }
                }
                _context.Add(monAn);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(QuanLyMenu));
            }
            return View(monAn);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> EditMenu(int? id)
        {
            if (id == null) return NotFound();
            var monAn = await _context.MonAn.FindAsync(id);
            if (monAn == null) return NotFound();

            ViewBag.DanhMucs = _context.DanhMuc.ToList();
            return View(monAn);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> EditMenu(int id, MonAn monAn, IFormFile? HinhAnhUpload)
        {
            if (id != monAn.Id_MonAn) return NotFound();

            ModelState.Remove("HinhAnh");
            ModelState.Remove("DanhMuc");

            if (ModelState.IsValid)
            {
                try
                {
                    var monAnCu = await _context.MonAn.AsNoTracking().FirstOrDefaultAsync(m => m.Id_MonAn == id);
                    if (monAnCu == null) return NotFound();

                    if (HinhAnhUpload != null && HinhAnhUpload.Length > 0)
                    {
                        string wwwRootPath = _hostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                        string fileName = Path.GetFileNameWithoutExtension(HinhAnhUpload.FileName);
                        string extension = Path.GetExtension(HinhAnhUpload.FileName);

                        monAn.HinhAnh = fileName + DateTime.Now.ToString("yymmssfff") + extension;
                        string path = Path.Combine(wwwRootPath, "images", monAn.HinhAnh);

                        Directory.CreateDirectory(Path.Combine(wwwRootPath, "images"));
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await HinhAnhUpload.CopyToAsync(fileStream);
                        }
                    }
                    else
                    {
                        monAn.HinhAnh = monAnCu.HinhAnh;
                    }

                    _context.Update(monAn);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(QuanLyMenu));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.MonAn.Any(e => e.Id_MonAn == monAn.Id_MonAn)) return NotFound();
                    else throw;
                }
            }
            ViewBag.DanhMucs = _context.DanhMuc.ToList();
            return View(monAn);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteMenu(int? id)
        {
            if (id == null) return NotFound();
            var monAn = await _context.MonAn.FindAsync(id);
            if (monAn == null) return NotFound();
            return View(monAn);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteMenu")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var monAn = await _context.MonAn.FindAsync(id);
            if (monAn != null)
            {
                _context.MonAn.Remove(monAn);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(QuanLyMenu));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> QuanLyMonDacTrung()
        {
            var allMenu = await _context.MonAn.Include(m => m.DanhMuc).ToListAsync();
            return View(allMenu);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateMonDacTrung(List<int> selectedIds)
        {
            var allMenu = await _context.MonAn.ToListAsync();
            foreach (var item in allMenu)
            {
                item.IsDacTrung = selectedIds.Contains(item.Id_MonAn);
            }
            _context.UpdateRange(allMenu);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Home");
        }

        // ==========================================
        // THÊM DANH MỤC MÓN ĂN MỚI
        // ==========================================

        // 1. Mở Form nhập liệu
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult CreateDanhMuc()
        {
            return View();
        }

        // 2. Nhận dữ liệu và Lưu vào Database
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateDanhMuc(DanhMuc danhMuc)
        {
            if (ModelState.IsValid)
            {
                // Thêm danh mục mới vào DB
                _context.DanhMuc.Add(danhMuc);
                await _context.SaveChangesAsync();

                // Lưu xong thì tự động quay về trang Danh sách Menu
                return RedirectToAction(nameof(QuanLyMenu));
            }
            return View(danhMuc);
        }

        // =========================================================================
        // PHẦN B: CÁC CHỨC NĂNG DÀNH CHO CẢ ADMIN VÀ NHÂN VIÊN
        // (Không gắn ổ khóa Admin ở đây, nên nó sẽ dùng ổ khóa chung trên cùng)
        // =========================================================================

        // 1. HIỂN THỊ DANH SÁCH ĐẶT BÀN (CÓ TÌM KIẾM KHÔNG DẤU & PHÂN TRANG)
        [HttpGet]
        public async Task<IActionResult> QuanLyDatBan(string searchString, int page = 1)
        {
            int pageSize = 10;
            var query = _context.DatBan.AsQueryable();

            ViewBag.SearchString = searchString;

            if (!string.IsNullOrEmpty(searchString))
            {
                // Chuẩn hóa từ khóa: Viết thường và bỏ hết dấu tiếng Việt
                string searchNormalized = ConvertToUnSign(searchString.ToLower().Trim());

                // Kéo toàn bộ tài khoản ra để xử lý tiếng Việt không dấu
                // (Phù hợp với mô hình dữ liệu vừa và nhỏ)
                var allUsers = await _context.TaiKhoan.ToListAsync();

                // Lọc ra ID của những khách hàng khớp Tên (không dấu) HOẶC khớp SĐT
                var matchingUserIds = allUsers
                    .Where(t =>
                        (!string.IsNullOrEmpty(t.HoTen) && ConvertToUnSign(t.HoTen.ToLower()).Contains(searchNormalized)) ||
                        (!string.IsNullOrEmpty(t.SoDienThoai) && t.SoDienThoai.Contains(searchString)) // Hoặc thay bằng t.SDT tùy tên cột trong Model của bạn
                    )
                    .Select(t => t.Id_TK)
                    .ToList();

                // Áp dụng bộ lọc vào danh sách đặt bàn
                query = query.Where(d => matchingUserIds.Contains(d.Id_TK));
            }

            // Sắp xếp đơn mới nhất lên đầu
            query = query.OrderByDescending(d => d.NgayTao);

            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var danhSachDatBan = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var tkIds = danhSachDatBan.Select(d => d.Id_TK).Distinct();
            var taiKhoans = await _context.TaiKhoan
                                          .Where(t => tkIds.Contains(t.Id_TK))
                                          .ToDictionaryAsync(t => t.Id_TK, t => t);

            ViewBag.TaiKhoans = taiKhoans;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(danhSachDatBan);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> QuanLyDatTiec()
        {
            var dsTiec = await (from dt in _context.DatTiec
                                join tk in _context.TaiKhoan on dt.Id_TK equals tk.Id_TK
                                join mn in _context.MenuTiec on dt.Id_MenuTiec equals mn.Id_MenuTiec
                                orderby dt.NgayTao descending
                                select new
                                {
                                    dt.Id_DatTiec,
                                    tk.HoTen,
                                    tk.SoDienThoai,
                                    mn.TenMenu,
                                    dt.SoLuongBan,
                                    dt.ThoiGian,
                                    dt.STrangThai
                                }).ToListAsync();

            ViewBag.DsTiec = dsTiec;
            return View();
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> ChiTietDatTiec(int id)
        {
            var chiTiet = await (from dt in _context.DatTiec
                                 join tk in _context.TaiKhoan on dt.Id_TK equals tk.Id_TK
                                 join mn in _context.MenuTiec on dt.Id_MenuTiec equals mn.Id_MenuTiec
                                 where dt.Id_DatTiec == id
                                 select new
                                 {
                                     dt.Id_DatTiec,
                                     tk.HoTen,
                                     tk.SoDienThoai,
                                     tk.Email,
                                     dt.DiaChi,
                                     dt.ThoiGian,
                                     dt.NgayTao,
                                     dt.SoLuongBan,
                                     dt.GhiChu,
                                     dt.STrangThai,
                                     dt.TienCocTiec,
                                     mn.TenMenu,
                                     mn.DanhSachMonAn,
                                     TongTien = dt.SoLuongBan * mn.Gia
                                 }).FirstOrDefaultAsync();

            if (chiTiet == null) return NotFound();
            ViewBag.ChiTiet = chiTiet;
            return View();
        }

        [HttpPost] // <-- Phải có dòng này vì Form gửi dữ liệu ngầm
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> CapNhatTienCoc(int id, int tienCocTiec)
        {
            var tiec = await _context.DatTiec.FindAsync(id);
            if (tiec != null)
            {
                tiec.TienCocTiec = tienCocTiec;
                await _context.SaveChangesAsync();
            }
            // Lưu xong thì quay lại trang chi tiết của đúng đơn đó
            return RedirectToAction("ChiTietDatTiec", new { id = id });
        }

        // Hàm dùng để Đổi trạng thái (Duyệt / Hủy)
        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> CapNhatTrangThaiTiec(int id, int trangThaiMoi)
        {
            var tiec = await _context.DatTiec.FindAsync(id);
            if (tiec != null)
            {
                tiec.STrangThai = trangThaiMoi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ChiTietDatTiec", new { id = id });
        }

        // ==========================================
        // HÀM HỖ TRỢ: CHUYỂN TIẾNG VIỆT CÓ DẤU THÀNH KHÔNG DẤU
        // ==========================================
        private string ConvertToUnSign(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;

            // Ép chuỗi về chuẩn Unicode FormD (tách riêng chữ và dấu)
            string temp = text.Normalize(NormalizationForm.FormD);

            // Xóa các ký tự dấu
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string result = regex.Replace(temp, string.Empty);

            // Xử lý riêng biệt ký tự 'Đ' và 'đ' của Việt Nam
            return result.Replace('\u0111', 'd').Replace('\u0110', 'D');
        }

        [HttpGet]
        public async Task<IActionResult> ChiTietDatBan(int id)
        {
            var datBan = await _context.DatBan.FindAsync(id);
            if (datBan == null) return NotFound();

            ViewBag.KhachHang = await _context.TaiKhoan.FindAsync(datBan.Id_TK);
            var chiTietMon = await (from ct in _context.ChiTietDatBan
                                    join m in _context.MonAn on ct.Id_MonAn equals m.Id_MonAn
                                    where ct.Id_DatBan == id
                                    select new { m.TenMon, ct.SoLuong, ct.DonGia }).ToListAsync();
            ViewBag.ChiTietMon = chiTietMon;

            return View(datBan);
        }

        [HttpPost]
        public async Task<IActionResult> CapNhatDatBan(int Id_DatBan, int TrangThai, decimal TienCoc)
        {
            var datBan = await _context.DatBan.FindAsync(Id_DatBan);
            if (datBan != null)
            {
                datBan.TrangThai = TrangThai;
                datBan.TienCoc = TienCoc;

                if (TrangThai != 0)
                {
                    var emailNV = User.Identity?.Name;
                    var nv = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == emailNV || t.HoTen == emailNV);

                    if (nv != null)
                    {
                        datBan.Id_NhanVienDuyet = nv.Id_TK;
                        datBan.TenNhanVienDuyet = nv.HoTen;
                    }
                }

                await _context.SaveChangesAsync();
            }
            return RedirectToAction("QuanLyDatBan");
        }

        // ====================== QUẢN LÝ ĐƠN MANG VỀ ======================

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> QuanLyDonMangVe(string search = "", int page = 1)
        {
            int pageSize = 10;

            var query = _context.DonMangVe
                                .Include(d => d.TaiKhoan)
                                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(d => d.TaiKhoan.HoTen.Contains(search) ||
                                         d.DiaChiGiaoHang.Contains(search));
            }

            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var donMangVe = await query
                .OrderByDescending(d => d.NgayDat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.Search = search;

            return View(donMangVe);
        }

        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> ChiTietDonMangVe(int id)
        {
            var don = await _context.DonMangVe
                .Include(d => d.TaiKhoan)
                .Include(d => d.ChiTietDonMangVes)
                    .ThenInclude(ct => ct.MonAn)
                .FirstOrDefaultAsync(d => d.Id_Don == id);

            if (don == null) return NotFound();

            return View(don);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> CapNhatTrangThaiDonMangVe(int id, int trangThaiMoi)
        {
            var don = await _context.DonMangVe.FindAsync(id);
            if (don != null)
            {
                don.TrangThai = trangThaiMoi;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ChiTietDonMangVe", new { id = id });
        }
    }
}