using DACS_Nhom19.Models;

namespace DACS_Nhom19.ViewModels
{
    public class DashboardViewModel
    {

        // ============================================
        // Dashboard cho Admin / Quản lý
        // ============================================
        public DateOnly TuNgay { get; set; }
        public DateOnly DenNgay { get; set; }

        
        public int TongNhanVien { get; set; }
        public int TongCaLam { get; set; }

        
        public int DangKyChoDuyet { get; set; }

        
        public int CaHoanThanhHomNay { get; set; }
        public int CaTrongTuan { get; set; }

        
        public List<DangKyCa> DangKyMoiNhat { get; set; } = new();

        
        public List<PhanCongCa> PhanCongHomNay { get; set; } = new();
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

    // ============================================
    // Dashboard cho Nhân viên
    // ============================================
    public class NhanVienDashboardViewModel
    {
        public string HoTen { get; set; } = "";
        public string MaNhanVienCode { get; set; } = "";

        public DateOnly TuNgay { get; set; }
        public DateOnly DenNgay { get; set; }

        public int SoCaTrongTuan { get; set; }
        public decimal TongGioTrongTuan { get; set; }
        public int SoCaChoDuyet { get; set; }
        public int SoCaSapToi { get; set; } // sắp tới (hôm nay trở đi trong 7 ngày)

        public int SoCaToiThieuTuan { get; set; }
        public decimal SoGioToiThieuTuan { get; set; }

        public List<PhanCongCa> CaSapToi { get; set; } = new();
        public List<DangKyCa> DangKyCuaToi { get; set; } = new();
    }
}