using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyPhongKham.Models
{
    public class BacSi
    {
        [Key] public int maBS { get; set; }

        [Required(ErrorMessage = "Vui long nhap ho ten bac si!")]
        [StringLength(75)]
        public string tenBS { get; set; } = string.Empty;

        [Required][ForeignKey("mCK")] public int mck { get; set; }
        public ChuyenKhoa? mCK { get; set; }
        [DataType(DataType.Date)] public DateTime nsBS { get; set; }
        public bool Gt { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập SDT!")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^0[1-9][0-9]{8}$", ErrorMessage = "SDT sai định dạng!")]
        public string sdtBS { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email!")]
        [EmailAddress(ErrorMessage = "Email chưa đúng định dạng!")]
        [StringLength(100)]
        public string emailBS { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập trình độ học vấn của bác sĩ!")]
        [StringLength(100)] public string TrD { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số năm kinh nghiệm!")]
        [Range(0, 100, ErrorMessage = "Vui lòng nhập số năm hợp lệ!")]
        public int namKN { get; set; }

        [Required(ErrorMessage = "Phi kham la bao nhieu???")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Vui lòng nhập giá tiền hợp lệ!")]
        public double giaKham { get; set; }

        public enum TrangThaiBacSi
        {
            [Display(Name = "Đang trong ca làm")]
            DangLamViec,
            [Display(Name = "Tạm nghỉ")]
            TamNghi,
            [Display(Name = "Đã nghỉ làm")]
            DaNghiLam
        }
        public TrangThaiBacSi TrangThaiBS { get; set; } = TrangThaiBacSi.DangLamViec;
        public List<LichKham> dsLK { get; set; } = new List<LichKham>();
    }
}