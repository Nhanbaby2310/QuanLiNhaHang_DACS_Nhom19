namespace DACS_Nhom19.ViewModels
{
    public class ThongKeTongHopViewModel
    {
        public int MaNhanVien { get; set; }
        public string MaNhanVienCode { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string ChucVu { get; set; } = string.Empty;

        public int SoCaToiThieuTuan { get; set; }
        public decimal SoGioToiThieuTuan { get; set; }

        public int TongSoCa { get; set; }
        public decimal TongSoGio { get; set; }

        public string KetQua { get; set; } = string.Empty;
    }
}