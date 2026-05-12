using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DACS_Nhom19.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TaiKhoanController : Controller
    {
        // DbContext để thao tác với database
        private readonly ApplicationDbContext _context;

        public TaiKhoanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // 1. DANH SÁCH TÀI KHOẢN
        // =========================================
        public async Task<IActionResult> Index(string keyword, int? maVaiTro, string trangThai)
        {
            var query = _context.TaiKhoans
                .Include(x => x.MaVaiTroNavigation)
                .AsQueryable();

            // Tìm kiếm theo tên đăng nhập hoặc tên hiển thị
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.TenDangNhap.Contains(keyword) ||
                    x.HoTenHienThi.Contains(keyword));
            }

            // Lọc theo vai trò
            if (maVaiTro.HasValue)
            {
                query = query.Where(x => x.MaVaiTro == maVaiTro.Value);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            ViewBag.Keyword = keyword;
            ViewBag.MaVaiTro = maVaiTro;
            ViewBag.TrangThai = trangThai;

            ViewBag.VaiTroList = new SelectList(
                await _context.VaiTros.OrderBy(x => x.TenVaiTro).ToListAsync(),
                "MaVaiTro",
                "TenVaiTro",
                maVaiTro
            );

            var data = await query
                .OrderBy(x => x.TenDangNhap)
                .ToListAsync();

            return View(data);
        }

        // =========================================
        // 2. CHI TIẾT TÀI KHOẢN
        // =========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // =========================================
        // 3. HIỂN THỊ FORM THÊM
        // =========================================
        public async Task<IActionResult> Create()
        {
            await LoadVaiTroDropdown();
            LoadTrangThaiDropdown();
            return View();
        }

        // =========================================
        // 4. XỬ LÝ THÊM
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TenDangNhap,MatKhau,HoTenHienThi,MaVaiTro,TrangThai")] TaiKhoan taiKhoan)
        {
            ModelState.Remove("MaVaiTroNavigation");

            await ValidateTaiKhoan(taiKhoan);

            if (!ModelState.IsValid)
            {
                await LoadVaiTroDropdown(taiKhoan.MaVaiTro);
                LoadTrangThaiDropdown();
                return View(taiKhoan);
            }

            _context.TaiKhoans.Add(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Thêm tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // 5. HIỂN THỊ FORM SỬA
        // =========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            await LoadVaiTroDropdown(taiKhoan.MaVaiTro);
            LoadTrangThaiDropdown();
            return View(taiKhoan);
        }

        // =========================================
        // 6. XỬ LÝ SỬA
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTenHienThi,MaVaiTro,TrangThai,NgayTao,LanDangNhapCuoi")] TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.MaTaiKhoan) return NotFound();

            ModelState.Remove("MaVaiTroNavigation");

            await ValidateTaiKhoan(taiKhoan, taiKhoan.MaTaiKhoan);

            if (!ModelState.IsValid)
            {
                await LoadVaiTroDropdown(taiKhoan.MaVaiTro);
                LoadTrangThaiDropdown();
                return View(taiKhoan);
            }

            try
            {
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật tài khoản thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaiKhoanExists(taiKhoan.MaTaiKhoan))
                    return NotFound();
                else
                    throw;
            }
        }

        // =========================================
        // 7. HIỂN THỊ FORM XÓA
        // =========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoans
                .Include(x => x.MaVaiTroNavigation)
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (taiKhoan == null) return NotFound();

            return View(taiKhoan);
        }

        // =========================================
        // 8. XỬ LÝ XÓA
        // =========================================
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

            // Kiểm tra tài khoản có đang được gắn cho nhân viên không
            var nhanVienDangDung = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == id);

            if (nhanVienDangDung != null)
            {
                TempData["Error"] = $"Không thể xóa vì tài khoản này đang được gắn cho nhân viên '{nhanVienDangDung.HoTen}'. Hãy bỏ liên kết ở nhân viên trước.";
                return RedirectToAction(nameof(Index));
            }

            _context.TaiKhoans.Remove(taiKhoan);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Xóa tài khoản thành công.";
            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // 9. KIỂM TRA TỒN TẠI
        // =========================================
        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoans.Any(x => x.MaTaiKhoan == id);
        }

        // =========================================
        // 10. KIỂM TRA DỮ LIỆU
        // =========================================
        private async Task ValidateTaiKhoan(TaiKhoan taiKhoan, int? currentId = null)
        {
            // Không cho trùng tên đăng nhập
            bool isDuplicate = await _context.TaiKhoans.AnyAsync(x =>
                x.TenDangNhap == taiKhoan.TenDangNhap &&
                x.MaTaiKhoan != currentId);

            if (isDuplicate)
            {
                ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
            }

            // Họ tên hiển thị không được để trống
            if (string.IsNullOrWhiteSpace(taiKhoan.HoTenHienThi))
            {
                ModelState.AddModelError("HoTenHienThi", "Họ tên hiển thị không được để trống.");
            }

            // Mật khẩu không được quá ngắn
            if (string.IsNullOrWhiteSpace(taiKhoan.MatKhau) || taiKhoan.MatKhau.Length < 6)
            {
                ModelState.AddModelError("MatKhau", "Mật khẩu phải có ít nhất 6 ký tự.");
            }
        }

        // =========================================
        // 11. NẠP DROPDOWN VAI TRÒ
        // =========================================
        private async Task LoadVaiTroDropdown(int? selectedId = null)
        {
            ViewBag.MaVaiTro = new SelectList(
                await _context.VaiTros.OrderBy(x => x.TenVaiTro).ToListAsync(),
                "MaVaiTro",
                "TenVaiTro",
                selectedId
            );
        }

        // =========================================
        // 12. NẠP DROPDOWN TRẠNG THÁI
        // =========================================
        private void LoadTrangThaiDropdown()
        {
            ViewBag.TrangThaiList = new SelectList(new List<string>
            {
                "Hoạt động",
                "Khóa"
            });
        }
    }
}