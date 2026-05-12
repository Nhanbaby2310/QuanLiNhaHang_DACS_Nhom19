using System.Diagnostics;
using DACS_Nhom19.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DACS_Nhom19.Data;
using DACS_Nhom19.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DACS_Nhom19.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var homNay = DateOnly.FromDateTime(DateTime.Now);

            var model = new DashboardViewModel();

            // Tổng nhân viên
            model.TongNhanVien = await _context.NhanViens.CountAsync();

            // Tổng ca làm
            model.TongCaLam = await _context.CaLams.CountAsync();

            // Đăng ký chờ duyệt
            model.DangKyChoDuyet = await _context.DangKyCas
                .CountAsync(x => x.TrangThai == "Chờ duyệt");

            // Ca hoàn thành hôm nay
            model.CaHoanThanhHomNay = await _context.PhanCongCas
                .CountAsync(x =>
                    x.TrangThai == "Hoàn thành" &&
                    x.NgayLam == homNay);

            // Đăng ký mới nhất
            model.DangKyMoiNhat = await _context.DangKyCas
                .Include(x => x.MaNhanVienNavigation)
                .Include(x => x.MaCaNavigation)
                .OrderByDescending(x => x.NgayDangKy)
                .Take(5)
                .ToListAsync();

            // Nhân viên chưa đạt định mức
            var danhSachNhanVien = await _context.NhanViens.ToListAsync();

            foreach (var nv in danhSachNhanVien)
            {
                var phanCongHoanThanh = await _context.PhanCongCas
                    .Include(x => x.MaCaNavigation)
                    .Where(x =>
                        x.MaNhanVien == nv.MaNhanVien &&
                        x.TrangThai == "Hoàn thành")
                    .ToListAsync();

                int soCa = phanCongHoanThanh.Count;

                decimal tongGio = phanCongHoanThanh
                    .Sum(x => x.MaCaNavigation.SoGio ?? 0);

                bool chuaDat =
                    soCa < nv.SoCaToiThieuTuan ||
                    tongGio < nv.SoGioToiThieuTuan;

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
            var thongKeCa = await _context.PhanCongCas
                .Include(x => x.MaNhanVienNavigation)
                .Where(x => x.TrangThai == "Hoàn thành")
                .GroupBy(x => x.MaNhanVienNavigation.HoTen)
                .Select(g => new
                 {
                    HoTen = g.Key,
                    SoCa = g.Count()
                })
                .ToListAsync();

            ViewBag.ChartLabels = JsonSerializer.Serialize(
                thongKeCa.Select(x => x.HoTen)
            );

            ViewBag.ChartData = JsonSerializer.Serialize(
                thongKeCa.Select(x => x.SoCa)
            );

            return View(model);

        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}