using System.Collections.Generic;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Models
{
    public class BacSiFilterViewModel
    {
        public string? SearchTen { get; set; }           // Tìm theo tên BS
        public int? MaCK { get; set; }                   // Lọc theo Chuyên khoa
        public int? MinKinhNghiem { get; set; }          // Số năm kinh nghiệm tối thiểu
        public double? MaxGiaKham { get; set; }         // Giá khám tối đa

        public string? SortBy { get; set; } = "ten_asc";

        public List<BacSi> DanhSachBacSi { get; set; } = new List<BacSi>();
        
        public List<ChuyenKhoa> DanhSachChuyenKhoa { get; set; } = new List<ChuyenKhoa>();
    }
}