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

    public class ThongKeTongHopPageViewModel
    {
        public DateOnly TuNgay { get; set; }
        public DateOnly DenNgay { get; set; }
        public string? Keyword { get; set; }
        public List<ThongKeTongHopViewModel> DanhSach { get; set; } = new();

        public int TongNhanVien => DanhSach.Count;
        public int TongDat => DanhSach.Count(x => x.KetQua == "Đạt định mức");
        public int TongChuaDat => DanhSach.Count(x => x.KetQua == "Chưa đạt định mức");
    }
}
