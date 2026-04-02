using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WebBoTo.Data;
using WebBoTo.Models;

namespace WebBoTo.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string matKhau)
        {
            // Băm mật khẩu người dùng nhập vào để so sánh với chuỗi đã mã hóa trong DB
            string hashedPassword = HashPassword(matKhau, email);

            var user = await _context.TaiKhoan.FirstOrDefaultAsync(u => u.Email == email && u.MatKhau == hashedPassword);
            if (user != null)
            {
                // ====================================================
                // 1. CHẶN TÀI KHOẢN BỊ KHÓA (TRANGTHAI == FALSE)
                // ====================================================
                if (user.TrangThai == false)
                {
                    ViewBag.Error = "Tài khoản của bạn đã bị khóa do vi phạm. Vui lòng liên hệ Admin!";
                    return View();
                }

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.HoTen),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.VaiTro)
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

                // KIỂM TRA MẬT KHẨU TẠM THỜI
                if (matKhau.StartsWith("BOTOCHINHCU@"))
                {
                    TempData["WarningMessage"] = "Bạn đang sử dụng mật khẩu tạm thời. Vui lòng tạo mật khẩu mới để bảo mật tài khoản!";
                    return RedirectToAction("DatLaiMatKhau");
                }
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Sai email hoặc mật khẩu";
            return View();
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(TaiKhoan model)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.TaiKhoan.AnyAsync(u => u.Email == model.Email);
                if (exists)
                {
                    ViewBag.Error = "Email đã tồn tại";
                    return View(model);
                }

                // Mã hóa (Hash) mật khẩu trước khi lưu vào CSDL
                model.MatKhau = HashPassword(model.MatKhau, model.Email);
                model.VaiTro = "Customer";
                model.TrangThai = true; // Mặc định tài khoản mới tạo là không bị khóa

                _context.TaiKhoan.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // 1. HIỂN THỊ TRANG THÔNG TIN TÀI KHOẢN
        [HttpGet]
        public async Task<IActionResult> ThongTinTaiKhoan()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");

            var emailOrName = User.Identity.Name;
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == emailOrName || t.HoTen == emailOrName);

            if (tk == null) return RedirectToAction("Login");

            return View(tk);
        }

        // 2. XỬ LÝ LƯU CẬP NHẬT THÔNG TIN VÀ AVATAR
        [HttpPost]
        public async Task<IActionResult> CapNhatTaiKhoan(int id, string hoTen, string soDienThoai, IFormFile avatarFile)
        {
            var tk = await _context.TaiKhoan.FindAsync(id);
            if (tk != null)
            {
                tk.HoTen = hoTen;
                tk.SoDienThoai = soDienThoai;

                // Xử lý upload Avatar mới
                if (avatarFile != null && avatarFile.Length > 0)
                {
                    // Tạo thư mục avatars nếu chưa có
                    string uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/avatars");
                    if (!Directory.Exists(uploadFolder))
                    {
                        Directory.CreateDirectory(uploadFolder);
                    }

                    // Lưu file
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(avatarFile.FileName);
                    string filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await avatarFile.CopyToAsync(stream);
                    }

                    tk.Avatar = fileName; // Lưu tên file vào Database
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            }

            return RedirectToAction("ThongTinTaiKhoan");
        }

        // --- TÍNH NĂNG QUÊN MẬT KHẨU ---

        [HttpGet]
        public IActionResult QuenMatKhau()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> QuenMatKhau(string hoTen, string email)
        {
            // 1. Kiểm tra xem có tài khoản nào khớp Họ tên và Email không
            var user = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == email && t.HoTen == hoTen);
            if (user == null)
            {
                ViewBag.Error = "Thông tin Họ Tên hoặc Email không chính xác!";
                return View();
            }

            // Tạo mật khẩu tạm bắt đầu bằng chữ TEMP@
            string newPassword = "BOTOCHINHCU@" + Guid.NewGuid().ToString().Substring(0, 6);

            // 3. Mã hóa mật khẩu mới và lưu vào database
            user.MatKhau = HashPassword(newPassword, email);
            await _context.SaveChangesAsync();

            // 4. Gửi email chứa mật khẩu mới
            string subject = "Cấp lại mật khẩu - Bò Tơ Chính Cư";
            string body = $"Chào <b>{hoTen}</b>,<br><br>" +
                          $"Hệ thống đã nhận được yêu cầu cấp lại mật khẩu của bạn.<br>" +
                          $"Mật khẩu mới của bạn là: <b style='color:red; font-size:18px;'>{newPassword}</b><br><br>" +
                          $"Vui lòng đăng nhập bằng mật khẩu này và đổi lại mật khẩu mới để đảm bảo an toàn.<br><br>" +
                          $"Trân trọng,<br>Bò Tơ Chính Cư.";
            try
            {
                await SendEmailAsync(email, subject, body);
                TempData["SuccessMessage"] = "Mật khẩu mới đã được gửi vào Email của bạn. Vui lòng kiểm tra Hộp thư đến (hoặc Spam).";
                return RedirectToAction("Login");
            }
            catch (Exception)
            {
                ViewBag.Error = "Có lỗi xảy ra khi gửi email. Vui lòng kiểm tra lại kết nối mạng.";
                return View();
            }
        }

        // --- TÍNH NĂNG ĐỔI MẬT KHẨU BẮT BUỘC ---
        [HttpGet]
        public IActionResult DatLaiMatKhau()
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DatLaiMatKhau(string matKhauCu, string matKhauMoi, string xacNhanMatKhau)
        {
            if (!User.Identity.IsAuthenticated) return RedirectToAction("Login");

            if (matKhauMoi != xacNhanMatKhau)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View();
            }

            var userName = User.Identity.Name;
            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == userName || t.HoTen == userName);

            if (tk != null)
            {
                // Kiểm tra mật khẩu cũ (hoặc mật khẩu tạm) có gõ đúng không
                string hashedOld = HashPassword(matKhauCu, tk.Email);
                if (tk.MatKhau != hashedOld)
                {
                    ViewBag.Error = "Mật khẩu hiện tại không đúng!";
                    return View();
                }

                // Mã hóa và lưu mật khẩu mới vào DB
                tk.MatKhau = HashPassword(matKhauMoi, tk.Email);
                await _context.SaveChangesAsync();

                // Đổi thành công thì đá về trang Thông tin cá nhân
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Tài khoản của bạn đã được bảo mật an toàn.";
                return RedirectToAction("ThongTinTaiKhoan");
            }

            return RedirectToAction("Login");
        }

        // =============================================
        // CÁC HÀM TIỆN ÍCH DÙNG CHUNG
        // =============================================

        // 1. HÀM BĂM MẬT KHẨU
        private string HashPassword(string password, string email)
        {
            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(email.ToLower())))
            {
                var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashBytes);
            }
        }

        // 2. HÀM GỬI EMAIL TỰ ĐỘNG
        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            string fromEmail = "dzminh344@gmail.com";
            string fromPassword = "uaxcpqsvdadhhzvc"; // App Password

            MailMessage message = new MailMessage(fromEmail, toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
            {
                smtp.Credentials = new NetworkCredential(fromEmail, fromPassword);
                smtp.EnableSsl = true;
                await smtp.SendMailAsync(message);
            }
        }

        // =============================================
        // ĐĂNG NHẬP BẰNG GOOGLE
        // =============================================

        public IActionResult LoginWithGoogle()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleResponse") };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded) return RedirectToAction("Login");

            var claims = result.Principal.Identities.FirstOrDefault()?.Claims;
            var email = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims?.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(email)) return RedirectToAction("Login");

            var tk = await _context.TaiKhoan.FirstOrDefaultAsync(t => t.Email == email);

            // ====================================================
            // 2. CHẶN ĐĂNG NHẬP GOOGLE NẾU TÀI KHOẢN BỊ KHÓA
            // ====================================================
            if (tk != null && tk.TrangThai == false)
            {
                // Xóa phiên đăng nhập hờ của Google đi
                await HttpContext.SignOutAsync(GoogleDefaults.AuthenticationScheme);

                // Dùng TempData vì lệnh RedirectToAction không nhận ViewBag
                TempData["ErrorMessage"] = "Tài khoản của bạn đã bị khóa do vi phạm. Vui lòng liên hệ Admin!";
                return RedirectToAction("Login");
            }

            if (tk == null)
            {
                tk = new TaiKhoan
                {
                    Email = email,
                    HoTen = name ?? "Khách Hàng",
                    MatKhau = "",
                    VaiTro = "User",
                    SoDienThoai = "",
                    TrangThai = true // Tài khoản mới tạo qua Google mặc định là mở khóa
                };
                _context.TaiKhoan.Add(tk);
                await _context.SaveChangesAsync();
            }

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, tk.Email),
                new Claim("HoTen", tk.HoTen),
                new Claim(ClaimTypes.Role, "Customer")
            };

            var identity = new ClaimsIdentity(userClaims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            if (string.IsNullOrWhiteSpace(tk.SoDienThoai))
            {
                TempData["WarningMessage"] = "Vui lòng cập nhật Số điện thoại để hệ thống có thể liên hệ xác nhận khi bạn đặt bàn/đặt tiệc nhé!";
                return RedirectToAction("ThongTinTaiKhoan", "Account");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}