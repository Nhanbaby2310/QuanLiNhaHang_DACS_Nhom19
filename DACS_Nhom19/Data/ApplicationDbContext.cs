using System;
using System.Collections.Generic;
using DACS_Nhom19.Models;
using Microsoft.EntityFrameworkCore;

namespace DACS_Nhom19.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CaLam> CaLams { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<PhanCongCa> PhanCongCas { get; set; }

    public virtual DbSet<TaiKhoan> TaiKhoans { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    public virtual DbSet<DangKyCa> DangKyCas { get; set; }

    // Connection string được inject qua DI trong Program.cs (đọc từ appsettings.json).
    // Không hardcode ở đây để không ghi đè cấu hình và để dễ chạy trên máy khác.

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CaLam>(entity =>
        {
            entity.HasKey(e => e.MaCa).HasName("PK__CaLam__27258E7B09C521B0");

            entity.ToTable("CaLam");

            entity.HasIndex(e => e.MaCaCode, "UQ__CaLam__DB28C089B180FB4C").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.LoaiCa).HasMaxLength(20);
            entity.Property(e => e.MaCaCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoGio)
                .HasComputedColumnSql("(CONVERT([decimal](4,2),datediff(minute,[GioBatDau],[GioKetThuc])/(60.0)))", false)
                .HasColumnType("decimal(4, 2)");
            entity.Property(e => e.SoLuongNhanVienToiDa).HasDefaultValue(1);
            entity.Property(e => e.SoLuongNhanVienToiThieu).HasDefaultValue(1);
            entity.Property(e => e.TenCa).HasMaxLength(50);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Hoạt động");
        });


        modelBuilder.Entity<DangKyCa>(entity =>
        {
            entity.HasKey(e => e.MaDangKy).HasName("PK__DangKyCa__BA3FA9CF");

            entity.ToTable("DangKyCa");

            entity.HasIndex(e => new { e.MaNhanVien, e.MaCa, e.NgayLam }, "UX_DangKyCa_NhanVien_Ca_Ngay")
                .IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayDangKy).HasColumnType("datetime2");
            entity.Property(e => e.NgayDuyet).HasColumnType("datetime2");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaCaNavigation).WithMany()
                .HasForeignKey(d => d.MaCa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKyCa_CaLam");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany()
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DangKyCa_NhanVien");

            entity.HasOne(d => d.NguoiDuyetNavigation).WithMany()
                .HasForeignKey(d => d.NguoiDuyet)
                .HasConstraintName("FK_DangKyCa_NguoiDuyet");
        });


        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNhanVien).HasName("PK__NhanVien__77B2CA4752B76904");

            entity.ToTable("NhanVien");

            entity.HasIndex(e => e.SoDienThoai, "UQ_NhanVien_SoDienThoai").IsUnique();

            entity.HasIndex(e => e.MaNhanVienCode, "UQ__NhanVien__82A2A840B2AC49F8").IsUnique();

            entity.HasIndex(e => e.Email, "UX_NhanVien_Email")
                .IsUnique()
                .HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.MaTaiKhoan, "UX_NhanVien_MaTaiKhoan")
                .IsUnique()
                .HasFilter("([MaTaiKhoan] IS NOT NULL)");

            entity.Property(e => e.ChucVu).HasMaxLength(50);
            entity.Property(e => e.DiaChi).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.LoaiNhanVien).HasMaxLength(20);
            entity.Property(e => e.MaNhanVienCode)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.SoGioToiThieuTuan).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Đang làm");

            entity.HasOne(d => d.MaTaiKhoanNavigation).WithOne(p => p.NhanVien)
                .HasForeignKey<NhanVien>(d => d.MaTaiKhoan)
                .HasConstraintName("FK_NhanVien_TaiKhoan");
        });

        modelBuilder.Entity<PhanCongCa>(entity =>
        {
            entity.HasKey(e => e.MaPhanCong).HasName("PK__PhanCong__C279D916A433BAC7");

            entity.ToTable("PhanCongCa", tb =>
                {
                    tb.HasTrigger("TRG_PhanCongCa_KiemTraChongGio");
                    tb.HasTrigger("TRG_PhanCongCa_KiemTraSoLuongToiDa");
                });

            entity.HasIndex(e => new { e.MaNhanVien, e.NgayLam }, "IX_PhanCongCa_MaNhanVien_NgayLam");

            entity.HasIndex(e => new { e.NgayLam, e.MaCa }, "IX_PhanCongCa_NgayLam_MaCa");

            entity.HasIndex(e => new { e.MaNhanVien, e.MaCa, e.NgayLam }, "UX_PhanCongCa_NhanVien_Ca_Ngay").IsUnique();

            entity.Property(e => e.GhiChu).HasMaxLength(255);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TrangThai)
                .HasMaxLength(30)
                .HasDefaultValue("Đã phân công");

            entity.HasOne(d => d.MaCaNavigation).WithMany(p => p.PhanCongCas)
                .HasForeignKey(d => d.MaCa)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhanCongCa_CaLam");

            entity.HasOne(d => d.MaNhanVienNavigation).WithMany(p => p.PhanCongCas)
                .HasForeignKey(d => d.MaNhanVien)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PhanCongCa_NhanVien");

            entity.HasOne(d => d.NguoiTaoNavigation).WithMany(p => p.PhanCongCas)
                .HasForeignKey(d => d.NguoiTao)
                .HasConstraintName("FK_PhanCongCa_NguoiTao");
        });

        modelBuilder.Entity<TaiKhoan>(entity =>
        {
            entity.HasKey(e => e.MaTaiKhoan).HasName("PK__TaiKhoan__AD7C6529F874E431");

            entity.ToTable("TaiKhoan");

            entity.HasIndex(e => e.TenDangNhap, "UQ__TaiKhoan__55F68FC0EB555062").IsUnique();

            entity.Property(e => e.HoTenHienThi).HasMaxLength(100);
            entity.Property(e => e.MatKhau)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.NgayTao).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.TenDangNhap)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.TrangThai)
                .HasMaxLength(20)
                .HasDefaultValue("Hoạt động");

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.TaiKhoans)
                .HasForeignKey(d => d.MaVaiTro)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TaiKhoan_VaiTro");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF8928C623");

            entity.ToTable("VaiTro");

            entity.HasIndex(e => e.TenVaiTro, "UQ__VaiTro__1DA55814FDDE1925").IsUnique();

            entity.Property(e => e.MoTa).HasMaxLength(255);
            entity.Property(e => e.TenVaiTro).HasMaxLength(30);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
