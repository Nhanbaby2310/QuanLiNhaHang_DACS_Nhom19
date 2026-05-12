using DACS_Nhom19.Data;
using DACS_Nhom19.Helpers;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DACS_Nhom19.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string keyword, int? maVaiTro, string trangThai)
        {
            var query = _context.TaiKhoans
                .Include(x => x.MaVaiTroNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.TenDangNhap.Contains(keyword) ||
                    x.HoTenHienThi.Contains(keyword));
            }

            if (maVaiTro.HasValue)
                query = query.Where(x => x.MaVaiTro == maVaiTro.Value);

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(x => x.TrangThai == trangThai);

            ViewBag.Keyword = keyword;
            ViewBag.MaVaiTro = maVaiTro;
            ViewBag.TrangThai = trangThai;

            ViewBag.VaiTroList = new SelectList(
                await _context.VaiTros.OrderBy(x => x.TenVaiTro).ToListAsync(),
                "MaVaiTro", "TenVaiTro", maVaiTro);

            var data = await query.OrderBy(x => x.TenDangNhap).ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        public async Task<IActionResult> Create()
        {
            await LoadVaiTroDropdown();
            LoadTrangThaiDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDangNhap,MatKhau,HoTenHienThi,MaVaiTro,TrangThai")] TaiKhoan taiKhoan)
        {
            ModelState.Remove("MaVaiTroNavigation");
            await ValidateTaiKhoan(taiKhoan, isCreate: true);

            if (!ModelState.IsValid)
            {
                await LoadVaiTroDropdown(taiKhoan.MaVaiTro);
                LoadTrangThaiDropdown();
                return View(taiKhoan);
            }

            taiKhoan.MatKhau = PasswordHelper.Hash(taiKhoan.MatKhau);

            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            var vm = new TaiKhoan
            {
                MaTaiKhoan = taiKhoan.MaTaiKhoan,
                TenDangNhap = taiKhoan.TenDangNhap,
                MatKhau = string.Empty,
                HoTenHienThi = taiKhoan.HoTenHienThi,
                MaVaiTro = taiKhoan.MaVaiTro,
                TrangThai = taiKhoan.TrangThai,
                NgayTao = taiKhoan.NgayTao,
                LanDangNhapCuoi = taiKhoan.LanDangNhapCuoi
            };

            await LoadVaiTroDropdown(vm.MaVaiTro);
            LoadTrangThaiDropdown();
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTenHienThi,MaVaiTro,TrangThai,NgayTao,LanDangNhapCuoi")] TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.MaTaiKhoan) return NotFound();

            ModelState.Remove("MaVaiTroNavigation");
            ModelState.Remove(nameof(TaiKhoan.MatKhau));

            await ValidateTaiKhoan(taiKhoan, currentId: id, isCreate: false);

            if (!ModelState.IsValid)
            {
                await LoadVaiTroDropdown(taiKhoan.MaVaiTro);
                LoadTrangThaiDropdown();
                return View(taiKhoan);
            }

            try
            {
                var db = await _context.TaiKhoans.FirstOrDefaultAsync(x => x.MaTaiKhoan == id);
                if (db == null) return NotFound();

                db.TenDangNhap = taiKhoan.TenDangNhap;
                db.HoTenHienThi = taiKhoan.HoTenHienThi;
                db.MaVaiTro = taiKhoan.MaVaiTro;
                db.TrangThai = taiKhoan.TrangThai;

                if (!string.IsNullOrWhiteSpace(taiKhoan.MatKhau))
                    db.MatKhau = PasswordHelper.Hash(taiKhoan.MatKhau);

                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaiKhoanExists(taiKhoan.MaTaiKhoan)) return NotFound();
                throw;
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null) return NotFound();
            return View(taiKhoan);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            var nhanVienDangDung = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaTaiKhoan == id);
            if (nhanVienDangDung != null)
            {
                TempData["Error"] = $"Không thể xóa vì tài khoản này đang gắn với nhân viên '{nhanVienDangDung.HoTen}'.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.TaiKhoans.Remove(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa tài khoản thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa do tài khoản đang được tham chiếu ở bảng khác.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TaiKhoanExists(int id) => _context.TaiKhoans.Any(x => x.MaTaiKhoan == id);

        private async Task ValidateTaiKhoan(TaiKhoan taiKhoan, int? currentId = null, bool isCreate = true)
        {
            bool isDuplicate = await _context.TaiKhoans.AnyAsync(x =>
                x.TenDangNhap == taiKhoan.TenDangNhap &&
                x.MaTaiKhoan != currentId);

            if (isDuplicate)
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");

            if (string.IsNullOrWhiteSpace(taiKhoan.HoTenHienThi))
                ModelState.AddModelError("HoTenHienThi", "Họ tên hiển thị không được để trống.");

            if (isCreate)
            {
                if (string.IsNullOrWhiteSpace(taiKhoan.MatKhau) || taiKhoan.MatKhau.Length < 6)
                    ModelState.AddModelError("MatKhau", "Mật khẩu phải có ít nhất 6 ký tự.");
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(taiKhoan.MatKhau) && taiKhoan.MatKhau.Length < 6)
                    ModelState.AddModelError("MatKhau", "Mật khẩu mới phải có ít nhất 6 ký tự.");
            }
        }

        private async Task LoadVaiTroDropdown(int? selectedId = null)
        {
            ViewBag.MaVaiTro = new SelectList(
                await _context.VaiTros.OrderBy(x => x.TenVaiTro).ToListAsync(),
                "MaVaiTro", "TenVaiTro", selectedId);
        }

        private void LoadTrangThaiDropdown()
        {
            ViewBag.TrangThaiList = new SelectList(new List<string> { "Hoạt động", "Khóa" });
        }
    }
}
