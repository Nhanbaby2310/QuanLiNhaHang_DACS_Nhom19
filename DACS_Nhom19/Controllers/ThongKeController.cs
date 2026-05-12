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

        [Authorize(Roles = "Nhân viên")]
        public async Task<IActionResult> CaNhan(DateOnly? tuNgay, DateOnly? denNgay)
        {
            var maTaiKhoanClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTaiKhoanClaim)) return Forbid();

            int maTaiKhoan = int.Parse(maTaiKhoanClaim);

            var nhanVien = await _context.NhanViens
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);

            if (nhanVien == null) return Forbid();

            var (from, to) = ResolveRange(tuNgay, denNgay);

            var danhSachHoanThanh = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x => x.MaNhanVien == nhanVien.MaNhanVien
                            && x.TrangThai == "Hoàn thành"
                            && x.NgayLam >= from
                            && x.NgayLam <= to)
                .OrderBy(x => x.NgayLam)
                .ThenBy(x => x.MaCaNavigation.GioBatDau)
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
                    ? "Chưa đạt định mức" : "Đạt định mức",
                TuNgay = from,
                DenNgay = to,
                DanhSachCa = danhSachHoanThanh.Select(x => new CaLamTrongTuan
                {
                    NgayLam = x.NgayLam,
                    TenCa = x.MaCaNavigation.TenCa,
                    GioBatDau = x.MaCaNavigation.GioBatDau.ToString("HH:mm"),
                    GioKetThuc = x.MaCaNavigation.GioKetThuc.ToString("HH:mm"),
                    SoGio = x.MaCaNavigation.SoGio ?? 0,
                    TrangThai = x.TrangThai
                }).ToList()
            };

            return View(vm);
        }

        [Authorize(Roles = "Admin,Quản lý")]
        public async Task<IActionResult> TongHop(string? keyword, DateOnly? tuNgay, DateOnly? denNgay)
        {
            var (from, to) = ResolveRange(tuNgay, denNgay);

            var nhanViens = await _context.NhanViens
                .OrderBy(x => x.HoTen)
                .ToListAsync();

            var phanCongHoanThanh = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x => x.TrangThai == "Hoàn thành"
                            && x.NgayLam >= from
                            && x.NgayLam <= to)
                .ToListAsync();

            IEnumerable<ThongKeTongHopViewModel> result = nhanViens.Select(nv =>
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
                        ? "Chưa đạt định mức" : "Đạt định mức"
                };
            });

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                result = result.Where(x =>
                    x.MaNhanVienCode.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    x.HoTen.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    (x.ChucVu ?? "").Contains(keyword, StringComparison.OrdinalIgnoreCase));
            }

            var page = new ThongKeTongHopPageViewModel
            {
                TuNgay = from,
                DenNgay = to,
                Keyword = keyword,
                DanhSach = result.ToList()
            };

            return View(page);
        }

        /// <summary>Mặc định = tuần hiện tại (thứ 2 → CN).</summary>
        private static (DateOnly from, DateOnly to) ResolveRange(DateOnly? tuNgay, DateOnly? denNgay)
        {
            if (tuNgay.HasValue && denNgay.HasValue)
            {
                var f = tuNgay.Value;
                var t = denNgay.Value;
                if (f > t) (f, t) = (t, f);
                return (f, t);
            }

            var today = DateOnly.FromDateTime(DateTime.Today);
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7;
            var monday = today.AddDays(-diffToMonday);
            var sunday = monday.AddDays(6);

            return (tuNgay ?? monday, denNgay ?? sunday);
        }
    }
}
