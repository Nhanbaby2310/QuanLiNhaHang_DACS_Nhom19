using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using DACS_Nhom19.Data;
using DACS_Nhom19.Models;
using DACS_Nhom19.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DACS_Nhom19.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Nhân viên"))
                return await NhanVienDashboard();

            return await AdminDashboard();
        }

        private async Task<IActionResult> AdminDashboard()
        {
            var homNay = DateOnly.FromDateTime(DateTime.Now);
            var (monday, sunday) = GetThisWeek();

            var model = new DashboardViewModel { TuNgay = monday, DenNgay = sunday };

            model.TongNhanVien = await _context.NhanViens.CountAsync(x => x.TrangThai != "Nghỉ việc");
            model.TongCaLam = await _context.CaLams.CountAsync(x => x.TrangThai == "Hoạt động");

            model.DangKyChoDuyet = await _context.DangKyCas.CountAsync(x => x.TrangThai == "Chờ duyệt");

            model.CaHoanThanhHomNay = await _context.PhanCongCas
                .CountAsync(x => x.TrangThai == "Hoàn thành" && x.NgayLam == homNay);

            model.CaTrongTuan = await _context.PhanCongCas
                .CountAsync(x => x.NgayLam >= monday && x.NgayLam <= sunday && x.TrangThai != "Đã hủy");

            model.DangKyMoiNhat = await _context.DangKyCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .OrderByDescending(x => x.NgayDangKy)
                .Take(5)
                .ToListAsync();

            model.PhanCongHomNay = await _context.PhanCongCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .Where(x => x.NgayLam == homNay && x.TrangThai != "Đã hủy")
                .OrderBy(x => x.MaCaNavigation.GioBatDau)
                .ToListAsync();

            var nhanViens = await _context.NhanViens
                .Where(x => x.TrangThai != "Nghỉ việc")
                .ToListAsync();

            var phanCongTrongTuan = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x => x.TrangThai == "Hoàn thành"
                         && x.NgayLam >= monday
                         && x.NgayLam <= sunday)
                .ToListAsync();

            foreach (var nv in nhanViens)
            {
                var cua = phanCongTrongTuan.Where(x => x.MaNhanVien == nv.MaNhanVien).ToList();
                int soCa = cua.Count;
                decimal tongGio = cua.Sum(x => x.MaCaNavigation.SoGio ?? 0);

                bool chuaDat = soCa < nv.SoCaToiThieuTuan || tongGio < nv.SoGioToiThieuTuan;
                if (chuaDat)
                {
                    model.NhanVienChuaDat.Add(new NhanVienThongKeViewModel
                    {
                        HoTen = nv.HoTen,
                        SoCaDaLam = soCa,
                        TongGioLam = (double)tongGio,
                        SoCaToiThieu = nv.SoCaToiThieuTuan,
                        SoGioToiThieu = (double)nv.SoGioToiThieuTuan
                    });
                }
            }

            var thongKeCa = phanCongTrongTuan
                .GroupBy(x => x.MaNhanVien)
                .Select(g => new { MaNhanVien = g.Key, SoCa = g.Count() })
                .ToList();

            var chartLabels = new List<string>();
            var chartData = new List<int>();

            foreach (var nv in nhanViens)
            {
                var row = thongKeCa.FirstOrDefault(x => x.MaNhanVien == nv.MaNhanVien);
                chartLabels.Add(nv.HoTen);
                chartData.Add(row?.SoCa ?? 0);
            }

            ViewBag.ChartLabels = JsonSerializer.Serialize(chartLabels);
            ViewBag.ChartData = JsonSerializer.Serialize(chartData);

            return View("Index", model);
        }

        private async Task<IActionResult> NhanVienDashboard()
        {
            var maTaiKhoanClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(maTaiKhoanClaim))
                return RedirectToAction("Login", "Account");

            int maTaiKhoan = int.Parse(maTaiKhoanClaim);
            var nhanVien = await _context.NhanViens.FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);

            if (nhanVien == null)
                return View("NhanVienChuaLienKet");

            var (monday, sunday) = GetThisWeek();
            var today = DateOnly.FromDateTime(DateTime.Today);

            var phanCongTuan = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x => x.MaNhanVien == nhanVien.MaNhanVien
                         && x.NgayLam >= monday
                         && x.NgayLam <= sunday
                         && x.TrangThai == "Hoàn thành")
                .ToListAsync();

            var caSapToi = await _context.PhanCongCas
                .Include(x => x.MaCaNavigation)
                .Where(x => x.MaNhanVien == nhanVien.MaNhanVien
                         && x.NgayLam >= today
                         && x.TrangThai != "Đã hủy"
                         && x.TrangThai != "Hoàn thành")
                .OrderBy(x => x.NgayLam)
                .ThenBy(x => x.MaCaNavigation.GioBatDau)
                .Take(5)
                .ToListAsync();

            var dangKyCuaToi = await _context.DangKyCas
                .Include(x => x.MaCaNavigation)
                .Where(x => x.MaNhanVien == nhanVien.MaNhanVien)
                .OrderByDescending(x => x.NgayDangKy)
                .Take(5)
                .ToListAsync();

            var vm = new NhanVienDashboardViewModel
            {
                HoTen = nhanVien.HoTen,
                MaNhanVienCode = nhanVien.MaNhanVienCode,
                TuNgay = monday,
                DenNgay = sunday,
                SoCaTrongTuan = phanCongTuan.Count,
                TongGioTrongTuan = phanCongTuan.Sum(x => x.MaCaNavigation.SoGio ?? 0),
                SoCaChoDuyet = await _context.DangKyCas.CountAsync(x =>
                    x.MaNhanVien == nhanVien.MaNhanVien && x.TrangThai == "Chờ duyệt"),
                SoCaSapToi = caSapToi.Count,
                SoCaToiThieuTuan = nhanVien.SoCaToiThieuTuan,
                SoGioToiThieuTuan = nhanVien.SoGioToiThieuTuan,
                CaSapToi = caSapToi,
                DangKyCuaToi = dangKyCuaToi
            };

            return View("IndexNhanVien", vm);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private static (DateOnly monday, DateOnly sunday) GetThisWeek()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int diffToMonday = ((int)today.DayOfWeek + 6) % 7;
            var monday = today.AddDays(-diffToMonday);
            var sunday = monday.AddDays(6);
            return (monday, sunday);
        }
    }
}
