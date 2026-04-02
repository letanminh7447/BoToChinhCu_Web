using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBoTo.Data;
using WebBoTo.Models;

namespace WebBoTo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Chỉ lấy những món được Admin tick chọn là Đặc Trưng (tối đa lấy 8 món cho đẹp trang chủ)
            var monDacTrung = await _context.MonAn
                                            .Where(m => m.IsDacTrung == true)
                                            .Take(8)
                                            .ToListAsync();
            return View(monDacTrung);
        }

        // ==========================================
        // 1. MENU MÓN ĂN THƯỜNG (CÓ PHÂN TRANG)
        // ==========================================
        public async Task<IActionResult> Menu(int? danhMucId, int page = 1)
        {
            int pageSize = 9; // BẠN CÓ THỂ ĐỔI SỐ NÀY THÀNH 2 HOẶC 3 ĐỂ TEST NHANH

            ViewBag.DanhMucs = _context.DanhMuc.ToList();
            ViewBag.SelectedDanhMuc = danhMucId;

            // 1. Tạo Query gốc (chưa phân trang)
            var query = _context.MonAn.Include(m => m.DanhMuc).AsQueryable();

            if (danhMucId.HasValue)
            {
                query = query.Where(m => m.DanhMucId == danhMucId.Value);
            }

            // 2. ĐẾM TỔNG SỐ LƯỢNG MÓN SAU KHI ĐÃ LỌC (Rất quan trọng)
            int totalItems = await query.CountAsync();

            // 3. Tính toán số trang bằng cách chia lấy trần (Ceiling)
            // Ví dụ: 9 món / 8 món 1 trang = 1.125 -> làm tròn lên là 2 trang.
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // 4. Báo lỗi ra console của VS để bạn dễ debug (Tùy chọn)
            Console.WriteLine($"TỔNG MÓN: {totalItems} | TỔNG TRANG: {totalPages} | TRANG HIỆN TẠI: {page}");

            // 5. Cắt phần dữ liệu của trang hiện tại
            var menu = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 6. Truyền số liệu qua View
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(menu);
        }

        // HÀM XEM CHI TIẾT MÓN ĂN
        [HttpGet]
        public async Task<IActionResult> ChiTietMonAn(int? id)
        {
            if (id == null) return NotFound();

            // Tìm món ăn theo ID, kèm theo thông tin Danh Mục của món đó
            var monAn = await _context.MonAn
                                      .Include(m => m.DanhMuc)
                                      .FirstOrDefaultAsync(m => m.Id_MonAn == id);

            if (monAn == null) return NotFound();

            return View(monAn);
        }

        [HttpGet]
        public async Task<IActionResult> DatBan()
        {
            // 1. Kiểm tra xem đã có phiên đăng nhập (Cookie) chưa
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Lấy giá trị đang được lưu trong Identity
            var identityValue = User.Identity.Name;

            // 3. Tìm kiếm linh hoạt: khớp Email HOẶC khớp Họ Tên
            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.Email == identityValue || t.HoTen == identityValue);

            // 4. Nếu vẫn không tìm thấy, báo lỗi rõ ràng ra màn hình
            if (taiKhoan == null)
            {
                return Content($"Lỗi hệ thống: Đã đăng nhập với thông tin là '{identityValue}', nhưng không tìm thấy dữ liệu khớp trong Database!");
            }

            // 5. Nếu thành công thì cho vào trang Đặt Bàn
            ViewBag.KhachHang = taiKhoan;

            // ĐÃ SỬA LỖI: Dùng ToListAsync() thay vì ToList()
            ViewBag.DanhMucs = await _context.DanhMuc.ToListAsync();
            var menu = await _context.MonAn.ToListAsync();

            return View(menu);
        }

        // HÀM NHẬN DỮ LIỆU TỪ JS VÀ LƯU VÀO DATABASE
        [HttpPost]
        public async Task<IActionResult> SubmitBooking([FromBody] BookingRequest request)
        {
            try
            {
                var email = User.Identity.Name;
                var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == email || t.HoTen == email);
                if (tk == null) return Json(new { success = false, message = "Lỗi xác thực tài khoản!" });

                // 1. Lưu thông tin Bàn
                var datBan = new DatBan
                {
                    Id_TK = tk.Id_TK,
                    SoLuongNguoi = request.SoNguoi,
                    NgayDat = DateTime.Parse(request.NgayDat),
                    GioDat = TimeSpan.Parse(request.GioDat),
                    NgayTao = DateTime.Now,
                    TrangThai = 0, // 0: Chờ xác nhận
                    GhiChu = ""
                };
                _context.DatBan.Add(datBan);
                await _context.SaveChangesAsync(); // Lưu để lấy Id_DatBan tự tăng

                // 2. Lưu thông tin Món Ăn (nếu khách có chọn)
                if (request.Items != null && request.Items.Any())
                {
                    foreach (var item in request.Items)
                    {
                        var ct = new ChiTietDatBan
                        {
                            Id_DatBan = datBan.Id_DatBan,
                            Id_MonAn = item.IdMonAn,
                            SoLuong = item.SoLuong,
                            DonGia = item.DonGia
                        };
                        _context.ChiTietDatBan.Add(ct);
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // HÀM HIỂN THỊ LỊCH SỬ ĐẶT BÀN CỦA RIÊNG TỪNG KHÁCH HÀNG
        [HttpGet]
        public async Task<IActionResult> LichSuDatBan()
        {
            // 1. Nếu chưa đăng nhập thì đẩy về trang Login
            if (!User.Identity.IsAuthenticated )
            {
                return RedirectToAction("Login", "Account");
            }

            // 2. Tìm ID của khách hàng đang đăng nhập
            var identityName = User.Identity.Name;
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == identityName || t.HoTen == identityName);

            if (tk == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // 3. Lấy danh sách lịch sử đặt bàn CHỈ CỦA KHÁCH HÀNG NÀY
            var lichSu = await _context.DatBan
                                       .Where(d => d.Id_TK == tk.Id_TK)
                                       .OrderByDescending(d => d.NgayTao)
                                       .ToListAsync();

            // 4. Lấy chi tiết các món ăn kèm theo đơn (nếu có)
            ViewBag.ChiTietMon = await (from ct in _context.ChiTietDatBan
                                        join m in _context.MonAn on ct.Id_MonAn equals m.Id_MonAn
                                        select new { ct.Id_DatBan, m.TenMon, ct.SoLuong, ct.DonGia }).ToListAsync();

            return View(lichSu);
        }

        // ==========================================
        // 2. MENU TIỆC CHO KHÁCH HÀNG (CÓ BỘ LỌC + PHÂN TRANG)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> MenuTiec(string sortOrder, int page = 1)
        {
            int pageSize = 8; // Menu tiệc để 6 combo 1 trang

            var query = _context.MenuTiec.AsQueryable();

            // BƯỚC 1: XỬ LÝ LỌC / SẮP XẾP
            ViewBag.CurrentSort = sortOrder; // Lưu lại trạng thái để hiển thị trên Dropdown
            switch (sortOrder)
            {
                case "gia_tang":
                    query = query.OrderBy(m => m.Gia);
                    break;
                case "gia_giam":
                    query = query.OrderByDescending(m => m.Gia);
                    break;
                default:
                    // Mặc định (hiển thị theo thứ tự thêm vào DB, ID giảm dần hoặc tăng dần tùy ý bạn)
                    query = query.OrderByDescending(m => m.Id_MenuTiec);
                    break;
            }

            // BƯỚC 2: PHÂN TRANG SAU KHI ĐÃ SẮP XẾP
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var menuTiec = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(menuTiec);
        }

        // 1. HIỂN THỊ TRANG TIN TỨC & ĐÁNH GIÁ (CÓ LỌC, PHÂN TRANG)
        [HttpGet]
        public async Task<IActionResult> TinTuc(string sortOrder, int page = 1)
        {
            int pageSize = 5; // Hiển thị 5 đánh giá trên 1 trang

            // Lấy Thông Báo
            ViewBag.ThongBaos = await _context.ThongBao
                .Include(t => t.TaiKhoan)
                .OrderByDescending(t => t.NgayTao)
                .ToListAsync();

            // Lấy riêng Đánh giá Nổi Bật cho thanh cuộn (Không đổi)
            ViewBag.DanhGiaNoiBats = await _context.DanhGia
                .Include(d => d.TaiKhoan)
                .Where(d => d.IsNoiBat == true)
                .OrderByDescending(d => d.NgayTao)
                .ToListAsync();

            // LƯU LẠI TRẠNG THÁI BỘ LỌC ĐỂ HIỂN THỊ
            ViewBag.CurrentSort = sortOrder;

            // XÂY DỰNG QUERY LẤY ĐÁNH GIÁ
            // AsSplitQuery() giúp fix lỗi nhân bản dữ liệu khi nối bảng Hình Ảnh
            var query = _context.DanhGia
                .Include(d => d.TaiKhoan)
                .Include(d => d.HinhAnhDanhGias)
                .AsSplitQuery()
                .AsQueryable();

            // XỬ LÝ SẮP XẾP
            switch (sortOrder)
            {
                case "sao_giam":
                    query = query.OrderByDescending(d => d.SoSao).ThenByDescending(d => d.NgayTao);
                    break;
                case "sao_tang":
                    query = query.OrderBy(d => d.SoSao).ThenByDescending(d => d.NgayTao);
                    break;
                default: // Mặc định là Mới nhất
                    query = query.OrderByDescending(d => d.NgayTao);
                    break;
            }

            // TÍNH TOÁN PHÂN TRANG
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            // CẮT DỮ LIỆU THEO TRANG VÀ NHÓM LẠI ĐỂ CHỐNG LẶP TRÙNG ID (nếu do bấm đúp chuột)
            var rawDanhGias = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // Loại bỏ hoàn toàn các đánh giá bị trùng lặp ID
            var danhGias = rawDanhGias.GroupBy(d => d.Id_DanhGia).Select(g => g.First()).ToList();

            // TRUYỀN DỮ LIỆU RA VIEW
            ViewBag.DanhGias = danhGias;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View();
        }

        // 2. KHÁCH HÀNG ĐĂNG ĐÁNH GIÁ MỚI
        [HttpPost]
        public async Task<IActionResult> PostReview(int rating, string comment, List<IFormFile> images)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var emailOrName = User.Identity.Name;
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == emailOrName || t.HoTen == emailOrName);

            if (tk == null) return RedirectToAction("TinTuc");

            // Lưu Đánh Giá vào DB
            var danhGia = new DanhGia
            {
                Id_TK = tk.Id_TK,
                SoSao = rating,
                BinhLuan = comment,
                NgayTao = DateTime.Now,
                IsNoiBat = false
            };
            _context.DanhGia.Add(danhGia);
            await _context.SaveChangesAsync();

            // Lưu Hình Ảnh (Nếu có)
            if (images != null && images.Count > 0)
            {
                // Giới hạn 10 ảnh
                int limit = Math.Min(images.Count, 10);
                for (int i = 0; i < limit; i++)
                {
                    var file = images[i];
                    if (file.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        _context.HinhAnhDanhGia.Add(new HinhAnhDanhGia
                        {
                            Id_DanhGia = danhGia.Id_DanhGia,
                            DuongDan = fileName
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("TinTuc");
        }

        [HttpGet]
        public async Task<IActionResult> DatTiecTaiGia(int? id)
        {
            // 1. Kiểm tra đăng nhập
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            // 2. Lấy thông tin tài khoản
            var identityValue = User.Identity.Name;
            var taiKhoan = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.Email == identityValue || t.HoTen == identityValue);

            if (taiKhoan == null) return Content("Lỗi: Không tìm thấy tài khoản!");

            // Truyền dữ liệu sang View (Giao diện)
            ViewBag.KhachHang = taiKhoan;
            ViewBag.DanhSachMenu = await _context.MenuTiec.ToListAsync();

            // 3. Xử lý hiển thị Menu
            MenuTiec menu = null;

            if (id.HasValue && id > 0)
            {
                var foundMenu = await _context.MenuTiec.FirstOrDefaultAsync(m => m.Id_MenuTiec == id);
                if (foundMenu != null)
                {
                    menu = foundMenu;
                    ViewBag.SelectedMenuId = id; // Thêm dòng này để tự động tích chọn thẻ Menu tương ứng bên giao diện
                }
            }

            return View(menu);
        }

        [HttpPost]
        public async Task<IActionResult> SubmitDatTiec(DatTiec request)
        {
            var identityValue = User.Identity.Name;
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == identityValue || t.HoTen == identityValue);

            if (tk == null) return RedirectToAction("Login", "Account");

            // Gắn thông tin ẩn
            request.Id_TK = tk.Id_TK;
            request.NgayTao = DateTime.Now;
            request.STrangThai = 0; // 0 = Chờ xác nhận

            // Lưu vào Database
            _context.DatTiec.Add(request);
            await _context.SaveChangesAsync();

            TempData["ThongBaoTiec"] = "Đặt tiệc thành công! Vui lòng chờ nhân viên xác nhận.";

            // Đặt xong chuyển hướng về trang Lịch sử
            return RedirectToAction("LichSuDatTiec");
        }

        [HttpGet]
        public async Task<IActionResult> LichSuDatTiec()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var email = User.Identity.Name;
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == email || t.HoTen == email);
            if (tk == null) return RedirectToAction("Login", "Account");

            ViewBag.LichSuTiec = await (from dt in _context.DatTiec
                                        join mn in _context.MenuTiec on dt.Id_MenuTiec equals mn.Id_MenuTiec
                                        where dt.Id_TK == tk.Id_TK
                                        orderby dt.NgayTao descending
                                        select new
                                        {
                                            dt.Id_DatTiec,
                                            dt.ThoiGian,
                                            dt.SoLuongBan,
                                            dt.DiaChi,
                                            dt.STrangThai,
                                            dt.TienCocTiec,
                                            mn.TenMenu,
                                            TongTien = dt.SoLuongBan * mn.Gia
                                        }).ToListAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ChiTietLichSuTiec(int id)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login", "Account");

            var email = User.Identity.Name;
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == email || t.HoTen == email);

            var chiTiet = await (from dt in _context.DatTiec
                                 join t in _context.TaiKhoan on dt.Id_TK equals t.Id_TK
                                 join mn in _context.MenuTiec on dt.Id_MenuTiec equals mn.Id_MenuTiec
                                 where dt.Id_DatTiec == id && dt.Id_TK == tk.Id_TK
                                 select new
                                 {
                                     dt.Id_DatTiec,
                                     t.HoTen,
                                     t.SoDienThoai,
                                     t.Email,
                                     dt.DiaChi,
                                     dt.ThoiGian,
                                     dt.NgayTao,
                                     dt.SoLuongBan,
                                     dt.GhiChu,
                                     dt.STrangThai,
                                     dt.TienCocTiec,
                                     mn.TenMenu,
                                     mn.DanhSachMonAn,
                                     mn.Gia,
                                     TongTien = dt.SoLuongBan * mn.Gia
                                 }).FirstOrDefaultAsync();

            if (chiTiet == null) return NotFound();
            ViewBag.ChiTietTiec = chiTiet;
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,NhanVien")]
        public async Task<IActionResult> CapNhatTienCoc(int id, int tienCocTiec)
        {
            var tiec = await _context.DatTiec.FindAsync(id);
            if (tiec != null)
            {
                tiec.TienCocTiec = tienCocTiec;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("ChiTietDatTiec", new { id = id });
        }


        // ==========================================
        // CÁC HÀM DÀNH RIÊNG CHO ADMIN (QUẢN LÝ TIN TỨC)
        // ==========================================

        // 1. ADMIN ĐĂNG THÔNG BÁO MỚI
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateThongBao(string noiDung)
        {
            var emailOrName = User.Identity?.Name;
            var admin = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == emailOrName || t.HoTen == emailOrName);

            if (admin != null && !string.IsNullOrEmpty(noiDung))
            {
                var tb = new ThongBao
                {
                    NoiDung = noiDung,
                    NgayTao = DateTime.Now,
                    Id_TK = admin.Id_TK
                };
                _context.ThongBao.Add(tb);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("TinTuc");
        }

        // 2. ADMIN TICK CHỌN NGÔI SAO "ĐÁNH GIÁ NỔI BẬT"
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ToggleNoiBat(int id)
        {
            var dg = await _context.DanhGia.FindAsync(id);
            if (dg != null)
            {
                // Nếu đang bật thì tắt, đang tắt thì bật
                dg.IsNoiBat = !dg.IsNoiBat;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("TinTuc");
        }

        // 3. ADMIN TRẢ LỜI ĐÁNH GIÁ CỦA KHÁCH
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ReplyDanhGia(int id, string adminReply)
        {
            var dg = await _context.DanhGia.FindAsync(id);
            if (dg != null)
            {
                dg.AdminReply = adminReply;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("TinTuc");
        }

        // TRANG LIÊN HỆ
        [HttpGet]
        public IActionResult LienHe()
        {
            return View();
        }

        // HÀM NHẬN DỮ LIỆU TỪ FORM LIÊN HỆ
        [HttpPost]
        public async Task<IActionResult> SubmitGopY(string hoTen, string soDienThoai, string email, string noiDung)
        {
            try
            {
                var gopY = new GopY
                {
                    HoTen = hoTen,
                    SoDienThoai = soDienThoai,
                    Email = email,
                    NoiDung = noiDung,
                    NgayGui = DateTime.Now,
                    DaDoc = false // Mặc định là Admin chưa đọc
                };

                _context.GopY.Add(gopY);
                await _context.SaveChangesAsync();

                // Gửi thông báo thành công ra màn hình cho khách hàng biết
                TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi góp ý. Chúng tôi sẽ ghi nhận và phản hồi sớm nhất!";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra trong quá trình gửi. Vui lòng thử lại sau.";
            }

            return RedirectToAction("LienHe");
        }
    }

    // --- CÁC CLASS HỖ TRỢ NHẬN DỮ LIỆU ĐẶT BÀN TỪ AJAX ---
    public class BookingRequest
    {
        public required string NgayDat { get; set; }
        public required string GioDat { get; set; }
        public int SoNguoi { get; set; }
        public required List<BookingItem> Items { get; set; }
    }

    public class BookingItem
    {
        public int IdMonAn { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
    }
}