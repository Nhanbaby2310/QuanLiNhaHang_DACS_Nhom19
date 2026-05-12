using System;
using System.Collections.Generic;

namespace DACS_Nhom19.Models;

public partial class PhanCongCa
{
    public int MaPhanCong { get; set; }

    public int MaNhanVien { get; set; }

    public int MaCa { get; set; }

    public DateOnly NgayLam { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public DateTime NgayTao { get; set; }

    public int? NguoiTao { get; set; }

    public virtual CaLam MaCaNavigation { get; set; } = null!;

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;

    public virtual TaiKhoan? NguoiTaoNavigation { get; set; }
}
