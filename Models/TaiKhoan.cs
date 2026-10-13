using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace QuanLyPhongKham.Models
{
    [Index(nameof(username), IsUnique = true)]
    public class TaiKhoan
    {
        [Key] public int maTK { get; set; }

        [StringLength(30, ErrorMessage = "Tên tài khoản không được dài hơn 30 ký tự")]
        [Required(ErrorMessage = "Tên tài khoản không được để trống!")]
        public string username { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống!")]
        [DataType(DataType.Password)]
        [StringLength(64, MinimumLength = 8, ErrorMessage = "Độ dài mật khẩu phải nằm trong đoạn [8; 64] ký tự!")]
        public string password { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập SDT!")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^0[1-9][0-9]{8}$", ErrorMessage = "SDT sai định dạng!")]
        public string sdt { get; set; }  = null!;

        [EmailAddress(ErrorMessage = "Email chưa đúng định dạng!")]
        [StringLength(100)] public string? email { get; set; }

        [Range(0, 1, ErrorMessage = "Role không hợp lệ (Chỉ nhận 0 hoặc 1)!")]
        public int role { get; set; } = 1;
        public enum TrangThaiTaiKhoan
        {
            [Display(Name = "Đang hoạt động")]
            DangHoatDong,
            [Display(Name = "Đang bị khóa")]
            BiKhoa
        }
        public TrangThaiTaiKhoan TrangThaiTK { get; set; } = TrangThaiTaiKhoan.DangHoatDong;
    }
}