namespace DACS_Nhom19.ViewModels
{
    public class ThongKeCaNhanViewModel
    {
        public string HoTen { get; set; } = string.Empty;
        public string MaNhanVienCode { get; set; } = string.Empty;

        public int SoCaToiThieuTuan { get; set; }
        public decimal SoGioToiThieuTuan { get; set; }

        public int TongSoCa { get; set; }
        public decimal TongSoGio { get; set; }

        public string KetQua { get; set; } = string.Empty;

        public DateOnly TuNgay { get; set; }
        public DateOnly DenNgay { get; set; }

        public List<CaLamTrongTuan> DanhSachCa { get; set; } = new();
    }

    public class CaLamTrongTuan
    {
        public DateOnly NgayLam { get; set; }
        public string TenCa { get; set; } = string.Empty;
        public string GioBatDau { get; set; } = string.Empty;
        public string GioKetThuc { get; set; } = string.Empty;
        public decimal SoGio { get; set; }
        public string TrangThai { get; set; } = string.Empty;
    }
}
