using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DACS_Nhom19.Controllers
{
    [Authorize(Roles = "Admin,Quản lý")]
    public class NhanVienController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NhanVienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DANH SÁCH
        public async Task<IActionResult> Index(string keyword, string loaiNhanVien, string trangThai)
        {
            var query = _context.NhanViens
                .Include(n => n.MaTaiKhoanNavigation)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(n =>
                    n.MaNhanVienCode.Contains(keyword) ||
                    n.HoTen.Contains(keyword) ||
                    n.SoDienThoai.Contains(keyword) ||
                    (n.ChucVu != null && n.ChucVu.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(loaiNhanVien))
            {
                query = query.Where(n => n.LoaiNhanVien == loaiNhanVien);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(n => n.TrangThai == trangThai);
            }

            ViewBag.Keyword = keyword;
            ViewBag.LoaiNhanVien = loaiNhanVien;
            ViewBag.TrangThai = trangThai;

            var data = await query
                .OrderBy(n => n.MaNhanVien)
                .ToListAsync();

            return View(data);
        }

        // CHI TIẾT
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens
                .Include(n => n.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // GET: CREATE
        public async Task<IActionResult> Create()
        {
            await LoadTaiKhoanDropdown();
            LoadDanhMuc();
            return View();
        }

        // POST: CREATE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNhanVienCode,HoTen,GioiTinh,NgaySinh,SoDienThoai,Email,DiaChi,ChucVu,LoaiNhanVien,NgayVaoLam,SoCaToiThieuTuan,SoGioToiThieuTuan,TrangThai,MaTaiKhoan")] NhanVien nhanVien)
        {
            ModelState.Remove("MaTaiKhoanNavigation");

            await ValidateNhanVien(nhanVien);

            if (ModelState.IsValid)
            {
                _context.Add(nhanVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm nhân viên thành công.";
                return RedirectToAction(nameof(Index));
            }

            await LoadTaiKhoanDropdown(nhanVien.MaTaiKhoan);
            LoadDanhMuc();
            return View(nhanVien);
        }

        // GET: EDIT
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null) return NotFound();

            await LoadTaiKhoanDropdown(nhanVien.MaTaiKhoan, nhanVien.MaNhanVien);
            LoadDanhMuc();

            return View(nhanVien);
        }

        // POST: EDIT
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaNhanVien,MaNhanVienCode,HoTen,GioiTinh,NgaySinh,SoDienThoai,Email,DiaChi,ChucVu,LoaiNhanVien,NgayVaoLam,SoCaToiThieuTuan,SoGioToiThieuTuan,TrangThai,MaTaiKhoan")] NhanVien nhanVien)
        {
            if (id != nhanVien.MaNhanVien) return NotFound();

            ModelState.Remove("MaTaiKhoanNavigation");

            await ValidateNhanVien(nhanVien, nhanVien.MaNhanVien);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(nhanVien);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật nhân viên thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NhanVienExists(nhanVien.MaNhanVien))
                        return NotFound();
                    else
                        throw;
                }
            }

            await LoadTaiKhoanDropdown(nhanVien.MaTaiKhoan, nhanVien.MaNhanVien);
            LoadDanhMuc();
            return View(nhanVien);
        }

        // GET: DELETE
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var nhanVien = await _context.NhanViens
                .Include(n => n.MaTaiKhoanNavigation)
                .FirstOrDefaultAsync(m => m.MaNhanVien == id);

            if (nhanVien == null) return NotFound();

            return View(nhanVien);
        }

        // POST: DELETE
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var nhanVien = await _context.NhanViens.FindAsync(id);
            if (nhanVien == null)
            {
                TempData["Error"] = "Nhân viên không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            bool dangDung = await _context.PhanCongCas.AnyAsync(x => x.MaNhanVien == id)
                         || await _context.DangKyCas.AnyAsync(x => x.MaNhanVien == id);

            if (dangDung)
            {
                TempData["Error"] = $"Không thể xóa nhân viên '{nhanVien.HoTen}' vì đã có phân công hoặc đăng ký ca. Hãy chuyển trạng thái sang 'Nghỉ việc' thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.NhanViens.Remove(nhanVien);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa nhân viên thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa nhân viên do đang được tham chiếu ở bảng khác.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool NhanVienExists(int id)
        {
            return _context.NhanViens.Any(e => e.MaNhanVien == id);
        }

        private async Task ValidateNhanVien(NhanVien nhanVien, int? currentId = null)
        {
            if (await _context.NhanViens.AnyAsync(x =>
                x.MaNhanVienCode == nhanVien.MaNhanVienCode &&
                x.MaNhanVien != currentId))
            {
                ModelState.AddModelError("MaNhanVienCode", "Mã nhân viên đã tồn tại.");
            }

            if (await _context.NhanViens.AnyAsync(x =>
                x.SoDienThoai == nhanVien.SoDienThoai &&
                x.MaNhanVien != currentId))
            {
                ModelState.AddModelError("SoDienThoai", "Số điện thoại đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(nhanVien.Email))
            {
                if (await _context.NhanViens.AnyAsync(x =>
                    x.Email == nhanVien.Email &&
                    x.MaNhanVien != currentId))
                {
                    ModelState.AddModelError("Email", "Email đã tồn tại.");
                }
            }

            if (nhanVien.MaTaiKhoan.HasValue)
            {
                if (await _context.NhanViens.AnyAsync(x =>
                    x.MaTaiKhoan == nhanVien.MaTaiKhoan &&
                    x.MaNhanVien != currentId))
                {
                    ModelState.AddModelError("MaTaiKhoan", "Tài khoản này đã được gắn cho nhân viên khác.");
                }
            }
        }

        private async Task LoadTaiKhoanDropdown(int? selectedId = null, int? currentNhanVienId = null)
        {
            var usedAccountIds = await _context.NhanViens
                .Where(x => x.MaTaiKhoan != null && x.MaNhanVien != currentNhanVienId)
                .Select(x => x.MaTaiKhoan!.Value)
                .ToListAsync();

            var taiKhoans = await _context.TaiKhoans
                .Where(t => t.TrangThai == "Hoạt động")
                .OrderBy(t => t.TenDangNhap)
                .ToListAsync();

            var data = taiKhoans
                .Where(t => !usedAccountIds.Contains(t.MaTaiKhoan) || t.MaTaiKhoan == selectedId)
                .Select(t => new
                {
                    t.MaTaiKhoan,
                    HienThi = t.TenDangNhap + " - " + t.HoTenHienThi
                })
                .ToList();

            ViewBag.MaTaiKhoan = new SelectList(data, "MaTaiKhoan", "HienThi", selectedId);
        }

        private void LoadDanhMuc()
        {
            ViewBag.GioiTinhList = new SelectList(new List<string> { "Nam", "Nữ" });
            ViewBag.LoaiNhanVienList = new SelectList(new List<string> { "Full-time", "Part-time" });
            ViewBag.TrangThaiList = new SelectList(new List<string> { "Đang làm", "Nghỉ phép", "Nghỉ việc" });
        }
    }
}