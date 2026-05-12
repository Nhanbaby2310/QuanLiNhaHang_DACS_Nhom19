using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace DACS_Nhom19.Controllers
{
    [Authorize(Roles = "Admin,Quản lý")]
    public class PhanCongCaController : Controller
    {
        
        private readonly ApplicationDbContext _context;

        public PhanCongCaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // 1. DANH SÁCH PHÂN CÔNG CA
      
        // =========================================
        public async Task<IActionResult> Index(string keyword, string ngayLam, int? maCa, string trangThai)
        {
            var query = _context.PhanCongCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .AsQueryable();

            
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaNhanVienNavigation.HoTen.Contains(keyword) ||
                    x.MaNhanVienNavigation.MaNhanVienCode.Contains(keyword) ||
                    x.MaCaNavigation.TenCa.Contains(keyword) ||
                    x.MaCaNavigation.MaCaCode.Contains(keyword));
            }

            
            if (!string.IsNullOrWhiteSpace(ngayLam) && DateOnly.TryParse(ngayLam, out var dateValue))
            {
                query = query.Where(x => x.NgayLam == dateValue);
            }

            
            if (maCa.HasValue)
            {
                query = query.Where(x => x.MaCa == maCa.Value);
            }

            
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
                .OrderByDescending(x => x.NgayLam)
                .ThenBy(x => x.MaCaNavigation.GioBatDau)
                .ThenBy(x => x.MaNhanVienNavigation.HoTen)
                .ToListAsync();

            return View(data);
        }

        // 2. CHI TIẾT
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

        // 3. CREATE (GET)
        public async Task<IActionResult> Create()
        {
            await LoadDropdowns();
            return View(new PhanCongCa
            {
                NgayLam = DateOnly.FromDateTime(DateTime.Today),
                TrangThai = "Đã phân công"
            });
        }

        // 4. CREATE (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaNhanVien,MaCa,NgayLam,TrangThai,GhiChu")] PhanCongCa phanCongCa)
        {

            ModelState.Remove(nameof(PhanCongCa.MaNhanVienNavigation));
            ModelState.Remove(nameof(PhanCongCa.MaCaNavigation));
            ModelState.Remove(nameof(PhanCongCa.NguoiTaoNavigation));

            await ValidatePhanCongCa(phanCongCa);

            if (ModelState.IsValid)
            {
                phanCongCa.NgayTao = DateTime.Now;
                phanCongCa.NguoiTao = GetCurrentTaiKhoanId();

                _context.Add(phanCongCa);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm phân công ca thành công.";
                return RedirectToAction(nameof(Index));
            }

            await LoadDropdowns(phanCongCa.MaNhanVien, phanCongCa.MaCa);
            return View(phanCongCa);
        }

        // 5. EDIT (GET)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var phanCongCa = await _context.PhanCongCas.FindAsync(id);
            if (phanCongCa == null) return NotFound();

            await LoadDropdowns(phanCongCa.MaNhanVien, phanCongCa.MaCa);
            return View(phanCongCa);
        }

        // 6. EDIT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaPhanCong,MaNhanVien,MaCa,NgayLam,TrangThai,GhiChu,NgayTao,NguoiTao")] PhanCongCa phanCongCa)
        {
            if (id != phanCongCa.MaPhanCong) return NotFound();

            ModelState.Remove(nameof(PhanCongCa.MaNhanVienNavigation));
            ModelState.Remove(nameof(PhanCongCa.MaCaNavigation));
            ModelState.Remove(nameof(PhanCongCa.NguoiTaoNavigation));

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
                if (!PhanCongCaExists(phanCongCa.MaPhanCong)) return NotFound();
                throw;
            }
        }

        // 7. DELETE (GET)
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

        // 8. DELETE (POST)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phanCong = await _context.PhanCongCas.FindAsync(id);
            if (phanCong == null)
            {
                TempData["Error"] = "Phân công không tồn tại.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.PhanCongCas.Remove(phanCong);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa phân công ca thành công.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Không thể xóa phân công này.";
            }

            return RedirectToAction(nameof(Index));
        }


        // =========================================
        // 9. ĐÁNH DẤU HOÀN THÀNH (POST)
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
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
            else
            {
                TempData["Error"] = "Phân công này đã kết thúc trước đó.";
            }

            return RedirectToAction(nameof(Index));
        }

        // =========================================
        // 10. HỦY PHÂN CÔNG (POST)
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var phanCong = await _context.PhanCongCas.FindAsync(id);
            if (phanCong == null) return NotFound();

            if (phanCong.TrangThai != "Hoàn thành" && phanCong.TrangThai != "Đã hủy")
            {
                phanCong.TrangThai = "Đã hủy";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã hủy phân công.";
            }
            return RedirectToAction(nameof(Index));
        }

        // 11. CALENDAR VIEW (trang lịch tuần/tháng)
        public IActionResult Calendar()
        {
            return View();
        }

        // API JSON cho FullCalendar
        [HttpGet]
        public async Task<IActionResult> Events(DateTime start, DateTime end)
        {
            var startDate = DateOnly.FromDateTime(start);
            var endDate = DateOnly.FromDateTime(end);

            var items = await _context.PhanCongCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .Where(x => x.NgayLam >= startDate && x.NgayLam <= endDate)
                .ToListAsync();

            var events = items.Select(p =>
            {
                var ngay = p.NgayLam;
                var s = new DateTime(ngay.Year, ngay.Month, ngay.Day,
                    p.MaCaNavigation.GioBatDau.Hour, p.MaCaNavigation.GioBatDau.Minute, 0);

                var gioKT = p.MaCaNavigation.GioKetThuc;
                // Nếu giờ kết thúc <= giờ bắt đầu (ca qua ngày), cộng thêm 1 ngày
                DateTime e;
                if (gioKT <= p.MaCaNavigation.GioBatDau)
                {
                    var next = ngay.AddDays(1);
                    e = new DateTime(next.Year, next.Month, next.Day, gioKT.Hour, gioKT.Minute, 0);
                }
                else
                {
                    e = new DateTime(ngay.Year, ngay.Month, ngay.Day, gioKT.Hour, gioKT.Minute, 0);
                }

                var color = p.TrangThai switch
                {
                    "Hoàn thành" => "#198754",
                    "Đã hủy" => "#6c757d",
                    "Đổi ca" => "#0dcaf0",
                    "Nghỉ" => "#ffc107",
                    _ => "#c1272d" // mặc định (Đã phân công) - màu đỏ burgundy
                };

                return new
                {
                    id = p.MaPhanCong,
                    title = $"{p.MaCaNavigation.TenCa} • {p.MaNhanVienNavigation.HoTen}",
                    start = s.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = e.ToString("yyyy-MM-ddTHH:mm:ss"),
                    backgroundColor = color,
                    borderColor = color,
                    url = Url.Action("Details", "PhanCongCa", new { id = p.MaPhanCong })
                };
            });

            return Json(events);
        }

        private bool PhanCongCaExists(int id) => _context.PhanCongCas.Any(x => x.MaPhanCong == id);

        private int? GetCurrentTaiKhoanId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(claim)) return null;
            if (int.TryParse(claim, out var id)) return id;
            return null;
        }


        // VALIDATE: chống trùng + chồng giờ
        private async Task ValidatePhanCongCa(PhanCongCa phanCongCa, int? currentId = null)
        {
           
            bool isDuplicate = await _context.PhanCongCas.AnyAsync(x =>
                x.MaNhanVien == phanCongCa.MaNhanVien &&
                x.MaCa == phanCongCa.MaCa &&
                x.NgayLam == phanCongCa.NgayLam &&
                x.MaPhanCong != currentId);

            if (isDuplicate)
            {
                ModelState.AddModelError("", "Nhân viên này đã được phân vào ca này trong ngày đã chọn.");
            }

            
            var caMoi = await _context.CaLams.FirstOrDefaultAsync(x => x.MaCa == phanCongCa.MaCa);
            if (caMoi == null)
            {
                ModelState.AddModelError("MaCa", "Ca làm không hợp lệ.");
                return;
            }

           
            var danhSachCaDaPhan = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x =>
                    x.MaNhanVien == phanCongCa.MaNhanVien &&
                    x.NgayLam == phanCongCa.NgayLam &&
                    x.MaPhanCong != currentId &&
                    x.TrangThai != "Đã hủy")
                .ToListAsync();

            
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