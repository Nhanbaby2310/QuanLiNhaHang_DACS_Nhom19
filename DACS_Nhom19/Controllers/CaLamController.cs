using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DACS_Nhom19.Controllers
{
    [Authorize(Roles = "Admin,Quản lý")]
    public class CaLamController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CaLamController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string keyword, string loaiCa, string trangThai)
        {
            var query = _context.CaLams.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaCaCode.Contains(keyword) ||
                    x.TenCa.Contains(keyword) ||
                    (x.GhiChu != null && x.GhiChu.Contains(keyword)));
            }

            if (!string.IsNullOrWhiteSpace(loaiCa))
                query = query.Where(x => x.LoaiCa == loaiCa);

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(x => x.TrangThai == trangThai);

            ViewBag.Keyword = keyword;
            ViewBag.LoaiCa = loaiCa;
            ViewBag.TrangThai = trangThai;

            var data = await query.OrderBy(x => x.GioBatDau).ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var caLam = await _context.CaLams.FirstOrDefaultAsync(x => x.MaCa == id);
            if (caLam == null) return NotFound();
            return View(caLam);
        }

        public IActionResult Create()
        {
            LoadDanhMuc();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCaCode,TenCa,GioBatDau,GioKetThuc,LoaiCa,SoLuongNhanVienToiThieu,SoLuongNhanVienToiDa,TrangThai,GhiChu")] CaLam caLam)
        {
            await ValidateCaLam(caLam);

            if (ModelState.IsValid)
            {
                _context.Add(caLam);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm ca làm thành công.";
                return RedirectToAction(nameof(Index));
            }

            LoadDanhMuc();
            return View(caLam);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var caLam = await _context.CaLams.FindAsync(id);
            if (caLam == null) return NotFound();

            LoadDanhMuc();
            return View(caLam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaCa,MaCaCode,TenCa,GioBatDau,GioKetThuc,LoaiCa,SoLuongNhanVienToiThieu,SoLuongNhanVienToiDa,TrangThai,GhiChu")] CaLam caLam)
        {
            if (id != caLam.MaCa) return NotFound();

            await ValidateCaLam(caLam, caLam.MaCa);

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(caLam);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật ca làm thành công.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CaLamExists(caLam.MaCa)) return NotFound();
                    throw;
                }
            }

            LoadDanhMuc();
            return View(caLam);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var caLam = await _context.CaLams.FirstOrDefaultAsync(x => x.MaCa == id);
            if (caLam == null) return NotFound();

            return View(caLam);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var caLam = await _context.CaLams.FindAsync(id);
            if (caLam == null)
            {
                TempData["Error"] = "Ca làm không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            bool dangDung = await _context.PhanCongCas.AnyAsync(x => x.MaCa == id)
                         || await _context.DangKyCas.AnyAsync(x => x.MaCa == id);

            if (dangDung)
            {
                TempData["Error"] = $"Không thể xóa ca '{caLam.TenCa}' vì đã được dùng trong phân công hoặc đăng ký. Hãy đổi trạng thái sang 'Ngưng' thay vì xóa.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.CaLams.Remove(caLam);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa ca làm thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa ca làm do đang được tham chiếu.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CaLamExists(int id) => _context.CaLams.Any(e => e.MaCa == id);

        private async Task ValidateCaLam(CaLam caLam, int? currentId = null)
        {
            if (await _context.CaLams.AnyAsync(x => x.MaCaCode == caLam.MaCaCode && x.MaCa != currentId))
                ModelState.AddModelError("MaCaCode", "Mã ca đã tồn tại.");

            if (await _context.CaLams.AnyAsync(x => x.TenCa == caLam.TenCa && x.MaCa != currentId))
                ModelState.AddModelError("TenCa", "Tên ca đã tồn tại.");

            if (caLam.GioKetThuc <= caLam.GioBatDau)
                ModelState.AddModelError("GioKetThuc", "Giờ kết thúc phải lớn hơn giờ bắt đầu.");

            if (caLam.SoLuongNhanVienToiThieu < 1)
                ModelState.AddModelError("SoLuongNhanVienToiThieu", "Số lượng tối thiểu phải >= 1.");

            if (caLam.SoLuongNhanVienToiDa < caLam.SoLuongNhanVienToiThieu)
                ModelState.AddModelError("SoLuongNhanVienToiDa", "Số lượng tối đa phải lớn hơn hoặc bằng số lượng tối thiểu.");
        }

        private void LoadDanhMuc()
        {
            ViewBag.LoaiCaList = new SelectList(new List<string> { "Chuẩn", "Đặc biệt" });
            ViewBag.TrangThaiList = new SelectList(new List<string> { "Hoạt động", "Ngưng" });
        }
    }
}
