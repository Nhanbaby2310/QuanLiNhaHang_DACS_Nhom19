# BÁO CÁO ĐỒ ÁN CHUYÊN NGÀNH

## HỆ THỐNG QUẢN LÝ CA LÀM VIỆC NHÀ HÀNG

**Nhóm thực hiện:** Nhóm 19

---

## MỤC LỤC

1. [Giới thiệu đề tài](#1-giới-thiệu-đề-tài)
2. [Phân tích yêu cầu](#2-phân-tích-yêu-cầu)
3. [Công nghệ sử dụng](#3-công-nghệ-sử-dụng)
4. [Thiết kế hệ thống](#4-thiết-kế-hệ-thống)
5. [Thiết kế cơ sở dữ liệu](#5-thiết-kế-cơ-sở-dữ-liệu)
6. [Chức năng hệ thống](#6-chức-năng-hệ-thống)
7. [Bảo mật](#7-bảo-mật)
8. [Kết luận](#8-kết-luận)

---


## 1. GIỚI THIỆU ĐỀ TÀI

### 1.1. Lý do chọn đề tài

Trong ngành kinh doanh nhà hàng, việc quản lý lịch làm việc của nhân viên là một bài toán phức tạp. Các nhà hàng thường gặp khó khăn trong việc:
- Sắp xếp ca làm phù hợp với số lượng nhân viên
- Tránh trùng lặp và chồng giờ khi phân công
- Theo dõi định mức làm việc của từng nhân viên
- Xử lý đăng ký ca, đổi ca một cách có hệ thống

Do đó, nhóm quyết định xây dựng **Hệ thống Quản lý Ca Làm Việc Nhà Hàng** nhằm số hóa và tự động hóa quy trình quản lý nhân sự - ca làm.

### 1.2. Mục tiêu

- Xây dựng hệ thống web quản lý ca làm việc cho nhà hàng
- Hỗ trợ phân công ca tự động, kiểm tra chồng giờ
- Cho phép nhân viên đăng ký ca, quản lý duyệt/từ chối
- Thống kê số ca, số giờ làm việc theo tuần/tháng
- Dashboard trực quan cho cả quản lý và nhân viên

### 1.3. Phạm vi

- Quản lý nhân viên (CRUD)
- Quản lý ca làm (CRUD)
- Phân công ca (tạo, sửa, xóa, đánh dấu hoàn thành, hủy)
- Đăng ký ca (nhân viên đăng ký, quản lý duyệt/từ chối)
- Quản lý tài khoản và phân quyền
- Thống kê tổng hợp và cá nhân
- Lịch làm việc dạng Calendar

---


## 2. PHÂN TÍCH YÊU CẦU

### 2.1. Yêu cầu chức năng

| STT | Chức năng | Mô tả | Vai trò |
|-----|-----------|--------|---------|
| 1 | Đăng nhập/Đăng xuất | Xác thực người dùng bằng Cookie Authentication | Tất cả |
| 2 | Dashboard | Hiển thị tổng quan hệ thống (Admin) / thông tin cá nhân (NV) | Tất cả |
| 3 | Quản lý nhân viên | Thêm, sửa, xóa, xem danh sách nhân viên | Admin, Quản lý |
| 4 | Quản lý ca làm | Thêm, sửa, xóa, xem danh sách ca làm | Admin, Quản lý |
| 5 | Phân công ca | Phân công nhân viên vào ca, kiểm tra chồng giờ | Admin, Quản lý |
| 6 | Đăng ký ca | Nhân viên đăng ký ca mong muốn | Nhân viên |
| 7 | Duyệt đăng ký | Duyệt hoặc từ chối đăng ký ca | Admin, Quản lý |
| 8 | Quản lý tài khoản | Tạo, sửa, xóa tài khoản người dùng | Admin |
| 9 | Thống kê cá nhân | Xem số ca/giờ làm việc của bản thân | Nhân viên |
| 10 | Thống kê tổng hợp | Xem thống kê toàn bộ nhân viên | Admin, Quản lý |
| 11 | Lịch phân công (Calendar) | Xem lịch phân công dạng FullCalendar | Admin, Quản lý |

### 2.2. Yêu cầu phi chức năng

- **Hiệu năng:** Hệ thống phản hồi nhanh, hỗ trợ truy vấn tìm kiếm/lọc
- **Bảo mật:** Mã hóa mật khẩu PBKDF2, phân quyền theo vai trò
- **Khả dụng:** Giao diện responsive, thân thiện người dùng
- **Mở rộng:** Kiến trúc MVC dễ bảo trì và mở rộng

### 2.3. Đối tượng sử dụng

| Vai trò | Mô tả |
|---------|--------|
| **Admin** | Toàn quyền quản lý hệ thống (tài khoản, nhân viên, ca làm, phân công, thống kê) |
| **Quản lý** | Quản lý nhân viên, ca làm, phân công, duyệt đăng ký, thống kê |
| **Nhân viên** | Xem lịch cá nhân, đăng ký ca, xem thống kê cá nhân |

---


## 3. CÔNG NGHỆ SỬ DỤNG

### 3.1. Nền tảng và Framework

| Công nghệ | Phiên bản | Mục đích |
|-----------|-----------|----------|
| .NET | 8.0 | Runtime platform |
| ASP.NET Core MVC | 8.0 | Web framework (Model-View-Controller) |
| Entity Framework Core | 8.0.13 | ORM (Object-Relational Mapping) |
| SQL Server | - | Hệ quản trị cơ sở dữ liệu |
| C# | 12 | Ngôn ngữ lập trình chính |

### 3.2. Thư viện NuGet

| Package | Phiên bản | Mục đích |
|---------|-----------|----------|
| Microsoft.EntityFrameworkCore | 8.0.13 | ORM framework |
| Microsoft.EntityFrameworkCore.SqlServer | 8.0.13 | Provider cho SQL Server |
| Microsoft.EntityFrameworkCore.Design | 8.0.13 | Hỗ trợ migration, scaffolding |
| Microsoft.EntityFrameworkCore.Tools | 8.0.13 | CLI tools cho EF Core |
| Microsoft.VisualStudio.Web.CodeGeneration.Design | 8.0.7 | Scaffolding code generation |

### 3.3. Front-end

- **Razor Views** (.cshtml) - Template engine của ASP.NET Core
- **Bootstrap** - CSS framework responsive
- **FullCalendar** - Thư viện JavaScript hiển thị lịch
- **Chart.js** - Biểu đồ thống kê

### 3.4. Kiến trúc

Hệ thống áp dụng mô hình **MVC (Model-View-Controller)**:

```
┌─────────────────────────────────────────┐
│              Browser (Client)            │
└────────────────────┬────────────────────┘
                     │ HTTP Request
                     ▼
┌─────────────────────────────────────────┐
│           Controllers (C)               │
│  AccountController, HomeController,     │
│  CaLamController, NhanVienController,   │
│  TaiKhoanController, DangKyCaController,│
│  PhanCongCaController, ThongKeController│
└──────┬──────────────────────┬───────────┘
       │                      │
       ▼                      ▼
┌──────────────┐    ┌─────────────────────┐
│  Models (M)  │    │    Views (V)        │
│  CaLam       │    │    Razor (.cshtml)  │
│  NhanVien    │    │    Layouts, Partials│
│  TaiKhoan    │    └─────────────────────┘
│  VaiTro      │
│  PhanCongCa  │
│  DangKyCa    │
└──────┬───────┘
       │
       ▼
┌──────────────────────────────────────────┐
│  ApplicationDbContext (EF Core)          │
│              ↓                           │
│         SQL Server Database              │
└──────────────────────────────────────────┘
```

---


## 4. THIẾT KẾ HỆ THỐNG

### 4.1. Cấu trúc thư mục dự án

```
DACS_Nhom19/
├── Controllers/           # Các controller xử lý logic
│   ├── AccountController.cs
│   ├── CaLamController.cs
│   ├── DangKyCaController.cs
│   ├── HomeController.cs
│   ├── NhanVienController.cs
│   ├── PhanCongCaController.cs
│   ├── TaiKhoanController.cs
│   └── ThongKeController.cs
├── Data/
│   └── ApplicationDbContext.cs    # DbContext - cấu hình Entity Framework
├── Helpers/
│   └── PasswordHelper.cs          # Helper mã hóa mật khẩu PBKDF2
├── Models/                # Các entity/model
│   ├── CaLam.cs
│   ├── DangKyCa.cs
│   ├── NhanVien.cs
│   ├── PhanCongCa.cs
│   ├── TaiKhoan.cs
│   └── VaiTro.cs
├── ViewModels/            # ViewModel cho các View phức tạp
│   ├── DangKyCaFormViewModel.cs
│   ├── DashboardViewModel.cs
│   ├── LoginViewModel.cs
│   ├── ThongKeCaNhanViewModel.cs
│   └── ThongKeTongHopViewModel.cs
├── Views/                 # Razor Views
│   ├── Account/
│   ├── CaLam/
│   ├── DangKyCa/
│   ├── Home/
│   ├── NhanVien/
│   ├── PhanCongCa/
│   ├── TaiKhoan/
│   ├── ThongKe/
│   └── Shared/
├── wwwroot/               # Static files (CSS, JS, images)
├── Program.cs             # Entry point + DI configuration
└── DACS_Nhom19.csproj     # Project file
```

### 4.2. Sơ đồ Use Case

#### Admin / Quản lý:
- Đăng nhập / Đăng xuất
- Xem Dashboard (tổng quan)
- Quản lý nhân viên (CRUD)
- Quản lý ca làm (CRUD)
- Phân công ca cho nhân viên
- Duyệt / Từ chối đăng ký ca
- Xem lịch phân công (Calendar)
- Thống kê tổng hợp
- Quản lý tài khoản (chỉ Admin)

#### Nhân viên:
- Đăng nhập / Đăng xuất
- Xem Dashboard cá nhân
- Đăng ký ca làm
- Xem/sửa/xóa đăng ký ca (khi chưa duyệt)
- Xem thống kê cá nhân

---


## 5. THIẾT KẾ CƠ SỞ DỮ LIỆU

### 5.1. Sơ đồ quan hệ (ERD)

```
┌──────────────┐        ┌──────────────┐
│   VaiTro     │        │   TaiKhoan   │
│──────────────│        │──────────────│
│ MaVaiTro (PK)│◄──────│ MaVaiTro (FK)│
│ TenVaiTro    │   1:N  │ MaTaiKhoan(PK)│
│ MoTa         │        │ TenDangNhap  │
└──────────────┘        │ MatKhau      │
                        │ HoTenHienThi │
                        │ TrangThai    │
                        │ NgayTao      │
                        │ LanDangNhapCuoi│
                        └──────┬───────┘
                               │ 1:1
                               ▼
                        ┌──────────────┐
                        │  NhanVien    │
                        │──────────────│
                        │ MaNhanVien(PK)│
                        │ MaNhanVienCode│
                        │ HoTen        │
                        │ GioiTinh     │
                        │ NgaySinh     │
                        │ SoDienThoai  │
                        │ Email        │
                        │ DiaChi       │
                        │ ChucVu       │
                        │ LoaiNhanVien │
                        │ NgayVaoLam   │
                        │ SoCaToiThieuTuan│
                        │ SoGioToiThieuTuan│
                        │ TrangThai    │
                        │ MaTaiKhoan(FK)│
                        └──────┬───────┘
                               │
              ┌────────────────┼────────────────┐
              │ 1:N            │ 1:N            │
              ▼                ▼                │
┌──────────────────┐  ┌──────────────────┐     │
│   PhanCongCa     │  │    DangKyCa      │     │
│──────────────────│  │──────────────────│     │
│ MaPhanCong (PK)  │  │ MaDangKy (PK)   │     │
│ MaNhanVien (FK)  │  │ MaNhanVien (FK)  │     │
│ MaCa (FK)        │  │ MaCa (FK)        │     │
│ NgayLam          │  │ NgayLam          │     │
│ TrangThai        │  │ NgayDangKy       │     │
│ GhiChu           │  │ TrangThai        │     │
│ NgayTao          │  │ GhiChu           │     │
│ NguoiTao (FK)    │  │ NguoiDuyet (FK)  │     │
└────────┬─────────┘  │ NgayDuyet        │     │
         │            └────────┬─────────┘     │
         │                     │               │
         └─────────┬───────────┘               │
                   │ N:1                       │
                   ▼                           │
            ┌──────────────┐                   │
            │    CaLam     │◄──────────────────┘
            │──────────────│
            │ MaCa (PK)    │
            │ MaCaCode     │
            │ TenCa        │
            │ GioBatDau    │
            │ GioKetThuc   │
            │ LoaiCa       │
            │ SoLuongNVToiThieu│
            │ SoLuongNVToiDa│
            │ TrangThai    │
            │ GhiChu       │
            │ SoGio (computed)│
            └──────────────┘
```

### 5.2. Mô tả chi tiết các bảng

#### Bảng VaiTro (Vai trò)
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|---------------|-----------|--------|
| MaVaiTro | int | PK, Identity | Mã vai trò |
| TenVaiTro | nvarchar(30) | Unique, NOT NULL | Tên vai trò (Admin, Quản lý, Nhân viên) |
| MoTa | nvarchar(255) | NULL | Mô tả vai trò |

#### Bảng TaiKhoan (Tài khoản)
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|---------------|-----------|--------|
| MaTaiKhoan | int | PK, Identity | Mã tài khoản |
| TenDangNhap | varchar(50) | Unique, NOT NULL | Tên đăng nhập |
| MatKhau | varchar(255) | NOT NULL | Mật khẩu (hash PBKDF2) |
| HoTenHienThi | nvarchar(100) | NOT NULL | Họ tên hiển thị |
| MaVaiTro | int | FK → VaiTro | Vai trò |
| TrangThai | nvarchar(20) | Default 'Hoạt động' | Trạng thái (Hoạt động/Khóa) |
| NgayTao | datetime2 | Default sysdatetime() | Ngày tạo |
| LanDangNhapCuoi | datetime2 | NULL | Lần đăng nhập cuối |

#### Bảng NhanVien (Nhân viên)
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|---------------|-----------|--------|
| MaNhanVien | int | PK, Identity | Mã nhân viên |
| MaNhanVienCode | varchar(20) | Unique, NOT NULL | Mã nhân viên (hiển thị) |
| HoTen | nvarchar(100) | NOT NULL | Họ và tên |
| GioiTinh | nvarchar(10) | NOT NULL | Giới tính |
| NgaySinh | date | NOT NULL | Ngày sinh |
| SoDienThoai | varchar(15) | Unique, NOT NULL | Số điện thoại |
| Email | varchar(100) | Unique (nullable) | Email |
| DiaChi | nvarchar(255) | NULL | Địa chỉ |
| ChucVu | nvarchar(50) | NOT NULL | Chức vụ |
| LoaiNhanVien | nvarchar(20) | NOT NULL | Loại (Full-time/Part-time) |
| NgayVaoLam | date | NOT NULL | Ngày vào làm |
| SoCaToiThieuTuan | int | NOT NULL | Số ca tối thiểu/tuần |
| SoGioToiThieuTuan | decimal(5,2) | NOT NULL | Số giờ tối thiểu/tuần |
| TrangThai | nvarchar(20) | Default 'Đang làm' | Trạng thái |
| MaTaiKhoan | int | FK → TaiKhoan, Unique | Liên kết tài khoản (1:1) |

#### Bảng CaLam (Ca làm)
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|---------------|-----------|--------|
| MaCa | int | PK, Identity | Mã ca |
| MaCaCode | varchar(20) | Unique, NOT NULL | Mã ca (hiển thị) |
| TenCa | nvarchar(50) | NOT NULL | Tên ca |
| GioBatDau | time | NOT NULL | Giờ bắt đầu |
| GioKetThuc | time | NOT NULL | Giờ kết thúc |
| LoaiCa | nvarchar(20) | NOT NULL | Loại ca (Chuẩn/Đặc biệt) |
| SoLuongNhanVienToiThieu | int | Default 1 | Số NV tối thiểu |
| SoLuongNhanVienToiDa | int | Default 1 | Số NV tối đa |
| TrangThai | nvarchar(20) | Default 'Hoạt động' | Trạng thái |
| GhiChu | nvarchar(255) | NULL | Ghi chú |
| SoGio | decimal(4,2) | Computed column | Số giờ (tự tính) |

#### Bảng PhanCongCa (Phân công ca)
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|---------------|-----------|--------|
| MaPhanCong | int | PK, Identity | Mã phân công |
| MaNhanVien | int | FK → NhanVien | Nhân viên |
| MaCa | int | FK → CaLam | Ca làm |
| NgayLam | date | NOT NULL | Ngày làm |
| TrangThai | nvarchar(30) | Default 'Đã phân công' | Trạng thái |
| GhiChu | nvarchar(255) | NULL | Ghi chú |
| NgayTao | datetime2 | Default sysdatetime() | Ngày tạo |
| NguoiTao | int | FK → TaiKhoan | Người tạo phân công |

**Ràng buộc đặc biệt:**
- Unique: (MaNhanVien, MaCa, NgayLam)
- Trigger: TRG_PhanCongCa_KiemTraChongGio (kiểm tra chồng giờ)
- Trigger: TRG_PhanCongCa_KiemTraSoLuongToiDa (kiểm tra số lượng NV tối đa)

#### Bảng DangKyCa (Đăng ký ca)
| Cột | Kiểu dữ liệu | Ràng buộc | Mô tả |
|-----|---------------|-----------|--------|
| MaDangKy | int | PK, Identity | Mã đăng ký |
| MaNhanVien | int | FK → NhanVien | Nhân viên đăng ký |
| MaCa | int | FK → CaLam | Ca đăng ký |
| NgayLam | date | NOT NULL | Ngày muốn làm |
| NgayDangKy | datetime2 | NOT NULL | Thời điểm đăng ký |
| TrangThai | nvarchar(20) | NOT NULL | Trạng thái (Chờ duyệt/Đã duyệt/Từ chối) |
| GhiChu | nvarchar(255) | NULL | Ghi chú |
| NguoiDuyet | int | FK → TaiKhoan | Người duyệt |
| NgayDuyet | datetime2 | NULL | Ngày duyệt |

**Ràng buộc đặc biệt:**
- Unique: (MaNhanVien, MaCa, NgayLam)

---


## 6. CHỨC NĂNG HỆ THỐNG

### 6.1. Đăng nhập / Xác thực

**Controller:** `AccountController`

- Sử dụng Cookie Authentication của ASP.NET Core
- Xác thực bằng tên đăng nhập + mật khẩu (PBKDF2 hash)
- Phân quyền theo vai trò (Role-based Authorization)
- Tự động rehash mật khẩu plain-text cũ sang PBKDF2 khi đăng nhập
- Session hết hạn sau 8 giờ
- Claims: NameIdentifier, Name, TenDangNhap, Role

### 6.2. Dashboard

**Controller:** `HomeController`

#### Dashboard Admin/Quản lý:
- Tổng số nhân viên đang làm
- Tổng số ca làm hoạt động
- Số đăng ký ca chờ duyệt
- Số ca hoàn thành hôm nay
- Số ca trong tuần
- Danh sách phân công hôm nay
- Danh sách đăng ký mới nhất (5 mới nhất)
- Nhân viên chưa đạt định mức tuần
- Biểu đồ số ca hoàn thành theo nhân viên (Chart.js)

#### Dashboard Nhân viên:
- Số ca trong tuần / Tổng giờ trong tuần
- Số ca chờ duyệt
- Số ca sắp tới
- Định mức tuần (số ca/giờ tối thiểu)
- Danh sách ca sắp tới (5 ca gần nhất)
- Danh sách đăng ký gần đây

### 6.3. Quản lý Ca Làm

**Controller:** `CaLamController` | **Quyền:** Admin, Quản lý

| Chức năng | Mô tả |
|-----------|--------|
| Danh sách | Hiển thị tất cả ca làm, hỗ trợ tìm kiếm theo từ khóa, lọc theo loại ca, trạng thái |
| Thêm mới | Tạo ca làm với mã, tên, giờ bắt đầu/kết thúc, loại, số lượng NV |
| Sửa | Cập nhật thông tin ca làm |
| Xóa | Xóa ca (chỉ khi chưa được dùng trong phân công/đăng ký) |
| Chi tiết | Xem thông tin chi tiết ca làm |

**Validation:**
- Không trùng mã ca, không trùng tên ca
- Giờ kết thúc > Giờ bắt đầu
- Số lượng NV tối thiểu >= 1
- Số lượng NV tối đa >= Số lượng tối thiểu

### 6.4. Quản lý Nhân Viên

**Controller:** `NhanVienController` | **Quyền:** Admin, Quản lý

| Chức năng | Mô tả |
|-----------|--------|
| Danh sách | Hiển thị NV, tìm kiếm theo mã/tên/SĐT/chức vụ, lọc theo loại/trạng thái |
| Thêm mới | Tạo nhân viên với đầy đủ thông tin cá nhân + liên kết tài khoản |
| Sửa | Cập nhật thông tin nhân viên |
| Xóa | Xóa NV (chỉ khi chưa có phân công/đăng ký) |
| Chi tiết | Xem thông tin chi tiết nhân viên |

**Validation:**
- Không trùng mã NV, SĐT, Email
- Tài khoản liên kết phải chưa được gắn cho NV khác

### 6.5. Phân Công Ca

**Controller:** `PhanCongCaController` | **Quyền:** Admin, Quản lý

| Chức năng | Mô tả |
|-----------|--------|
| Danh sách | Hiển thị phân công, tìm/lọc theo từ khóa, ngày, ca, trạng thái |
| Thêm mới | Phân công NV vào ca + ngày cụ thể |
| Sửa | Cập nhật thông tin phân công |
| Xóa | Xóa phân công |
| Hoàn thành | Đánh dấu ca đã hoàn thành |
| Hủy | Hủy phân công |
| Calendar | Xem lịch dạng FullCalendar (API JSON cho events) |

**Validation đặc biệt:**
- Chống trùng: 1 NV không thể phân vào cùng ca + cùng ngày 2 lần
- Chống chồng giờ: Kiểm tra thời gian ca mới không overlap với ca đã phân trong ngày

**Trạng thái phân công:** Đã phân công → Hoàn thành / Đổi ca / Nghỉ / Đã hủy

### 6.6. Đăng Ký Ca (Workflow)

**Controller:** `DangKyCaController` | **Quyền:** Admin, Quản lý, Nhân viên

**Quy trình:**
```
NV đăng ký → Chờ duyệt → Admin/QL duyệt → Tạo PhanCongCa tự động
                       → Admin/QL từ chối → Kết thúc (có lý do)
```

| Chức năng | Vai trò | Mô tả |
|-----------|---------|--------|
| Đăng ký | Nhân viên | Chọn ca + ngày muốn làm |
| Xem danh sách | Tất cả | NV xem của mình, QL xem tất cả |
| Sửa/Xóa | Nhân viên | Chỉ khi trạng thái "Chờ duyệt" |
| Duyệt | Admin, QL | Tạo bản ghi PhanCongCa, đổi trạng thái "Đã duyệt" |
| Từ chối | Admin, QL | Đổi trạng thái "Từ chối", ghi lý do |

**Validation:**
- Chống trùng đăng ký (cùng NV + cùng ca + cùng ngày)
- Kiểm tra chồng giờ với đăng ký khác + phân công đã có
- Khi duyệt: kiểm tra lại chồng giờ với phân công hiện tại

### 6.7. Quản lý Tài Khoản

**Controller:** `TaiKhoanController` | **Quyền:** Admin

| Chức năng | Mô tả |
|-----------|--------|
| Danh sách | Hiển thị tài khoản, tìm/lọc theo tên, vai trò, trạng thái |
| Thêm mới | Tạo tài khoản với tên đăng nhập, mật khẩu (hash), vai trò |
| Sửa | Cập nhật thông tin (mật khẩu có thể bỏ trống = giữ nguyên) |
| Xóa | Xóa tài khoản (chỉ khi chưa liên kết NV) |

### 6.8. Thống Kê

**Controller:** `ThongKeController`

#### Thống kê cá nhân (Nhân viên):
- Số ca hoàn thành trong khoảng thời gian
- Tổng số giờ làm
- So sánh với định mức (Đạt/Chưa đạt)
- Danh sách chi tiết các ca đã làm

#### Thống kê tổng hợp (Admin/Quản lý):
- Bảng tổng hợp tất cả nhân viên
- Số ca / Tổng giờ / Kết quả đạt định mức
- Hỗ trợ tìm kiếm theo tên/mã NV
- Lọc theo khoảng thời gian (mặc định: tuần hiện tại)
- Thống kê: Tổng NV, Đạt, Chưa đạt

---


## 7. BẢO MẬT

### 7.1. Xác thực (Authentication)

- **Cookie Authentication:** Sử dụng ASP.NET Core Cookie Authentication
- **Claims-based Identity:** Lưu thông tin user trong Cookie dưới dạng Claims
- **Session Expiry:** Cookie hết hạn sau 8 giờ (IsPersistent = true)
- **Login Path:** `/Account/Login`
- **Access Denied Path:** `/Account/AccessDenied`

### 7.2. Phân quyền (Authorization)

Sử dụng `[Authorize]` attribute với Role-based authorization:

| Controller | Quyền truy cập |
|------------|----------------|
| AccountController | Tất cả (Login AllowAnonymous) |
| HomeController | Tất cả (đã đăng nhập) |
| CaLamController | Admin, Quản lý |
| NhanVienController | Admin, Quản lý |
| TaiKhoanController | Admin |
| PhanCongCaController | Admin, Quản lý |
| DangKyCaController | Admin, Quản lý, Nhân viên |
| ThongKeController.CaNhan | Nhân viên |
| ThongKeController.TongHop | Admin, Quản lý |

### 7.3. Mã hóa mật khẩu

Sử dụng **PBKDF2** (Password-Based Key Derivation Function 2):

- **Thuật toán:** HMACSHA256
- **Salt size:** 128-bit (16 bytes)
- **Key size:** 256-bit (32 bytes)
- **Iterations:** 100,000
- **Định dạng lưu:** `PBKDF2|iterations|saltBase64|hashBase64`
- **Backward compatible:** Hỗ trợ verify mật khẩu plain-text cũ và tự rehash

### 7.4. Chống tấn công

- **CSRF Protection:** `[ValidateAntiForgeryToken]` trên tất cả POST action
- **SQL Injection:** Sử dụng Entity Framework Core (parameterized queries)
- **XSS:** Razor tự động HTML encode output
- **Timing Attack:** Sử dụng `CryptographicOperations.FixedTimeEquals()` khi verify hash

---

## 8. KẾT LUẬN

### 8.1. Kết quả đạt được

- Xây dựng thành công hệ thống quản lý ca làm việc nhà hàng với đầy đủ chức năng CRUD
- Workflow đăng ký - duyệt ca hoạt động chính xác
- Hệ thống kiểm tra chồng giờ, trùng lặp hoạt động ổn định
- Dashboard trực quan với biểu đồ và thống kê
- Phân quyền rõ ràng theo 3 vai trò
- Bảo mật tốt với PBKDF2, CSRF protection, Role-based Authorization
- Calendar view giúp quản lý dễ dàng theo dõi lịch phân công

### 8.2. Hạn chế

- Chưa có chức năng thông báo realtime (notification)
- Chưa hỗ trợ xuất báo cáo dạng Excel/PDF
- Chưa có API cho ứng dụng mobile
- Chưa có chức năng quản lý lương dựa trên ca làm

### 8.3. Hướng phát triển

- Tích hợp SignalR cho thông báo realtime
- Thêm chức năng export báo cáo (Excel, PDF)
- Xây dựng REST API để hỗ trợ mobile app
- Tích hợp module tính lương tự động
- Thêm chức năng đổi ca giữa các nhân viên
- Deploy lên Azure App Service

---

## PHỤ LỤC

### A. Cấu hình Program.cs

```csharp
// Services
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => { ... });
builder.Services.AddAuthorization();

// Middleware pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
```

### B. Các trạng thái trong hệ thống

| Entity | Trạng thái |
|--------|-----------|
| Tài khoản | Hoạt động, Khóa |
| Nhân viên | Đang làm, Nghỉ phép, Nghỉ việc |
| Ca làm | Hoạt động, Ngưng |
| Phân công ca | Đã phân công, Hoàn thành, Đổi ca, Nghỉ, Đã hủy |
| Đăng ký ca | Chờ duyệt, Đã duyệt, Từ chối |

---

*Báo cáo được tạo cho Đồ án Chuyên ngành - Nhóm 19*
