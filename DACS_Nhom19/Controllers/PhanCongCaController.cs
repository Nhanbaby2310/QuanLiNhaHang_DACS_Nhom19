using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DACS_Nhom19.Controllers
{
    [Authorize(Roles = "Admin,Quản lý")]
    public class PhanCongCaController : Controller
    {
        // DbContext để làm việc với database
        private readonly ApplicationDbContext _context;

        public PhanCongCaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // 1. DANH SÁCH PHÂN CÔNG CA
        // Có tìm kiếm + lọc
        // =========================================
        public async Task<IActionResult> Index(string keyword, string ngayLam, int? maCa, string trangThai)
        {
            var query = _context.PhanCongCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .AsQueryable();

            // Tìm theo mã nhân viên, họ tên, mã ca, tên ca
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaNhanVienNavigation.HoTen.Contains(keyword) ||
                    x.MaNhanVienNavigation.MaNhanVienCode.Contains(keyword) ||
                    x.MaCaNavigation.TenCa.Contains(keyword) ||
                    x.MaCaNavigation.MaCaCode.Contains(keyword));
            }

            // Lọc theo ngày làm
            if (!string.IsNullOrWhiteSpace(ngayLam) && DateOnly.TryParse(ngayLam, out var dateValue))
            {
                query = query.Where(x => x.NgayLam == dateValue);
            }

            // Lọc theo ca
            if (maCa.HasValue)
            {
                query = query.Where(x => x.MaCa == maCa.Value);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            ViewBag.Keyword = keyword;
            ViewBag.NgayLam = ngayLam;
            ViewBag.MaCa = maCa;
            ViewBag.TrangThai = trangThai;

            ViewBag.CaLamList = new SelectList(
                await _context.CaLams.OrderBy(x => x.GioBatDau).ToListAsync(),
                "MaCa",
                "TenCa",
                maCa
            );

            var data = await query
                .OrderBy(x => x.NgayLam)
                .ThenBy(x => x.MaCaNavigation.GioBatDau)
                .ThenBy(x => x.MaNhanVienNavigation.HoTen)
                .ToListAsync();

            return View(data);
        }

        // =========================================
        // 2. CHI TIẾT PHÂN CÔNG CA
        // =========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var phanCong = await _context.PhanCongCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .Include(x => x.NguoiTaoNavigation)
                .FirstOrDefaultAsync(x => x.MaPhanCong == id);

            if (phanCong == null) return NotFound();

            return View(phanCong);
        }

        // =========================================
        // 3. HIỂN THỊ FORM THÊM MỚI
        // =========================================
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View();
        }

        // =========================================
        // 4. XỬ LÝ THÊM MỚI
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNhanVien,MaCa,NgayLam,TrangThai,GhiChu")] PhanCongCa phanCongCa)
        {
            await ValidatePhanCongCa(phanCongCa);

            if (ModelState.IsValid)
            {
                _context.Add(phanCongCa);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm phân công ca thành công.";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns(phanCongCa.MaNhanVien, phanCongCa.MaCa);
            return View(phanCongCa);
        }

        // =========================================
        // 5. HIỂN THỊ FORM SỬA
        // =========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phanCongCa = await _context.PhanCongCas.FindAsync(id);
            if (phanCongCa == null) return NotFound();

            await LoadDropdowns(phanCongCa.MaNhanVien, phanCongCa.MaCa);
            return View(phanCongCa);
        }

        // =========================================
        // 6. XỬ LÝ CẬP NHẬT
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhanCong,MaNhanVien,MaCa,NgayLam,TrangThai,GhiChu,NgayTao,NguoiTao")] PhanCongCa phanCongCa)
        {
            if (id != phanCongCa.MaPhanCong)
                return NotFound();

            ModelState.Remove("MaNhanVienNavigation");
            ModelState.Remove("MaCaNavigation");
            ModelState.Remove("NguoiTaoNavigation");

            await ValidatePhanCongCa(phanCongCa, phanCongCa.MaPhanCong);

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(phanCongCa.MaNhanVien, phanCongCa.MaCa);
                return View(phanCongCa);
            }

            try
            {
                _context.Update(phanCongCa);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Cập nhật phân công ca thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PhanCongCaExists(phanCongCa.MaPhanCong))
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

            var phanCong = await _context.PhanCongCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .FirstOrDefaultAsync(x => x.MaPhanCong == id);

            if (phanCong == null) return NotFound();

            return View(phanCong);
        }

        // =========================================
        // 8. XỬ LÝ XÓA
        // =========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phanCong = await _context.PhanCongCas.FindAsync(id);

            if (phanCong != null)
            {
                _context.PhanCongCas.Remove(phanCong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa phân công ca thành công.";
            }

            return RedirectToAction(nameof(Index));
        }


        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> HoanThanh(int id)
        {
            var phanCong = await _context.PhanCongCas.FindAsync(id);
            if (phanCong == null) return NotFound();

            if (phanCong.TrangThai != "Hoàn thành" && phanCong.TrangThai != "Đã hủy")
            {
                phanCong.TrangThai = "Hoàn thành";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã cập nhật trạng thái hoàn thành.";
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // 9. KIỂM TRA TỒN TẠI
        // =========================================
        private bool PhanCongCaExists(int id)
        {
            return _context.PhanCongCas.Any(x => x.MaPhanCong == id);
        }

        // =========================================
        // 10. KIỂM TRA DỮ LIỆU
        // - Không trùng cùng nhân viên + cùng ca + cùng ngày
        // - Không chồng giờ trong cùng ngày
        // =========================================
        private async Task ValidatePhanCongCa(PhanCongCa phanCongCa, int? currentId = null)
        {
            // Không cho trùng cùng nhân viên + cùng ca + cùng ngày
            bool isDuplicate = await _context.PhanCongCas.AnyAsync(x =>
                x.MaNhanVien == phanCongCa.MaNhanVien &&
                x.MaCa == phanCongCa.MaCa &&
                x.NgayLam == phanCongCa.NgayLam &&
                x.MaPhanCong != currentId);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Nhân viên này đã được phân vào ca này trong ngày đã chọn.");
            }

            // Lấy ca mới để kiểm tra giờ
            var caMoi = await _context.CaLams.FirstOrDefaultAsync(x => x.MaCa == phanCongCa.MaCa);
            if (caMoi == null)
            {
                ModelState.AddModelError("MaCa", "Ca làm không hợp lệ.");
                return;
            }

            // Lấy các phân công khác cùng nhân viên, cùng ngày
            var danhSachCaDaPhan = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x =>
                    x.MaNhanVien == phanCongCa.MaNhanVien &&
                    x.NgayLam == phanCongCa.NgayLam &&
                    x.MaPhanCong != currentId &&
                    x.TrangThai != "Đã hủy")
                .ToListAsync();

            // Kiểm tra chồng giờ
            foreach (var item in danhSachCaDaPhan)
            {
                var caCu = item.MaCaNavigation;

                bool biChongGio =
                    caMoi.GioBatDau < caCu.GioKetThuc &&
                    caCu.GioBatDau < caMoi.GioKetThuc;

                if (biChongGio)
                {
                    ModelState.AddModelError("", $"Nhân viên đang bị chồng giờ với ca '{caCu.TenCa}' trong ngày đã chọn.");
                    break;
                }
            }
        }

        // =========================================
        // 11. NẠP DROPDOWN
        // =========================================
        private async Task LoadDropdowns(int? selectedNhanVien = null, int? selectedCa = null)
        {
            // Chỉ lấy nhân viên đang làm hoặc nghỉ phép
            var nhanViens = await _context.NhanViens
                .Where(x => x.TrangThai != "Nghỉ việc")
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            var nhanVienData = nhanViens.Select(x => new
            {
                x.MaNhanVien,
                HienThi = x.MaNhanVienCode + " - " + x.HoTen
            }).ToList();

            ViewBag.MaNhanVien = new SelectList(nhanVienData, "MaNhanVien", "HienThi", selectedNhanVien);

            // Chỉ lấy ca đang hoạt động
            var caLams = await _context.CaLams
                .Where(x => x.TrangThai == "Hoạt động")
                .OrderBy(x => x.GioBatDau)
                .ToListAsync();

            var caLamData = caLams.Select(x => new
            {
                x.MaCa,
                HienThi = x.MaCaCode + " - " + x.TenCa
            }).ToList();

            ViewBag.MaCa = new SelectList(caLamData, "MaCa", "HienThi", selectedCa);

            // Dropdown trạng thái
            ViewBag.TrangThaiList = new SelectList(new List<string>
            {
                "Đã phân công",
                "Đổi ca",
                "Nghỉ",
                "Đã hủy",
                "Hoàn thành"
            });
        }
    }
}