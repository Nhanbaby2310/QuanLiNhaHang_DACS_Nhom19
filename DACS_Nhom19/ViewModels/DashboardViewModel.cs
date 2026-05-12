using DACS_Nhom19.Models;

namespace DACS_Nhom19.ViewModels
{
    public class DashboardViewModel
    {
        // Tổng số nhân viên
        public int TongNhanVien { get; set; }

        // Tổng số ca làm
        public int TongCaLam { get; set; }

        // Số đăng ký chờ duyệt
        public int DangKyChoDuyet { get; set; }

        // Số ca hoàn thành hôm nay
        public int CaHoanThanhHomNay { get; set; }

        // Danh sách đăng ký mới nhất
        public List<DangKyCa> DangKyMoiNhat { get; set; } = new();

        // Nhân viên chưa đạt định mức
        public List<NhanVienThongKeViewModel> NhanVienChuaDat { get; set; } = new();
    }

    public class NhanVienThongKeViewModel
    {
        public string HoTen { get; set; } = "";

        public int SoCaDaLam { get; set; }

        public double TongGioLam { get; set; }

        public int SoCaToiThieu { get; set; }

        public double SoGioToiThieu { get; set; }
    }
}