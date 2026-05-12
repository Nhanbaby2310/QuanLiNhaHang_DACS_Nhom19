using System;
using System.Collections.Generic;

namespace DACS_Nhom19.Models;

public partial class DangKyCa
{
    public int MaDangKy { get; set; }

    public int MaNhanVien { get; set; }

    public int MaCa { get; set; }

    public DateOnly NgayLam { get; set; }

    public DateTime NgayDangKy { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public int? NguoiDuyet { get; set; }

    public DateTime? NgayDuyet { get; set; }

    public virtual CaLam MaCaNavigation { get; set; } = null!;

    public virtual NhanVien MaNhanVienNavigation { get; set; } = null!;

    public virtual TaiKhoan? NguoiDuyetNavigation { get; set; }
}