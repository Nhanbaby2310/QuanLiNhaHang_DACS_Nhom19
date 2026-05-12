using System;
using System.Collections.Generic;

namespace DACS_Nhom19.Models;

public partial class CaLam
{
    public int MaCa { get; set; }

    public string MaCaCode { get; set; } = null!;

    public string TenCa { get; set; } = null!;

    public TimeOnly GioBatDau { get; set; }

    public TimeOnly GioKetThuc { get; set; }

    public string LoaiCa { get; set; } = null!;

    public int SoLuongNhanVienToiThieu { get; set; }

    public int SoLuongNhanVienToiDa { get; set; }

    public string TrangThai { get; set; } = null!;

    public string? GhiChu { get; set; }

    public decimal? SoGio { get; set; }

    public virtual ICollection<PhanCongCa> PhanCongCas { get; set; } = new List<PhanCongCa>();
}
