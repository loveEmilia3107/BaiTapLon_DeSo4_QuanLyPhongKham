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
    public class BenhNhanController : BaseController
    {
        private readonly QuanLyPhongKhamDB _context;

        public BenhNhanController(QuanLyPhongKhamDB context)
        {
            _context = context;
        }

        // GET: BenhNhan hoặc BenhNhan?all=true
        public async Task<IActionResult> Index(bool all = false)
        {
            var query = _context.BenhNhan.Include(b => b.mTK).AsQueryable();

            if (IsAdmin && all)
            {
                ViewBag.IsShowAll = true;
            }
            else
            {
                query = query.Where(b => b.mtk == CurrentAccId);
                ViewBag.IsShowAll = false;
            }

            var dsBenhNhan = await query.ToListAsync();
            return View(dsBenhNhan);
        }

        // GET: BenhNhan/Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var benhNhan = await _context.BenhNhan
                .Include(b => b.mTK)
                .FirstOrDefaultAsync(m => m.maBN == id);

            if (benhNhan == null) return NotFound();

            // Phan quyen
            if (!IsAdmin && benhNhan.mtk != CurrentAccId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem hồ sơ bệnh nhân này!";
                return RedirectToAction(nameof(Index));
            }

            return View(benhNhan);
        }

        // GET: BenhNhan/Create
        public async Task<IActionResult> Create()
        {
            if (CurrentAccId <= 0)
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            var currentAcc = await _context.TaiKhoan.FindAsync(CurrentAccId);
            if (currentAcc != null)
            {
                ViewBag.DefaultSDT = currentAcc.sdt;
                ViewBag.DefaultEmail = currentAcc.email;
            }

            // Kiem tra admin
            if (IsAdmin)
            {
                ViewData["mtk"] = new SelectList(_context.TaiKhoan.Select(t => new
                {
                    t.maTK,
                    Display = $"[Mã #{t.maTK}] - {t.username}"
                }), "maTK", "Display", CurrentAccId);
            }

            return View();
        }

        // POST: BenhNhan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("mtk,tenBN,nsBN,sdtBN,email,diachi")] BenhNhan benhNhan)
        {
            if (CurrentAccId <= 0)
            {
                return RedirectToAction("Login", "TaiKhoan");
            }

            //  NẾU KHÔNG PHẢI ADMIN -> BẮT BUỘC ÉP mtk LÀ CHÍNH TÀI KHOẢN ĐANG ĐĂNG NHẬP
            if (!IsAdmin || benhNhan.mtk <= 0)
            {
                benhNhan.mtk = CurrentAccId;
            }

            var targetAcc = await _context.TaiKhoan.FindAsync(benhNhan.mtk);
            if (targetAcc != null && string.IsNullOrWhiteSpace(benhNhan.sdtBN))
            {
                benhNhan.sdtBN = targetAcc.sdt;
                ModelState.Remove("sdtBN");
            }

            ModelState.Remove("maTK");
            ModelState.Remove("mTK");

            if (ModelState.IsValid)
            {
                _context.Add(benhNhan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tạo hồ sơ bệnh nhân '{benhNhan.tenBN}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            if (IsAdmin)
            {
                ViewData["mtk"] = new SelectList(_context.TaiKhoan.Select(t => new
                {
                    t.maTK,
                    Display = $"[Mã #{t.maTK}] - {t.username}"
                }), "maTK", "Display", benhNhan.mtk);
            }

            return View(benhNhan);
        }

        // GET: BenhNhan/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var benhNhan = await _context.BenhNhan.FindAsync(id);
            if (benhNhan == null) return NotFound();

            if (!IsAdmin && benhNhan.mtk != CurrentAccId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa hồ sơ bệnh nhân này!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["mtk"] = new SelectList(_context.TaiKhoan.Select(t => new
            {
                t.maTK,
                Display = $"[Mã #{t.maTK}] - {t.username}"
            }), "maTK", "Display", benhNhan.mtk);

            return View(benhNhan);
        }

        // POST: BenhNhan/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("maBN,mtk,tenBN,nsBN,sdtBN,email,diachi")] BenhNhan benhNhan)
        {
            if (id != benhNhan.maBN) return NotFound();

            var oldBN = await _context.BenhNhan.AsNoTracking().FirstOrDefaultAsync(b => b.maBN == id);
            if (oldBN == null) return NotFound();

            if (!IsAdmin && oldBN.mtk != CurrentAccId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa hồ sơ bệnh nhân này!";
                return RedirectToAction(nameof(Index));
            }

            if (!IsAdmin)
            {
                benhNhan.mtk = oldBN.mtk;
            }

            ModelState.Remove("mTK");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(benhNhan);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Cập nhật hồ sơ bệnh nhân '{benhNhan.tenBN}' thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BenhNhanExists(benhNhan.maBN)) return NotFound();
                    else throw;
                }
            }

            ViewData["mtk"] = new SelectList(_context.TaiKhoan.Select(t => new
            {
                t.maTK,
                Display = $"[Mã #{t.maTK}] - {t.username}"
            }), "maTK", "Display", benhNhan.mtk);

            return View(benhNhan);
        }

        // POST: BenhNhan/Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var benhNhan = await _context.BenhNhan.FindAsync(id);
            if (benhNhan == null)
            {
                TempData["ErrorMessage"] = "Hồ sơ Bệnh nhân không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            if (!IsAdmin && benhNhan.mtk != CurrentAccId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa hồ sơ bệnh nhân này!";
                return RedirectToAction(nameof(Index));
            }

            var dsDangKy = await _context.DangKyKham.Where(dk => dk.ooBN == id).ToListAsync();

            if (dsDangKy.Any())
            {
                bool coDonChuaXong = dsDangKy.Any(dk =>
                    dk.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaKham &&
                    dk.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaHuy);

                if (coDonChuaXong)
                {
                    TempData["ErrorMessage"] = $"KHÔNG THỂ XÓA: Bệnh nhân '{benhNhan.tenBN}' hiện vẫn còn lịch đăng ký khám chưa hoàn tất hoặc chưa hủy!";
                    return RedirectToAction(nameof(Index));
                }

                var listMaLK = dsDangKy.Select(dk => dk.mlk).Distinct().ToList();
                var dsLichKham = await _context.LichKham.Where(lk => listMaLK.Contains(lk.maLK)).ToListAsync();

                foreach (var dk in dsDangKy)
                {
                    var lk = dsLichKham.FirstOrDefault(x => x.maLK == dk.mlk);
                    if (lk != null && lk.currentSL > 0)
                    {
                        lk.currentSL -= 1;
                    }
                }

                _context.DangKyKham.RemoveRange(dsDangKy);
            }

            _context.BenhNhan.Remove(benhNhan);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa hồ sơ bệnh nhân '{benhNhan.tenBN}' thành công!";
            return RedirectToAction(nameof(Index));
        }

        private bool BenhNhanExists(int id)
        {
            return _context.BenhNhan.Any(e => e.maBN == id);
        }
    }
}
