using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace QuanLyPhongKham.Models
{
    public class BenhNhan
    {
        [Key] public int maBN { get; set; }
        [Required][ForeignKey("mTK")] public int mtk { get; set; }
        public TaiKhoan? mTK { get; set; }
        [Required(ErrorMessage = "Vui lòng nhập họ tên!")][StringLength(75)] public string tenBN { get; set; } = null!;
        [DataType(DataType.Date)] public DateTime nsBN { get; set; }

        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^0[1-9][0-9]{8}$", ErrorMessage = "SDT sai định dạng!")]
        public string? sdtBN { get; set; }

        [EmailAddress(ErrorMessage = "Email chưa đúng định dạng!")]
        [StringLength(100)]
        public string? email { get; set; }

        [StringLength(200, ErrorMessage = "Địa chỉ quá dài")]
        public string? diachi { get; set; }
        public List<DangKyKham> dsDKK { get; set; } = new List<DangKyKham>();
    }
}