using System;
using System.Collections.Generic;

namespace DACS_Nhom19.Models;

public partial class TaiKhoan
{
    public int MaTaiKhoan { get; set; }

    public string TenDangNhap { get; set; } = null!;

    public string MatKhau { get; set; } = null!;

    public string HoTenHienThi { get; set; } = null!;

    public int MaVaiTro { get; set; }

    public string TrangThai { get; set; } = null!;

    public DateTime NgayTao { get; set; }

    public DateTime? LanDangNhapCuoi { get; set; }

    public virtual VaiTro MaVaiTroNavigation { get; set; } = null!;

    public virtual NhanVien? NhanVien { get; set; }

    public virtual ICollection<PhanCongCa> PhanCongCas { get; set; } = new List<PhanCongCa>();
}
