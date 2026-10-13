using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Controllers
{
    public class HomeController : Controller
    {
        private readonly QuanLyPhongKhamDB _context;

        public HomeController(QuanLyPhongKhamDB context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy số liệu thống kê nhanh để hiện lên Dashboard
            ViewBag.SoLuongBacSi = await _context.BacSi.CountAsync();
            ViewBag.SoLuongChuyenKhoa = await _context.ChuyenKhoa.CountAsync();
            ViewBag.SoLuongBenhNhan = await _context.BenhNhan.CountAsync();
            ViewBag.SoLuongDangKyKham = await _context.DangKyKham.CountAsync();

            // Lấy danh sách bác sĩ nổi bật/mới nhất (tối đa 5 bác sĩ)
            var dsBacSi = await _context.BacSi
                .Include(b => b.mCK)
                .Take(5)
                .ToListAsync();

            return View(dsBacSi);
        }
    }
}