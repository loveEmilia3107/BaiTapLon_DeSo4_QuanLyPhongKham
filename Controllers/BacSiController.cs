using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Controllers
{
    public class BacSiController : BaseController
    {
        private readonly QuanLyPhongKhamDB _context;

        public BacSiController(QuanLyPhongKhamDB context)
        {
            _context = context;
        }

        // GET: BacSi
        public async Task<IActionResult> Index(string? SearchType, string? SearchTen, int? MaCK, int? MinKinhNghiem, double? MaxGiaKham, string? SortBy = "ten_asc")
        {
            bool isFiltering = !string.IsNullOrEmpty(SearchType) || !string.IsNullOrEmpty(SearchTen)
                || (MaCK.HasValue && MaCK.Value > 0)
                || MinKinhNghiem.HasValue
                || MaxGiaKham.HasValue;
            ViewBag.IsFiltering = isFiltering;

            if (SearchType == "ten")
            {
                MaCK = null;
                MinKinhNghiem = null;
                MaxGiaKham = null;
            }
            else if (SearchType == "ck")
            {
                SearchTen = null;
                MinKinhNghiem = null;
                MaxGiaKham = null;
            }
            else if (SearchType == "kn")
            {
                SearchTen = null;
                MaCK = null;
                MaxGiaKham = null;
            }
            else if (SearchType == "gia")
            {
                SearchTen = null;
                MaCK = null;
                MinKinhNghiem = null;
            }
            else // Trường hợp "Tất cả" hoặc SearchType rỗng
            {
                SearchType = null;
                SearchTen = null;
                MaCK = null;
                MinKinhNghiem = null;
                MaxGiaKham = null;
            }

            // 1. Query gốc + Include bảng Chuyên khoa
            var query = _context.BacSi
                .Include(b => b.mCK)
                .AsQueryable();

            if (SearchType == "ten" && !string.IsNullOrWhiteSpace(SearchTen))
            {
                query = query.Where(b => b.tenBS.Contains(SearchTen));
            }
            else if (SearchType == "ck" && MaCK.HasValue && MaCK.Value > 0)
            {
                query = query.Where(b => b.mck == MaCK.Value);
            }
            else if (SearchType == "kn" && MinKinhNghiem.HasValue)
            {
                query = query.Where(b => b.namKN >= MinKinhNghiem.Value);
            }
            else if (SearchType == "gia" && MaxGiaKham.HasValue)
            {
                query = query.Where(b => b.giaKham <= MaxGiaKham.Value);
            }

            query = SortBy switch
            {
                "ten_desc" => query.OrderByDescending(b => b.tenBS),
                "kinhnghiem_desc" => query.OrderByDescending(b => b.namKN),
                "giakham_asc" => query.OrderBy(b => b.giaKham),
                "giakham_desc" => query.OrderByDescending(b => b.giaKham),
                _ => query.OrderBy(b => b.tenBS)
            };

            ViewBag.DanhSachChuyenKhoa = await _context.ChuyenKhoa.ToListAsync();
            ViewBag.SearchType = SearchType;
            ViewBag.SearchTen = SearchTen;
            ViewBag.MaCK = MaCK;
            ViewBag.MinKinhNghiem = MinKinhNghiem;
            ViewBag.MaxGiaKham = MaxGiaKham;
            ViewBag.SortBy = SortBy;

            return View(await query.ToListAsync());
        }

        // GET: BacSi/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bacSi = await _context.BacSi
                .Include(b => b.mCK)
                .FirstOrDefaultAsync(m => m.maBS == id);
            if (bacSi == null)
            {
                return NotFound();
            }

            return View(bacSi);
        }

        // GET: BacSi/Create
        public IActionResult Create()
        {
            // 🛑 Dùng trực tiếp IsAdmin từ BaseController
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["mck"] = new SelectList(_context.ChuyenKhoa, "maCK", "tenCK");
            return View();
        }

        // POST: BacSi/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("maBS,tenBS,mck,nsBS,Gt,sdtBS,emailBS,TrD,namKN,giaKham,TrangThaiBS")] BacSi bacSi)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("mCK");
            ModelState.Remove("mCKNavigation"); // Bỏ qua nếu có property điều hướng sinh tự động
            ModelState.Remove("LichKhams");    // Bỏ qua danh sách lịch khám liên kết

            if (ModelState.IsValid)
            {
                _context.Add(bacSi);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm bác sĩ mới thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Đồng bộ Key ViewData["mck"] đồng nhất với hàm GET
            ViewData["mck"] = new SelectList(_context.ChuyenKhoa, "maCK", "tenCK", bacSi.mck);
            return View(bacSi);
        }

        // GET: BacSi/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var bacSi = await _context.BacSi.FindAsync(id);
            if (bacSi == null)
            {
                return NotFound();
            }
            ViewData["mck"] = new SelectList(_context.ChuyenKhoa, "maCK", "tenCK", bacSi.mck);
            return View(bacSi);
        }

        // POST: BacSi/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("maBS,tenBS,mck,nsBS,Gt,sdtBS,emailBS,TrD,namKN,giaKham,TrangThaiBS")] BacSi bacSi)
        {
            if (id != bacSi.maBS)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(bacSi);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BacSiExists(bacSi.maBS))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["mck"] = new SelectList(_context.ChuyenKhoa, "maCK", "tenCK", bacSi.mck);
            return View(bacSi);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. Gác cổng phân quyền Admin
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }

            var bacSi = await _context.BacSi.FindAsync(id);
            if (bacSi == null)
            {
                TempData["ErrorMessage"] = "Bác sĩ không tồn tại trên hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            // 2. Lấy tất cả Lịch khám của bác sĩ này
            var dsLichKham = await _context.LichKham
                .Where(l => l.maLK == id) // Hoặc l.mck tùy tên thuộc tính trong Model LichKham
                .ToListAsync();

            if (dsLichKham.Any())
            {
                var listMaLK = dsLichKham.Select(l => l.maLK).ToList();

                // 🛑 3. BẪY BẢO VỆ: Kiểm tra xem có đơn đăng ký khám nào chưa khám / chưa hủy không
                bool coDonChuaXong = await _context.DangKyKham
                    .AnyAsync(dk => listMaLK.Contains(dk.mlk) &&
                                    dk.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaKham &&
                                    dk.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaHuy);

                if (coDonChuaXong)
                {
                    TempData["ErrorMessage"] = $"Không thể xóa Bác sĩ '{bacSi.tenBS}' vì vẫn còn đơn đăng ký khám chưa xử lý xong! Vui lòng hoàn tất hoặc hủy các đơn đăng ký trước.";
                    return RedirectToAction(nameof(Index));
                }

                // 🛑 4. Nếu tất cả đơn đã DaKham hoặc DaHuy -> Dọn sạch phiếu cũ & Lịch khám
                var dsDangKyCu = await _context.DangKyKham
                    .Where(dk => listMaLK.Contains(dk.mlk))
                    .ToListAsync();

                if (dsDangKyCu.Any())
                {
                    _context.DangKyKham.RemoveRange(dsDangKyCu);
                }

                _context.LichKham.RemoveRange(dsLichKham);
            }

            // 5. Xóa Bác sĩ
            _context.BacSi.Remove(bacSi);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa thành công bác sĩ '{bacSi.tenBS}'!";
            return RedirectToAction(nameof(Index));
        }

        private bool BacSiExists(int id)
        {
            return _context.BacSi.Any(e => e.maBS == id);
        }
    }
}
