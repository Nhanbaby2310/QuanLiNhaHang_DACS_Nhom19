using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DACS_Nhom19.ViewModels;

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

        // Danh sách đăng ký ca
        public async Task<IActionResult> Index(string keyword, string ngayLam, string trangThai)
        {
            var query = _context.DangKyCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .Include(x => x.NguoiDuyetNavigation)
                .AsQueryable();

            // Nếu là nhân viên thì chỉ xem đăng ký của chính mình
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
            {
                query = query.Where(x => x.NgayLam == dateValue);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            ViewBag.Keyword = keyword;
            ViewBag.NgayLam = ngayLam;
            ViewBag.TrangThai = trangThai;

            var data = await query
                .OrderByDescending(x => x.NgayDangKy)
                .ToListAsync();

            return View(data);
        }

        // Chi tiết
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

        // Form thêm
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();

            var vm = new DangKyCaFormViewModel();

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

        // Xử lý thêm
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
                await LoadDropdowns(model.MaNhanVien, model.MaCa);

                if (IsNhanVien())
                {
                    var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == model.MaNhanVien);
                    ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                    ViewBag.CurrentNhanVienId = model.MaNhanVien;
                }

                return View(model);
            }

            // 1. Chặn trùng đúng cùng một ca
            bool isDuplicate = await _context.DangKyCas.AnyAsync(x =>
                x.MaNhanVien == model.MaNhanVien &&
                x.MaCa == model.MaCa &&
                x.NgayLam == model.NgayLam);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Bạn đã đăng ký đúng ca này trong ngày đã chọn.");

                await LoadDropdowns(model.MaNhanVien, model.MaCa);

                if (IsNhanVien())
                {
                    var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == model.MaNhanVien);
                    ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                    ViewBag.CurrentNhanVienId = model.MaNhanVien;
                }

                return View(model);
            }

            // 2. Chặn trùng giờ
            var loiTrungGio = await CheckDangKyCaBiTrungGio(model.MaNhanVien, model.MaCa, model.NgayLam);
            if (!string.IsNullOrEmpty(loiTrungGio))
            {
                ModelState.AddModelError("", loiTrungGio);

                await LoadDropdowns(model.MaNhanVien, model.MaCa);

                if (IsNhanVien())
                {
                    var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == model.MaNhanVien);
                    ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                    ViewBag.CurrentNhanVienId = model.MaNhanVien;
                }

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

        // Form sửa
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

                if (dangKyCa.TrangThai != "Chờ duyệt")
                    return Forbid();
            }

            var vm = new DangKyCaFormViewModel
            {
                MaNhanVien = dangKyCa.MaNhanVien,
                MaCa = dangKyCa.MaCa,
                NgayLam = dangKyCa.NgayLam,
                GhiChu = dangKyCa.GhiChu
            };

            await LoadDropdowns(vm.MaNhanVien, vm.MaCa);

            if (IsNhanVien())
            {
                var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == vm.MaNhanVien);
                ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                ViewBag.CurrentNhanVienId = vm.MaNhanVien;
            }

            ViewBag.MaDangKy = dangKyCa.MaDangKy;
            return View(vm);
        }

        // Xử lý sửa
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

                if (dangKyCa.TrangThai != "Chờ duyệt")
                    return Forbid();

                model.MaNhanVien = currentNhanVienId.Value;
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model.MaNhanVien, model.MaCa);

                if (IsNhanVien())
                {
                    var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == model.MaNhanVien);
                    ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                    ViewBag.CurrentNhanVienId = model.MaNhanVien;
                }

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

                await LoadDropdowns(model.MaNhanVien, model.MaCa);

                if (IsNhanVien())
                {
                    var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == model.MaNhanVien);
                    ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                    ViewBag.CurrentNhanVienId = model.MaNhanVien;
                }

                ViewBag.MaDangKy = id;
                return View(model);
            }

            var loiTrungGio = await CheckDangKyCaBiTrungGio(model.MaNhanVien, model.MaCa, model.NgayLam, id);
            if (!string.IsNullOrEmpty(loiTrungGio))
            {
                ModelState.AddModelError("", loiTrungGio);

                await LoadDropdowns(model.MaNhanVien, model.MaCa);

                if (IsNhanVien())
                {
                    var nv = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaNhanVien == model.MaNhanVien);
                    ViewBag.CurrentNhanVienText = nv != null ? $"{nv.MaNhanVienCode} - {nv.HoTen}" : "";
                    ViewBag.CurrentNhanVienId = model.MaNhanVien;
                }

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

        // Form xóa
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

                if (dangKyCa.TrangThai != "Chờ duyệt")
                    return Forbid();
            }

            return View(dangKyCa);
        }

        // Xử lý xóa
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

                if (dangKyCa.TrangThai != "Chờ duyệt")
                    return Forbid();
            }

            _context.DangKyCas.Remove(dangKyCa);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa đăng ký ca thành công.";

            return RedirectToAction(nameof(Index));
        }

        // Duyệt đăng ký ca
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> Duyet(int id)
        {
            var dangKyCa = await _context.DangKyCas
                .Include(x => x.MaCaNavigation)
                .FirstOrDefaultAsync(x => x.MaDangKy == id);

            if (dangKyCa == null) return NotFound();

            if (dangKyCa.TrangThai != "Chờ duyệt")
            {
                TempData["Success"] = "Đăng ký này đã được xử lý trước đó.";
                return RedirectToAction(nameof(Index));
            }

            // Kiểm tra chồng giờ với phân công đã có trước khi duyệt
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

                bool biChongGio =
                    caMoi.GioBatDau < caCu.GioKetThuc &&
                    caCu.GioBatDau < caMoi.GioKetThuc;

                if (biChongGio)
                {
                    TempData["Success"] = $"Không thể duyệt vì bị trùng giờ với ca '{caCu.TenCa}' đã được phân công.";
                    return RedirectToAction(nameof(Index));
                }
            }

            // Tạo phân công chính thức
            var phanCong = new PhanCongCa
            {
                MaNhanVien = dangKyCa.MaNhanVien,
                MaCa = dangKyCa.MaCa,
                NgayLam = dangKyCa.NgayLam,
                TrangThai = "Đã phân công",
                GhiChu = "Tạo từ đăng ký ca",
                NgayTao = DateTime.Now
            };

            _context.PhanCongCas.Add(phanCong);

            dangKyCa.TrangThai = "Đã duyệt";
            dangKyCa.NgayDuyet = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = "Duyệt đăng ký ca thành công.";
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateDangKyCa(DangKyCa dangKyCa, int? currentId = null)
        {
            // 1. Không cho đăng ký trùng đúng cùng 1 ca trong cùng 1 ngày
            bool isDuplicate = await _context.DangKyCas.AnyAsync(x =>
                x.MaNhanVien == dangKyCa.MaNhanVien &&
                x.MaCa == dangKyCa.MaCa &&
                x.NgayLam == dangKyCa.NgayLam &&
                x.MaDangKy != currentId);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Nhân viên đã đăng ký ca này trong ngày đã chọn.");
                return;
            }

            // 2. Lấy ca mới đang đăng ký để kiểm tra giờ
            var caMoi = await _context.CaLams.FirstOrDefaultAsync(x => x.MaCa == dangKyCa.MaCa);
            if (caMoi == null)
            {
                ModelState.AddModelError("MaCa", "Ca làm không hợp lệ.");
                return;
            }

            // 3. Kiểm tra chồng giờ với các đăng ký ca khác của chính nhân viên trong cùng ngày
            // Chỉ cần xét các đăng ký chưa bị từ chối
            var dangKyKhac = await _context.DangKyCas
                .Include(x => x.MaCaNavigation)
                .Where(x =>
                    x.MaNhanVien == dangKyCa.MaNhanVien &&
                    x.NgayLam == dangKyCa.NgayLam &&
                    x.MaDangKy != currentId &&
                    x.TrangThai != "Từ chối")
                .ToListAsync();

            foreach (var item in dangKyKhac)
            {
                var caCu = item.MaCaNavigation;

                bool biChongGio =
                    caMoi.GioBatDau < caCu.GioKetThuc &&
                    caCu.GioBatDau < caMoi.GioKetThuc;

                if (biChongGio)
                {
                    ModelState.AddModelError("", $"Ca đăng ký đang bị trùng giờ với ca '{caCu.TenCa}' trong cùng ngày.");
                    return;
                }
            }

            // 4. Kiểm tra chồng giờ với các phân công chính thức đã có
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

                bool biChongGio =
                    caMoi.GioBatDau < caCu.GioKetThuc &&
                    caCu.GioBatDau < caMoi.GioKetThuc;

                if (biChongGio)
                {
                    ModelState.AddModelError("", $"Ca đăng ký đang bị trùng giờ với phân công '{caCu.TenCa}' đã có trong ngày.");
                    return;
                }
            }
        }

        private async Task LoadDropdowns(int? selectedNhanVien = null, int? selectedCa = null)
        {
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
        }


        private async Task<string?> CheckDangKyCaBiTrungGio(int maNhanVien, int maCa, DateOnly ngayLam, int? currentId = null)
        {
            var caMoi = await _context.CaLams.FirstOrDefaultAsync(x => x.MaCa == maCa);
            if (caMoi == null) return "Ca làm không hợp lệ.";

            // Kiểm tra với các đăng ký ca khác của chính nhân viên trong cùng ngày
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

                bool biChongGio =
                    caMoi.GioBatDau < caCu.GioKetThuc &&
                    caCu.GioBatDau < caMoi.GioKetThuc;

                if (biChongGio)
                {
                    return $"Ca đăng ký bị trùng giờ với ca '{caCu.TenCa}' trong cùng ngày.";
                }
            }

            // Kiểm tra với các phân công chính thức đã có
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

                bool biChongGio =
                    caMoi.GioBatDau < caCu.GioKetThuc &&
                    caCu.GioBatDau < caMoi.GioKetThuc;

                if (biChongGio)
                {
                    return $"Ca đăng ký bị trùng giờ với phân công '{caCu.TenCa}' đã có trong ngày.";
                }
            }

            return null;
        }




        private int? GetCurrentTaiKhoanId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claim)) return null;
            return int.Parse(claim);
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

        private bool IsNhanVien()
        {
            return User.IsInRole("Nhân viên");
        }

        private bool IsAdminOrQuanLy()
        {
            return User.IsInRole("Admin") || User.IsInRole("Quản lý");
        }


    }
}