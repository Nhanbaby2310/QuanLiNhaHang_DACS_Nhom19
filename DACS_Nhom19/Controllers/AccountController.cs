using DACS_Nhom19.Data;
using DACS_Nhom19.Helpers;
using DACS_Nhom19.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DACS_Nhom19.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Hiển thị form đăng nhập
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Xử lý đăng nhập
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
                return View(model);

            // Tìm theo tên đăng nhập + trạng thái
            var taiKhoan = await _context.TaiKhoans
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefaultAsync(x =>
                    x.TenDangNhap == model.TenDangNhap &&
                    x.TrangThai == "Hoạt động");

            if (taiKhoan == null || !PasswordHelper.Verify(model.MatKhau, taiKhoan.MatKhau))
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Nếu DB đang lưu plain text, tự rehash bằng PBKDF2 để migrate dần
            if (!PasswordHelper.IsHashed(taiKhoan.MatKhau))
            {
                taiKhoan.MatKhau = PasswordHelper.Hash(model.MatKhau);
            }

            // Cập nhật lần đăng nhập cuối
            taiKhoan.LanDangNhapCuoi = DateTime.Now;
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, taiKhoan.MaTaiKhoan.ToString()),
                new Claim(ClaimTypes.Name, taiKhoan.HoTenHienThi),
                new Claim("TenDangNhap", taiKhoan.TenDangNhap),
                new Claim(ClaimTypes.Role, taiKhoan.MaVaiTroNavigation.TenVaiTro)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                });

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // Đăng xuất
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // Không đủ quyền
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
