using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyPhongKham.Models
{
    public class DangKyKham // : IValidatableObject
    {
        [Key] public int maPhieu { get; set; }
        [Required][ForeignKey("bn01")] public int ooBN { get; set; }
        public BenhNhan? bn01 { get; set; }

        [Required][ForeignKey("mLK")] public int mlk { get; set; }
        public LichKham? mLK { get; set; }

        [DataType(DataType.Date)] public DateTime ngayDK { get; set; }

        [StringLength(255, ErrorMessage = "Xin hãy viết vắn tắt lý do khám!")] public string? LiDo { get; set; }

        public enum TrangThaiDangKy
        {
            [Display(Name = "Chờ xác nhận lịch khám")]
            ChoXacNhan,
            [Display(Name = "Đã xác nhận(Trong thời gian này vẫn có thể cập nhật/hủy lịch khám)")]
            DaXacNhan,
            [Display(Name = "Đã khám xong!")]
            DaKham,
            [Display(Name = "Lịch bị hủy")]
            DaHuy
        }
        public TrangThaiDangKy TrangThaiDK { get; set; } = TrangThaiDangKy.ChoXacNhan;

        /* public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
            if (ngayDK.Date < DateTime.Today)
            {
                yield return new ValidationResult(
                    "Ngày đăng ký không được chọn ngày trong quá khứ!",
                    new[] { nameof(ngayDK) }
                );
            }
        } */
    }
}