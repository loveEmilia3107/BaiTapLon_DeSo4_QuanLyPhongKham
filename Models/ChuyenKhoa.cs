using System.ComponentModel.DataAnnotations;
namespace QuanLyPhongKham.Models
{
    public class ChuyenKhoa
    {
        [Key] public int maCK { get; set; }
        [Required(ErrorMessage = "Không được để trống tên CK!")] public string tenCK { get; set; } = string.Empty;
        [Required(ErrorMessage = "Bạn cần mô tả chuyên khoa để người dùng hiểu!")]
        public string motaCK { get; set; } = string.Empty;
        public enum TrangThaiChuyenKhoa
        {
            [Display(Name = "Đang hoạt động")]
            DangHoatDong,
            [Display(Name = "Ngừng hoạt động")]
            NgungHoatDong
        }
        public TrangThaiChuyenKhoa TrangThaiCK { get; set; } = TrangThaiChuyenKhoa.DangHoatDong;
        public List<BacSi> dsBS { get; set; } = new List<BacSi>();
    }
}