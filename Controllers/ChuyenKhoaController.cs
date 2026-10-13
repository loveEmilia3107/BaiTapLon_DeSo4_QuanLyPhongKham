using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Controllers
{
    public class ChuyenKhoaController : BaseController
    {
        private readonly QuanLyPhongKhamDB _context;

        public ChuyenKhoaController(QuanLyPhongKhamDB context)
        {
            _context = context;
        }

        // GET: ChuyenKhoa
        public async Task<IActionResult> Index()
        {
            return View(await _context.ChuyenKhoa.ToListAsync());
        }

        // GET: ChuyenKhoa/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var chuyenKhoa = await _context.ChuyenKhoa
                .FirstOrDefaultAsync(m => m.maCK == id);

            if (chuyenKhoa == null) return NotFound();

            return View(chuyenKhoa);
        }

        // GET: ChuyenKhoa/Create
        public IActionResult Create()
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: ChuyenKhoa/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("maCK,tenCK,motaCK,TrangThaiCK")] ChuyenKhoa chuyenKhoa)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }

            if (ModelState.IsValid)
            {
                _context.Add(chuyenKhoa);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm chuyên khoa mới thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(chuyenKhoa);
        }

        // GET: ChuyenKhoa/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }

            if (id == null) return NotFound();

            var chuyenKhoa = await _context.ChuyenKhoa.FindAsync(id);
            if (chuyenKhoa == null) return NotFound();

            return View(chuyenKhoa);
        }

        // POST: ChuyenKhoa/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("maCK,tenCK,motaCK,TrangThaiCK")] ChuyenKhoa chuyenKhoa)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }

            if (id != chuyenKhoa.maCK) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var oldCK = await _context.ChuyenKhoa.AsNoTracking().FirstOrDefaultAsync(c => c.maCK == id);

                    if (oldCK != null)
                    {
                        // 🛑 CASE 1: KHOA CHUYỂN TỪ ĐANG HOẠT ĐỘNG -> TẠM DỪNG / NGỪNG HOẠT ĐỘNG
                        if (oldCK.TrangThaiCK == ChuyenKhoa.TrangThaiChuyenKhoa.DangHoatDong &&
                            chuyenKhoa.TrangThaiCK != ChuyenKhoa.TrangThaiChuyenKhoa.DangHoatDong)
                        {
                            var dsBacSi = await _context.BacSi.Where(b => b.mck == id).ToListAsync();
                            var listMaBS = dsBacSi.Select(b => b.maBS).ToList();

                            if (dsBacSi.Any())
                            {
                                // Đổi Bác sĩ Đang làm việc -> Tạm nghỉ (Giữ nguyên Bác sĩ Đã nghỉ làm)
                                foreach (var bs in dsBacSi)
                                {
                                    if (bs.TrangThaiBS == BacSi.TrangThaiBacSi.DangLamViec)
                                    {
                                        bs.TrangThaiBS = BacSi.TrangThaiBacSi.TamNghi;
                                    }
                                }
                                _context.UpdateRange(dsBacSi);

                                // Đổi Lịch khám sang Tạm ngưng & Reset currentSL = 0
                                var dsLichKham = await _context.LichKham.Where(l => listMaBS.Contains(l.mmbs)).ToListAsync();
                                var listMaLK = dsLichKham.Select(l => l.maLK).ToList();

                                if (dsLichKham.Any())
                                {
                                    foreach (var lk in dsLichKham)
                                    {
                                        lk.currentSL = 0;
                                        lk.TrangThaiLK = LichKham.TrangThaiLichKham.TamNgung;
                                    }
                                    _context.UpdateRange(dsLichKham);

                                    // Wipe sạch toàn bộ Đăng ký khám dính lịch
                                    var dsDangKyWipe = await _context.DangKyKham.Where(d => listMaLK.Contains(d.mlk)).ToListAsync();
                                    if (dsDangKyWipe.Any())
                                    {
                                        _context.DangKyKham.RemoveRange(dsDangKyWipe);
                                    }
                                }
                            }
                        }
                        // 🛑 CASE 2: KHOA MỞ LẠI TỪ TẠM DỪNG / NGỪNG HOẠT ĐỘNG -> ĐANG HOẠT ĐỘNG
                        else if (oldCK.TrangThaiCK != ChuyenKhoa.TrangThaiChuyenKhoa.DangHoatDong &&
                                 chuyenKhoa.TrangThaiCK == ChuyenKhoa.TrangThaiChuyenKhoa.DangHoatDong)
                        {
                            var dsBacSi = await _context.BacSi.Where(b => b.mck == id).ToListAsync();
                            var listMaBS = dsBacSi.Select(b => b.maBS).ToList();

                            if (dsBacSi.Any())
                            {
                                // Khôi phục Bác sĩ Tạm nghỉ -> Đang làm việc
                                foreach (var bs in dsBacSi)
                                {
                                    if (bs.TrangThaiBS == BacSi.TrangThaiBacSi.TamNghi)
                                    {
                                        bs.TrangThaiBS = BacSi.TrangThaiBacSi.DangLamViec;
                                    }
                                }
                                _context.UpdateRange(dsBacSi);

                                // Khôi phục Lịch khám phụ thuộc vào currentSL trên maxSL
                                var dsLichKham = await _context.LichKham.Where(l => listMaBS.Contains(l.mmbs)).ToListAsync();
                                if (dsLichKham.Any())
                                {
                                    foreach (var lk in dsLichKham)
                                    {
                                        if (lk.TrangThaiLK == LichKham.TrangThaiLichKham.TamNgung)
                                        {
                                            // Còn chỗ -> ConNhanDK, Đã đủ chỗ -> DaDu
                                            if (lk.currentSL < lk.maxSL)
                                            {
                                                lk.TrangThaiLK = LichKham.TrangThaiLichKham.ConNhanDK;
                                            }
                                            else
                                            {
                                                lk.TrangThaiLK = LichKham.TrangThaiLichKham.DaDu;
                                            }
                                        }
                                    }
                                    _context.UpdateRange(dsLichKham);
                                }
                            }
                        }
                    }

                    _context.Update(chuyenKhoa);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Cập nhật Chuyên Khoa '{chuyenKhoa.tenCK}' và đồng bộ trạng thái hệ thống thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChuyenKhoaExists(chuyenKhoa.maCK)) return NotFound();
                    else throw;
                }
            }
            return View(chuyenKhoa);
        }

        // POST: ChuyenKhoa/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền thực hiện chức năng này!";
                return RedirectToAction(nameof(Index));
            }

            var chuyenKhoa = await _context.ChuyenKhoa.FindAsync(id);
            if (chuyenKhoa == null)
            {
                TempData["ErrorMessage"] = "Chuyên khoa không tồn tại trên hệ thống!";
                return RedirectToAction(nameof(Index));
            }

            // 🛑 XÓA CASCADE SẠCH SẼ TỪ TRÊN XUỐNG DƯỚI
            var dsBacSi = await _context.BacSi.Where(b => b.mck == id).ToListAsync();
            var listMaBS = dsBacSi.Select(b => b.maBS).ToList();

            var dsLichKham = await _context.LichKham.Where(l => listMaBS.Contains(l.mmbs)).ToListAsync();
            var listMaLK = dsLichKham.Select(l => l.maLK).ToList();

            var dsDangKy = await _context.DangKyKham.Where(d => listMaLK.Contains(d.mlk)).ToListAsync();

            if (dsDangKy.Any())
            {
                _context.DangKyKham.RemoveRange(dsDangKy);
            }

            if (dsLichKham.Any())
            {
                _context.LichKham.RemoveRange(dsLichKham);
            }

            if (dsBacSi.Any())
            {
                _context.BacSi.RemoveRange(dsBacSi);
            }

            _context.ChuyenKhoa.Remove(chuyenKhoa);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa vĩnh viễn Chuyên khoa '{chuyenKhoa.tenCK}' cùng toàn bộ dữ liệu Bác sĩ, Lịch khám & Đăng ký khám liên quan!";
            return RedirectToAction(nameof(Index));
        }

        private bool ChuyenKhoaExists(int id)
        {
            return _context.ChuyenKhoa.Any(e => e.maCK == id);
        }
    }
}