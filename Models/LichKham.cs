using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.CodeAnalysis.FlowAnalysis;
namespace QuanLyPhongKham.Models
{
    public class LichKham : IValidatableObject
    {
        [Key] public int maLK { get; set; }
        [Required][ForeignKey("bs")] public int mmbs { get; set; }
        public BacSi? bs { get; set; }
        [DataType(DataType.Date)] public DateTime ngayKham { get; set; }
        [DataType(DataType.Time)][DisplayFormat(DataFormatString = "{0:HH:mm}")] public TimeOnly gioBatdau { get; set; }
        [DataType(DataType.Time)][DisplayFormat(DataFormatString = "{0:HH:mm}")] public TimeOnly gioKetthuc { get; set; }

        [Range(0, 999, ErrorMessage = "Quá nhiều lịch khám hoặc SL không hợp lệ")]
        [Required] public int maxSL { get; set; }

        [Range(0, 999)]
        public int currentSL { get; set; } = 0;

        public enum TrangThaiLichKham
        {
            [Display(Name = "Còn nhận đăng ký khám")]
            ConNhanDK,
            [Display(Name = "Đã đủ lịch")]
            DaDu,
            [Display(Name = "Lịch khám bị tạm ngưng")]
            TamNgung
        }
        public TrangThaiLichKham TrangThaiLK { get; set; } = TrangThaiLichKham.TamNgung;
        public List<DangKyKham> dsDKK { get; set; } = new List<DangKyKham>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // gio bat dau khong duoc nho hon gio ket thuc
            if (gioBatdau >= gioKetthuc)
            {
                yield return new ValidationResult(
                    "Giờ bắt đầu phải nhỏ hơn giờ kết thúc",
                    new[] { nameof(gioBatdau) }
                );
            }

            // chong nhap lieu sai
            if (currentSL > maxSL)
            {
                yield return new ValidationResult(
                    "Số lượng khám hiện tại vượt quá số lượng tối đa!",
                    new[] { nameof(currentSL) }
                );
            }

            if (TrangThaiLK == TrangThaiLichKham.ConNhanDK && currentSL >= maxSL)
            {
                yield return new ValidationResult(
                   "Lịch khám đã đủ số lượng, không thể sửa thành còn nhận đăng ký được!",
                    new[] { nameof(TrangThaiLK) }
                );
            }
        }
    }
}