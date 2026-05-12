using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace DACS_Nhom19.Controllers
{
    [Authorize(Roles = "Admin,Quản lý")]
    public class CaLamController : Controller
    {
        // Biến _context dùng để làm việc với database
        private readonly ApplicationDbContext _context;

        // Hàm khởi tạo: nhận DbContext từ hệ thống DI của ASP.NET Core
        public CaLamController(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================================
        // 1. HIỂN THỊ DANH SÁCH CA LÀM
        // Có hỗ trợ tìm kiếm + lọc
        // =========================================
        public async Task<IActionResult> Index(string keyword, string loaiCa, string trangThai)
        {
            // Lấy dữ liệu từ bảng CaLams
            var query = _context.CaLams.AsQueryable();

            // Nếu có từ khóa thì tìm theo mã ca, tên ca, ghi chú
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(x =>
                    x.MaCaCode.Contains(keyword) ||
                    x.TenCa.Contains(keyword) ||
                    (x.GhiChu != null && x.GhiChu.Contains(keyword)));
            }

            // Lọc theo loại ca
            if (!string.IsNullOrWhiteSpace(loaiCa))
            {
                query = query.Where(x => x.LoaiCa == loaiCa);
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(x => x.TrangThai == trangThai);
            }

            // Gửi lại dữ liệu lọc ra View để giữ giá trị trên form tìm kiếm
            ViewBag.Keyword = keyword;
            ViewBag.LoaiCa = loaiCa;
            ViewBag.TrangThai = trangThai;

            // Sắp xếp theo giờ bắt đầu
            var data = await query
                .OrderBy(x => x.GioBatDau)
                .ToListAsync();

            return View(data);
        }

        // =========================================
        // 2. XEM CHI TIẾT 1 CA LÀM
        // =========================================
        public async Task<IActionResult> Details(int? id)
        {
            // Nếu id rỗng thì báo lỗi NotFound
            if (id == null) return NotFound();

            // Tìm ca làm theo id
            var caLam = await _context.CaLams
                .FirstOrDefaultAsync(x => x.MaCa == id);

            // Không tìm thấy thì báo lỗi
            if (caLam == null) return NotFound();

            return View(caLam);
        }

        // =========================================
        // 3. HIỂN THỊ FORM THÊM MỚI
        // =========================================
        public IActionResult Create()
        {
            // Đổ dữ liệu cho dropdown
            LoadDanhMuc();
            return View();
        }

        // =========================================
        // 4. XỬ LÝ THÊM MỚI
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MaCaCode,TenCa,GioBatDau,GioKetThuc,LoaiCa,SoLuongNhanVienToiThieu,SoLuongNhanVienToiDa,TrangThai,GhiChu")] CaLam caLam)
        {
            // Gọi hàm kiểm tra dữ liệu
            await ValidateCaLam(caLam);

            // Nếu dữ liệu hợp lệ
            if (ModelState.IsValid)
            {
                _context.Add(caLam);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm ca làm thành công.";
                return RedirectToAction(nameof(Index));
            }

            // Nếu lỗi thì nạp lại dropdown và trả về form
            LoadDanhMuc();
            return View(caLam);
        }

        // =========================================
        // 5. HIỂN THỊ FORM SỬA
        // =========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var caLam = await _context.CaLams.FindAsync(id);
            if (caLam == null) return NotFound();

            LoadDanhMuc();
            return View(caLam);
        }

        // =========================================
        // 6. XỬ LÝ CẬP NHẬT
        // =========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaCa,MaCaCode,TenCa,GioBatDau,GioKetThuc,LoaiCa,SoLuongNhanVienToiThieu,SoLuongNhanVienToiDa,TrangThai,GhiChu")] CaLam caLam)
        {
            // Kiểm tra id trên URL có khớp id của model không
            if (id != caLam.MaCa) return NotFound();

            // Kiểm tra dữ liệu, có truyền currentId để loại trừ chính bản ghi đang sửa
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
                    // Nếu bản ghi không còn tồn tại
                    if (!CaLamExists(caLam.MaCa))
                        return NotFound();
                    else
                        throw;
                }
            }

            LoadDanhMuc();
            return View(caLam);
        }

        // =========================================
        // 7. HIỂN THỊ FORM XÓA
        // =========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var caLam = await _context.CaLams
                .FirstOrDefaultAsync(x => x.MaCa == id);

            if (caLam == null) return NotFound();

            return View(caLam);
        }

        // =========================================
        // 8. XỬ LÝ XÓA
        // =========================================
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

            // Kiểm tra ràng buộc: đã được dùng trong PhanCongCa hoặc DangKyCa?
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

        // =========================================
        // 9. KIỂM TRA CA LÀM CÓ TỒN TẠI HAY KHÔNG
        // =========================================
        private bool CaLamExists(int id)
        {
            return _context.CaLams.Any(e => e.MaCa == id);
        }

        // =========================================
        // 10. KIỂM TRA DỮ LIỆU ĐẦU VÀO
        // Dùng cho Create và Edit
        // =========================================
        private async Task ValidateCaLam(CaLam caLam, int? currentId = null)
        {
            // Không cho trùng mã ca
            if (await _context.CaLams.AnyAsync(x =>
                x.MaCaCode == caLam.MaCaCode &&
                x.MaCa != currentId))
            {
                ModelState.AddModelError("MaCaCode", "Mã ca đã tồn tại.");
            }

            // Không cho trùng tên ca
            if (await _context.CaLams.AnyAsync(x =>
                x.TenCa == caLam.TenCa &&
                x.MaCa != currentId))
            {
                ModelState.AddModelError("TenCa", "Tên ca đã tồn tại.");
            }

            // Giờ kết thúc phải lớn hơn giờ bắt đầu
            if (caLam.GioKetThuc <= caLam.GioBatDau)
            {
                ModelState.AddModelError("GioKetThuc", "Giờ kết thúc phải lớn hơn giờ bắt đầu.");
            }

            // Số lượng tối thiểu phải >= 1
            if (caLam.SoLuongNhanVienToiThieu < 1)
            {
                ModelState.AddModelError("SoLuongNhanVienToiThieu", "Số lượng tối thiểu phải >= 1.");
            }

            // Số lượng tối đa phải >= số lượng tối thiểu
            if (caLam.SoLuongNhanVienToiDa < caLam.SoLuongNhanVienToiThieu)
            {
                ModelState.AddModelError("SoLuongNhanVienToiDa", "Số lượng tối đa phải lớn hơn hoặc bằng số lượng tối thiểu.");
            }
        }

        // =========================================
        // 11. NẠP DỮ LIỆU CHO DROPDOWN
        // =========================================
        private void LoadDanhMuc()
        {
            // Dropdown loại ca
            ViewBag.LoaiCaList = new SelectList(new List<string> { "Chuẩn", "Đặc biệt" });

            // Dropdown trạng thái
            ViewBag.TrangThaiList = new SelectList(new List<string> { "Hoạt động", "Ngưng" });
        }
    }
}