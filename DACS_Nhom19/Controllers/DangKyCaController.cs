using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using DACS_Nhom19.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DACS_Nhom19.Controllers
{
    [Authorize(Roles = "Admin,Quản lý,Nhân viên")]
    public class DangKyCaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DangKyCaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string keyword, string ngayLam, string trangThai)
        {
            var query = _context.DangKyCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .Include(x => x.NguoiDuyetNavigation)
                .AsQueryable();

            if (IsNhanVien())
            {
                var currentNhanVienId = await GetCurrentNhanVienIdAsync();
                if (currentNhanVienId == null) return Forbid();
                query = query.Where(x => x.MaNhanVien == currentNhanVienId.Value);
            }

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaNhanVienNavigation.HoTen.Contains(keyword) ||
                    x.MaNhanVienNavigation.MaNhanVienCode.Contains(keyword) ||
                    x.MaCaNavigation.TenCa.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(ngayLam) && DateOnly.TryParse(ngayLam, out var dateValue))
                query = query.Where(x => x.NgayLam == dateValue);

            if (!string.IsNullOrWhiteSpace(trangThai))
                query = query.Where(x => x.TrangThai == trangThai);

            ViewBag.Keyword = keyword;
            ViewBag.NgayLam = ngayLam;
            ViewBag.TrangThai = trangThai;

            var data = await query.OrderByDescending(x => x.NgayDangKy).ToListAsync();
            return View(data);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dangKyCa = await _context.DangKyCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .Include(x => x.NguoiDuyetNavigation)
                .FirstOrDefaultAsync(x => x.MaDangKy == id);

            if (dangKyCa == null) return NotFound();

            if (IsNhanVien())
            {
                var currentNhanVienId = await GetCurrentNhanVienIdAsync();
                if (currentNhanVienId == null || dangKyCa.MaNhanVien != currentNhanVienId.Value)
                    return Forbid();
            }

            return View(dangKyCa);
        }

        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            var vm = new DangKyCaFormViewModel
            {
                NgayLam = DateOnly.FromDateTime(DateTime.Today)
            };

            if (IsNhanVien())
            {
                var currentNhanVienId = await GetCurrentNhanVienIdAsync();
                if (currentNhanVienId == null) return Forbid();

                vm.MaNhanVien = currentNhanVienId.Value;

                var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == currentNhanVienId.Value);
                ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                ViewBag.CurrentNhanVienId = currentNhanVienId.Value;
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DangKyCaFormViewModel model)
        {
            if (IsNhanVien())
            {
                var currentNhanVienId = await GetCurrentNhanVienIdAsync();
                if (currentNhanVienId == null) return Forbid();
                model.MaNhanVien = currentNhanVienId.Value;
            }

            if (!ModelState.IsValid)
            {
                await PrepareCreateEditView(model);
                return View(model);
            }

            bool isDuplicate = await _context.DangKyCas.AnyAsync(x =>
                x.MaNhanVien == model.MaNhanVien &&
                x.MaCa == model.MaCa &&
                x.NgayLam == model.NgayLam);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Bạn đã đăng ký ca này trong ngày đã chọn.");
                await PrepareCreateEditView(model);
                return View(model);
            }

            var loiTrungGio = await CheckDangKyCaBiTrungGio(model.MaNhanVien, model.MaCa, model.NgayLam);
            if (!string.IsNullOrEmpty(loiTrungGio))
            {
                ModelState.AddModelError("", loiTrungGio);
                await PrepareCreateEditView(model);
                return View(model);
            }

            var dangKyCa = new DangKyCa
            {
                MaNhanVien = model.MaNhanVien,
                MaCa = model.MaCa,
                NgayLam = model.NgayLam,
                GhiChu = model.GhiChu,
                TrangThai = "Chờ duyệt",
                NgayDangKy = DateTime.Now
            };

            _context.DangKyCas.Add(dangKyCa);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đăng ký ca thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dangKyCa = await _context.DangKyCas.FindAsync(id);
            if (dangKyCa == null) return NotFound();

            if (IsNhanVien())
            {
                var currentNhanVienId = await GetCurrentNhanVienIdAsync();
                if (currentNhanVienId == null || dangKyCa.MaNhanVien != currentNhanVienId.Value)
                    return Forbid();
                if (dangKyCa.TrangThai != "Chờ duyệt") return Forbid();
            }

            var vm = new DangKyCaFormViewModel
            {
                MaNhanVien = dangKyCa.MaNhanVien,
                MaCa = dangKyCa.MaCa,
                NgayLam = dangKyCa.NgayLam,
                GhiChu = dangKyCa.GhiChu
            };

            await PrepareCreateEditView(vm);
            ViewBag.MaDangKy = dangKyCa.MaDangKy;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DangKyCaFormViewModel model)
        {
            var dangKyCa = await _context.DangKyCas.FindAsync(id);
            if (dangKyCa == null) return NotFound();

            if (IsNhanVien())
            {
                var currentNhanVienId = await GetCurrentNhanVienIdAsync();
                if (currentNhanVienId == null || dangKyCa.MaNhanVien != currentNhanVienId.Value)
                    return Forbid();
                if (dangKyCa.TrangThai != "Chờ duyệt") return Forbid();
                model.MaNhanVien = currentNhanVienId.Value;
            }

            if (!ModelState.IsValid)
            {
                await PrepareCreateEditView(model);
                ViewBag.MaDangKy = id;
                return View(model);
            }

            bool isDuplicate = await _context.DangKyCas.AnyAsync(x =>
                x.MaNhanVien == model.MaNhanVien &&
                x.MaCa == model.MaCa &&
                x.NgayLam == model.NgayLam &&
                x.MaDangKy != id);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Đã tồn tại đăng ký giống như vậy.");
                await PrepareCreateEditView(model);
                ViewBag.MaDangKy = id;
                return View(model);
            }

            var loiTrungGio = await CheckDangKyCaBiTrungGio(model.MaNhanVien, model.MaCa, model.NgayLam, id);
            if (!string.IsNullOrEmpty(loiTrungGio))
            {
                ModelState.AddModelError("", loiTrungGio);
                await PrepareCreateEditView(model);
                ViewBag.MaDangKy = id;
                return View(model);
            }

            dangKyCa.MaNhanVien = model.MaNhanVien;
            dangKyCa.MaCa = model.MaCa;
            dangKyCa.NgayLam = model.NgayLam;
            dangKyCa.GhiChu = model.GhiChu;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Cập nhật đăng ký ca thành công.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dangKyCa = await _context.DangKyCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .FirstOrDefaultAsync(x => x.MaDangKy == id);

            if (dangKyCa == null) return NotFound();

            if (IsNhanVien())
            {
                var currentNhanVienId = await GetCurrentNhanVienIdAsync();
                if (currentNhanVienId == null || dangKyCa.MaNhanVien != currentNhanVienId.Value)
                    return Forbid();
                if (dangKyCa.TrangThai != "Chờ duyệt") return Forbid();
            }

            return View(dangKyCa);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dangKyCa = await _context.DangKyCas.FindAsync(id);
            if (dangKyCa == null) return NotFound();

            if (IsNhanVien())
            {
                var currentNhanVienId = await GetCurrentNhanVienIdAsync();
                if (currentNhanVienId == null || dangKyCa.MaNhanVien != currentNhanVienId.Value)
                    return Forbid();
                if (dangKyCa.TrangThai != "Chờ duyệt") return Forbid();
            }

            _context.DangKyCas.Remove(dangKyCa);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa đăng ký ca thành công.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> Duyet(int id)
        {
            var dangKyCa = await _context.DangKyCas
                .Include(x => x.MaCaNavigation)
                .FirstOrDefaultAsync(x => x.MaDangKy == id);

            if (dangKyCa == null) return NotFound();

            if (dangKyCa.TrangThai != "Chờ duyệt")
            {
                TempData["Error"] = "Đăng ký này đã được xử lý trước đó.";
                return RedirectToAction(nameof(Index));
            }

            var phanCongKhac = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x =>
                    x.MaNhanVien == dangKyCa.MaNhanVien &&
                    x.NgayLam == dangKyCa.NgayLam &&
                    x.TrangThai != "Đã hủy")
                .ToListAsync();

            foreach (var item in phanCongKhac)
            {
                var caCu = item.MaCaNavigation;
                var caMoi = dangKyCa.MaCaNavigation;

                bool biChongGio = caMoi.GioBatDau < caCu.GioKetThuc && caCu.GioBatDau < caMoi.GioKetThuc;
                if (biChongGio)
                {
                    TempData["Error"] = $"Không thể duyệt vì bị trùng giờ với ca '{caCu.TenCa}' đã được phân công.";
                    return RedirectToAction(nameof(Index));
                }
            }

            var nguoiDuyetId = GetCurrentTaiKhoanId();

            var phanCong = new PhanCongCa
            {
                MaNhanVien = dangKyCa.MaNhanVien,
                MaCa = dangKyCa.MaCa,
                NgayLam = dangKyCa.NgayLam,
                TrangThai = "Đã phân công",
                GhiChu = "Tạo từ đăng ký ca",
                NgayTao = DateTime.Now,
                NguoiTao = nguoiDuyetId
            };

            _context.PhanCongCas.Add(phanCong);

            dangKyCa.TrangThai = "Đã duyệt";
            dangKyCa.NgayDuyet = DateTime.Now;
            dangKyCa.NguoiDuyet = nguoiDuyetId;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Duyệt đăng ký ca thành công.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> TuChoi(int id, string? lyDo)
        {
            var dangKyCa = await _context.DangKyCas.FindAsync(id);
            if (dangKyCa == null) return NotFound();

            if (dangKyCa.TrangThai != "Chờ duyệt")
            {
                TempData["Error"] = "Đăng ký này đã được xử lý trước đó.";
                return RedirectToAction(nameof(Index));
            }

            dangKyCa.TrangThai = "Từ chối";
            dangKyCa.NgayDuyet = DateTime.Now;
            dangKyCa.NguoiDuyet = GetCurrentTaiKhoanId();

            if (!string.IsNullOrWhiteSpace(lyDo))
            {
                dangKyCa.GhiChu = string.IsNullOrWhiteSpace(dangKyCa.GhiChu)
                    ? $"[Từ chối] {lyDo}"
                    : $"{dangKyCa.GhiChu} | [Từ chối] {lyDo}";

                if (dangKyCa.GhiChu.Length > 255)
                    dangKyCa.GhiChu = dangKyCa.GhiChu.Substring(0, 255);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã từ chối đăng ký ca.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PrepareCreateEditView(DangKyCaFormViewModel model)
        {
            await LoadDropdowns(model.MaNhanVien, model.MaCa);

            if (IsNhanVien())
            {
                var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == model.MaNhanVien);
                ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                ViewBag.CurrentNhanVienId = model.MaNhanVien;
            }
        }

        private async Task LoadDropdowns(int? selectedNhanVien = null, int? selectedCa = null)
        {
            var nhanViens = await _context.NhanViens
                .Where(x => x.TrangThai != "Nghỉ việc")
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            var nhanVienData = nhanViens
                .Select(x => new { x.MaNhanVien, HienThi = x.MaNhanVienCode + " - " + x.HoTen })
                .ToList();

            ViewBag.MaNhanVien = new SelectList(nhanVienData, "MaNhanVien", "HienThi", selectedNhanVien);

            var caLams = await _context.CaLams
                .Where(x => x.TrangThai == "Hoạt động")
                .OrderBy(x => x.GioBatDau)
                .ToListAsync();

            var caLamData = caLams
                .Select(x => new { x.MaCa, HienThi = x.MaCaCode + " - " + x.TenCa })
                .ToList();

            ViewBag.MaCa = new SelectList(caLamData, "MaCa", "HienThi", selectedCa);
        }

        private async Task<string?> CheckDangKyCaBiTrungGio(int maNhanVien, int maCa, DateOnly ngayLam, int? currentId = null)
        {
            var caMoi = await _context.CaLams.FirstOrDefaultAsync(x => x.MaCa == maCa);
            if (caMoi == null) return "Ca làm không hợp lệ.";

            var dangKyKhac = await _context.DangKyCas
                .Include(x => x.MaCaNavigation)
                .Where(x =>
                    x.MaNhanVien == maNhanVien &&
                    x.NgayLam == ngayLam &&
                    x.MaDangKy != currentId &&
                    x.TrangThai != "Từ chối")
                .ToListAsync();

            foreach (var item in dangKyKhac)
            {
                var caCu = item.MaCaNavigation;
                bool biChongGio = caMoi.GioBatDau < caCu.GioKetThuc && caCu.GioBatDau < caMoi.GioKetThuc;
                if (biChongGio)
                    return $"Ca đăng ký bị trùng giờ với ca '{caCu.TenCa}' trong cùng ngày.";
            }

            var phanCongKhac = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x =>
                    x.MaNhanVien == maNhanVien &&
                    x.NgayLam == ngayLam &&
                    x.TrangThai != "Đã hủy")
                .ToListAsync();

            foreach (var item in phanCongKhac)
            {
                var caCu = item.MaCaNavigation;
                bool biChongGio = caMoi.GioBatDau < caCu.GioKetThuc && caCu.GioBatDau < caMoi.GioKetThuc;
                if (biChongGio)
                    return $"Ca đăng ký bị trùng giờ với phân công '{caCu.TenCa}' đã có trong ngày.";
            }

            return null;
        }

        private int? GetCurrentTaiKhoanId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claim)) return null;
            if (int.TryParse(claim, out var id)) return id;
            return null;
        }

        private async Task<int?> GetCurrentNhanVienIdAsync()
        {
            var maTaiKhoan = GetCurrentTaiKhoanId();
            if (maTaiKhoan == null) return null;

            return await _context.NhanViens
                .Where(x => x.MaTaiKhoan == maTaiKhoan)
                .Select(x => (int?)x.MaNhanVien)
                .FirstOrDefaultAsync();
        }

        private bool IsNhanVien() => User.IsInRole("Nhân viên");
    }
}
