using System;
using System.Collections.Generic;

namespace DACS_Nhom19.Models;

public partial class NhanVien
{
    public int MaNhanVien { get; set; }

    public string MaNhanVienCode { get; set; } = null!;

    public string HoTen { get; set; } = null!;

    public string GioiTinh { get; set; } = null!;

    public DateOnly NgaySinh { get; set; }

    public string SoDienThoai { get; set; } = null!;

    public string? Email { get; set; }

    public string? DiaChi { get; set; }

    public string ChucVu { get; set; } = null!;

    public string LoaiNhanVien { get; set; } = null!;

    public DateOnly NgayVaoLam { get; set; }

    public int SoCaToiThieuTuan { get; set; }

    public decimal SoGioToiThieuTuan { get; set; }

    public string TrangThai { get; set; } = null!;

    public int? MaTaiKhoan { get; set; }

    public virtual TaiKhoan? MaTaiKhoanNavigation { get; set; }

    public virtual ICollection<PhanCongCa> PhanCongCas { get; set; } = new List<PhanCongCa>();
}
