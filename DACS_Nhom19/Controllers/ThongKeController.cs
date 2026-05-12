using DACS_Nhom19.Data;
using DACS_Nhom19.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DACS_Nhom19.Controllers
{
    [Authorize]
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // NHÂN VIÊN: xem thống kê của chính mình
        [Authorize(Roles = "Nhân viên")]
        public async Task<IActionResult> CaNhan()
        {
            var maTaiKhoanClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTaiKhoanClaim)) return Forbid();

            int maTaiKhoan = int.Parse(maTaiKhoanClaim);

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);

            if (nhanVien == null) return Forbid();

            var danhSachHoanThanh = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x => x.MaNhanVien == nhanVien.MaNhanVien && x.TrangThai == "Hoàn thành")
                .ToListAsync();

            var tongSoCa = danhSachHoanThanh.Count;
            var tongSoGio = danhSachHoanThanh.Sum(x => x.MaCaNavigation.SoGio ?? 0);

            var vm = new ThongKeCaNhanViewModel
            {
                HoTen = nhanVien.HoTen,
                MaNhanVienCode = nhanVien.MaNhanVienCode,
                SoCaToiThieuTuan = nhanVien.SoCaToiThieuTuan,
                SoGioToiThieuTuan = nhanVien.SoGioToiThieuTuan,
                TongSoCa = tongSoCa,
                TongSoGio = tongSoGio,
                KetQua = (tongSoCa < nhanVien.SoCaToiThieuTuan || tongSoGio < nhanVien.SoGioToiThieuTuan)
                    ? "Chưa đạt định mức"
                    : "Đạt định mức"
            };

            return View(vm);
        }

        // ADMIN, QUẢN LÝ: xem thống kê toàn bộ nhân viên
        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> TongHop(string keyword)
        {
            var nhanViens = await _context.NhanViens
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            var phanCongHoanThanh = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x => x.TrangThai == "Hoàn thành")
                .ToListAsync();

            var result = nhanViens.Select(nv =>
            {
                var ds = phanCongHoanThanh.Where(x => x.MaNhanVien == nv.MaNhanVien).ToList();

                int tongSoCa = ds.Count;
                decimal tongSoGio = ds.Sum(x => x.MaCaNavigation.SoGio ?? 0);

                return new ThongKeTongHopViewModel
                {
                    MaNhanVien = nv.MaNhanVien,
                    MaNhanVienCode = nv.MaNhanVienCode,
                    HoTen = nv.HoTen,
                    ChucVu = nv.ChucVu,
                    SoCaToiThieuTuan = nv.SoCaToiThieuTuan,
                    SoGioToiThieuTuan = nv.SoGioToiThieuTuan,
                    TongSoCa = tongSoCa,
                    TongSoGio = tongSoGio,
                    KetQua = (tongSoCa < nv.SoCaToiThieuTuan || tongSoGio < nv.SoGioToiThieuTuan)
                        ? "Chưa đạt định mức"
                        : "Đạt định mức"
                };
            });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(x =>
                    x.MaNhanVienCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.HoTen.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.ChucVu.Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            ViewBag.Keyword = keyword;

            return View(result.ToList());
        }
    }
}