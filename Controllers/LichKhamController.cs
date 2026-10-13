using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham.Models;
using static QuanLyPhongKham.Models.LichKham;

namespace QuanLyPhongKham.Controllers
{
    public class LichKhamController : BaseController
    {
        private readonly QuanLyPhongKhamDB _context;

        public LichKhamController(QuanLyPhongKhamDB context)
        {
            _context = context;
        }

        // GET: LichKham
        public async Task<IActionResult> Index(int? maBS)
        {
            var query = _context.LichKham.Include(l => l.bs).AsQueryable();

            // 🛑 LỌC THEO MÃ BÁC SĨ NẾU CÓ PARAMETER TRÊN URL
            if (maBS.HasValue && maBS.Value > 0)
            {
                query = query.Where(l => l.mmbs == maBS.Value); // hoặc l.maBS tùy tên thuộc tính khóa ngoại trong Model LichKham của ông

                var bacSi = await _context.BacSi.FindAsync(maBS.Value);
                ViewBag.TenBacSiDangChon = bacSi?.tenBS;
                ViewBag.MaBSChon = maBS.Value;
            }

            var dsLichKham = await query.OrderByDescending(l => l.ngayKham).ToListAsync();
            return View(dsLichKham);
        }

        // GET: LichKham/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichKham = await _context.LichKham
                .Include(l => l.bs)
                .FirstOrDefaultAsync(m => m.maLK == id);
            if (lichKham == null)
            {
                return NotFound();
            }

            return View(lichKham);
        }

        // GET: LichKham/Create
        public IActionResult Create()
        {
            ViewData["mmbs"] = new SelectList(_context.BacSi, "maBS", "tenBS");
            return View();
        }

        // POST: LichKham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        // BỎ currentSL KHỎI [Bind]
        public async Task<IActionResult> Create([Bind("maLK,mmbs,ngayKham,gioBatdau,gioKetthuc,maxSL,TrangThaiLK")] LichKham lichKham)
        {
            // Ép buộc currentSL = 0 khi mới tạo lịch
            lichKham.currentSL = 0;
            ModelState.Remove("currentSL");

            if (ModelState.IsValid)
            {
                _context.Add(lichKham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["mmbs"] = new SelectList(_context.BacSi, "maBS", "tenBS", lichKham.mmbs);
            return View(lichKham);
        }

        // GET: LichKham/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lichKham = await _context.LichKham.FindAsync(id);
            if (lichKham == null)
            {
                return NotFound();
            }
            ViewData["mmbs"] = new SelectList(_context.BacSi, "maBS", "tenBS", lichKham.mmbs);
            return View(lichKham);
        }

        // POST: LichKham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("maLK,mmbs,ngayKham,gioBatdau,gioKetthuc,maxSL,TrangThaiLK")] LichKham lichKham)
        {
            if (id != lichKham.maLK) return NotFound();

            // 1. Lấy dữ liệu thực tế từ DB
            var lichKhamInDb = await _context.LichKham.AsNoTracking().FirstOrDefaultAsync(l => l.maLK == id);
            if (lichKhamInDb == null) return NotFound();

            // 2. Gán lại currentSL chuẩn từ DB
            lichKham.currentSL = lichKhamInDb.currentSL;
            ModelState.Remove("currentSL");

            // 🛑 3. BẮT BỘC KIỂM TRA Ở CONTROLLER (Vì currentSL vừa lấy từ DB ra)
            if (lichKham.maxSL < lichKhamInDb.currentSL)
            {
                ModelState.AddModelError("maxSL", $"Số lượng tối đa ({lichKham.maxSL}) không được nhỏ hơn số lượng đã đăng ký ({lichKhamInDb.currentSL})!");
            }

            if (lichKham.TrangThaiLK == TrangThaiLichKham.ConNhanDK && lichKhamInDb.currentSL >= lichKham.maxSL)
            {
                ModelState.AddModelError("TrangThaiLK", "Lịch khám đã đủ số lượng, không thể chuyển thành còn nhận đăng ký!");
            }

            if (ModelState.IsValid)
            {
                _context.Update(lichKham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["mmbs"] = new SelectList(_context.BacSi, "maBS", "tenBS", lichKham.mmbs);
            return View(lichKham);
        }

        // POST: LichKham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 🛑 1. CHẶN BẢO VỆ PHÂN QUYỀN ADMIN
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }

            var lichKham = await _context.LichKham.FindAsync(id);
            if (lichKham == null)
            {
                TempData["ErrorMessage"] = "Lịch khám không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // 🛑 2. KIỂM TRA ĐƠN ĐĂNG KÝ CỦA LỊCH KHÁM NÀY
            var dsDangKy = await _context.DangKyKham
                .Where(dk => dk.mlk == id)
                .ToListAsync();

            // Bẫy lỗi: Còn đơn chưa hoàn tất (Chưa phải DaKham hoặc DaHuy) HOẶC currentSL > 0
            bool coDonChuaXong = dsDangKy.Any(dk =>
                dk.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaKham &&
                dk.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaHuy);

            if (coDonChuaXong || lichKham.currentSL > 0)
            {
                TempData["ErrorMessage"] = $"KHÔNG THỂ XÓA: Lịch khám mã #{id} hiện đang có lượt đăng ký khám chưa xử lý xong!";
                return RedirectToAction(nameof(Index));
            }

            // 🛑 3. NẾU TẤT CẢ PHIẾU ĐỀU ĐÃ ĐÃ KHÁM HOẶC ĐÃ HỦY -> DỌN PHIẾU CŨ VÀ XÓA LỊCH
            if (dsDangKy.Any())
            {
                _context.DangKyKham.RemoveRange(dsDangKy);
            }

            _context.LichKham.Remove(lichKham);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa thành công Lịch khám mã #{id}!";
            return RedirectToAction(nameof(Index));
        }

        private bool LichKhamExists(int id)
        {
            return _context.LichKham.Any(e => e.maLK == id);
        }
    }
}