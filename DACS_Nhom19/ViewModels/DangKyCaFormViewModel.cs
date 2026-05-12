using System.ComponentModel.DataAnnotations;

namespace DACS_Nhom19.ViewModels
{
    public class DangKyCaFormViewModel
    {
        [Required]
        public int MaNhanVien { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ca làm")]
        public int MaCa { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày làm")]
        [DataType(DataType.Date)]
        public DateOnly NgayLam { get; set; }

        public string? GhiChu { get; set; }
    }
}