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
    public class DangKyKhamController : BaseController
    {
        private readonly QuanLyPhongKhamDB _context;

        public DangKyKhamController(QuanLyPhongKhamDB context)
        {
            _context = context;
        }

        // 🛠️ HÀM HELPER ĐỔ DỮ LIỆU VÀO VIEWBAG (CÓ HỖ TRỢ TOGGLE showAllBN CHO ADMIN)
        private async Task LoadViewDataLichKham(int? selectedMaLK = null, int? selectedMaBN = null, bool showAllBN = false)
        {
            // 🛑 CHỈ NẠP CÁC LỊCH KHÁM MÀ BÁC SĨ CHƯA NGHỈ HẲN VÀ CHUYÊN KHOA ĐANG HOẠT ĐỘNG
            var dsLichKhamRaw = await _context.LichKham
                .Include(l => l.bs)
                    .ThenInclude(b => b.mCK)
                .Where(l => (l.currentSL < l.maxSL || l.maLK == selectedMaLK)
                         && l.bs != null
                         && l.bs.TrangThaiBS != BacSi.TrangThaiBacSi.DaNghiLam
                         && l.bs.mCK != null
                         && l.bs.mCK.TrangThaiCK == ChuyenKhoa.TrangThaiChuyenKhoa.DangHoatDong)
                .ToListAsync();

            var dsLichKham = dsLichKhamRaw.Select(l => new
            {
                l.maLK,
                ThongTinHienThi = $"[Mã {l.maLK}] BS. {(l.bs != null ? l.bs.tenBS : "Chưa phân công")} " +
                                  $"{(l.bs != null && l.bs.TrangThaiBS == BacSi.TrangThaiBacSi.TamNghi ? "(BS Tạm nghỉ - Đặt trước)" : "")} " +
                                  $"| {l.ngayKham:dd/MM/yyyy} " +
                                  $"({l.gioBatdau:HH\\:mm} - {l.gioKetthuc:HH\\:mm}) " +
                                  $"| Còn {l.maxSL - l.currentSL}/{l.maxSL} chỗ"
            }).ToList();

            ViewData["mlk"] = new SelectList(dsLichKham, "maLK", "ThongTinHienThi", selectedMaLK);

            // 🛑 LỌC BỆNH NHÂN: NẾU LÀ ADMIN VÀ MỞ CỜ showAllBN -> LOAD TOÀN BỘ. MẶC ĐỊNH CHỈ LẤY CỦA TÀI KHOẢN HIỆN TẠI
            var queryBenhNhan = _context.BenhNhan.AsQueryable();
            if (!IsAdmin || !showAllBN)
            {
                queryBenhNhan = queryBenhNhan.Where(b => b.mtk == CurrentAccId);
            }

            var dsBenhNhan = await queryBenhNhan.ToListAsync();
            ViewData["ooBN"] = new SelectList(dsBenhNhan, "maBN", "tenBN", selectedMaBN);
            ViewBag.ShowAllBN = IsAdmin && showAllBN;
        }

        // GET: DangKyKham hoặc DangKyKham?all=true
        public async Task<IActionResult> Index(bool all = false)
        {
            var query = _context.DangKyKham
                .Include(d => d.bn01)
                .Include(d => d.mLK)
                .ThenInclude(l => l!.bs)
                .AsQueryable();

            // 🛑 PHÂN QUYỀN GÓC NHÌN XEM DANH SÁCH:
            if (IsAdmin && all)
            {
                ViewBag.IsShowAll = true;
            }
            else
            {
                query = query.Where(d => d.bn01 != null && d.bn01.mtk == CurrentAccId);
                ViewBag.IsShowAll = false;
            }

            var dsDangKy = await query.OrderByDescending(d => d.ngayDK).ToListAsync();
            return View(dsDangKy);
        }

        // GET: DangKyKham/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dangKyKham = await _context.DangKyKham
                .Include(d => d.bn01)
                .Include(d => d.mLK)
                .ThenInclude(l => l!.bs)
                .FirstOrDefaultAsync(m => m.maPhieu == id);

            if (dangKyKham == null) return NotFound();

            if (!IsAdmin && dangKyKham.bn01 != null && dangKyKham.bn01.mtk != CurrentAccId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem thông tin phiếu đăng ký này!";
                return RedirectToAction(nameof(Index));
            }

            return View(dangKyKham);
        }

        // GET: DangKyKham/Create?maLK=5&showAllBN=true
        public async Task<IActionResult> Create(int? maLK, bool showAllBN = false)
        {
            if (!maLK.HasValue || maLK.Value <= 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn một Lịch khám cụ thể trước khi thực hiện đăng ký!";
                return RedirectToAction("Index", "LichKham");
            }

            var lichKham = await _context.LichKham
                .Include(l => l.bs)
                    .ThenInclude(b => b.mCK)
                .FirstOrDefaultAsync(l => l.maLK == maLK.Value);

            if (lichKham == null)
            {
                TempData["ErrorMessage"] = "Lịch khám bạn chọn không tồn tại trên hệ thống!";
                return RedirectToAction("Index", "LichKham");
            }

            // 🛑 CHẶN ĐẶT LỊCH KHI BÁC SĨ ĐÃ NGHỈ LÀM HOẶC KHOA TẠM DỪNG
            if (lichKham.bs != null && lichKham.bs.TrangThaiBS == BacSi.TrangThaiBacSi.DaNghiLam)
            {
                TempData["ErrorMessage"] = $"Không thể đăng ký: Bác sĩ '{lichKham.bs.tenBS}' đã nghỉ làm tại phòng khám!";
                return RedirectToAction("Index", "LichKham");
            }

            if (lichKham.bs != null && lichKham.bs.mCK != null && lichKham.bs.mCK.TrangThaiCK != ChuyenKhoa.TrangThaiChuyenKhoa.DangHoatDong)
            {
                TempData["ErrorMessage"] = $"Không thể đăng ký: Chuyên khoa '{lichKham.bs.mCK.tenCK}' hiện đang tạm dừng hoạt động!";
                return RedirectToAction("Index", "LichKham");
            }

            if (lichKham.TrangThaiLK != LichKham.TrangThaiLichKham.ConNhanDK || lichKham.currentSL >= lichKham.maxSL)
            {
                TempData["ErrorMessage"] = "Lịch khám này hiện đang tạm ngưng hoặc đã hết chỗ đăng ký!";
                return RedirectToAction("Index", "LichKham");
            }

            bool hasBenhNhan = IsAdmin && showAllBN
                ? await _context.BenhNhan.AnyAsync()
                : await _context.BenhNhan.AnyAsync(b => b.mtk == CurrentAccId);

            if (!hasBenhNhan)
            {
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ Bệnh nhân nào phù hợp! Vui lòng tạo hồ sơ Bệnh nhân trước.";
                return RedirectToAction("Create", "BenhNhan");
            }

            await LoadViewDataLichKham(maLK.Value, null, showAllBN);

            var model = new DangKyKham
            {
                mlk = maLK.Value,
                ngayDK = DateTime.Now
            };

            return View(model);
        }

        // POST: DangKyKham/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("maPhieu,ooBN,mlk,ngayDK,LiDo,TrangThaiDK")] DangKyKham dangKyKham, bool showAllBN = false)
        {
            if (!IsAdmin || !showAllBN)
            {
                bool isMyBenhNhan = await _context.BenhNhan.AnyAsync(b => b.maBN == dangKyKham.ooBN && b.mtk == CurrentAccId);
                if (!isMyBenhNhan)
                {
                    ModelState.AddModelError("ooBN", "Bệnh nhân được chọn không thuộc tài khoản của bạn!");
                }
            }

            if (ModelState.IsValid)
            {
                var lichKhamTarget = await _context.LichKham
                    .Include(l => l.bs)
                        .ThenInclude(b => b.mCK)
                    .FirstOrDefaultAsync(l => l.maLK == dangKyKham.mlk);

                if (lichKhamTarget == null)
                {
                    ModelState.AddModelError("mlk", "Lịch khám không tồn tại trên hệ thống!");
                }
                else if (lichKhamTarget.bs != null && lichKhamTarget.bs.TrangThaiBS == BacSi.TrangThaiBacSi.DaNghiLam)
                {
                    ModelState.AddModelError("mlk", $"Bác sĩ '{lichKhamTarget.bs.tenBS}' đã nghỉ làm tại phòng khám, không thể đăng ký!");
                }
                else if (lichKhamTarget.bs != null && lichKhamTarget.bs.mCK != null && lichKhamTarget.bs.mCK.TrangThaiCK != ChuyenKhoa.TrangThaiChuyenKhoa.DangHoatDong)
                {
                    ModelState.AddModelError("mlk", $"Chuyên khoa '{lichKhamTarget.bs.mCK.tenCK}' hiện đang tạm dừng hoạt động, không thể đăng ký!");
                }
                else if (lichKhamTarget.TrangThaiLK != LichKham.TrangThaiLichKham.ConNhanDK)
                {
                    ModelState.AddModelError("mlk", "Ca lịch khám này hiện đang tạm ngưng hoặc không tiếp nhận đăng ký mới!");
                }
                else if (lichKhamTarget.currentSL >= lichKhamTarget.maxSL)
                {
                    ModelState.AddModelError("mlk", "Lịch khám này vừa mới hết chỗ, vui lòng chọn ca khác!");
                }
                else
                {
                    bool daCoLichTrongNgay = await (from dk in _context.DangKyKham
                                                    join lk in _context.LichKham on dk.mlk equals lk.maLK
                                                    where dk.ooBN == dangKyKham.ooBN
                                                       && lk.ngayKham.Date == lichKhamTarget.ngayKham.Date
                                                       && dk.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaHuy
                                                    select dk).AnyAsync();

                    if (daCoLichTrongNgay)
                    {
                        ModelState.AddModelError("", $"CẢNH BÁO: Bệnh nhân này đã có lịch khám trong ngày {lichKhamTarget.ngayKham:dd/MM/yyyy}!");
                        ModelState.AddModelError("mlk", "Trùng lịch khám trong ngày!");
                    }
                    else
                    {
                        lichKhamTarget.currentSL += 1;

                        // 🛑 TỰ ĐỘNG CHUYỂN TRẠNG THÁI LỊCH KHÁM SANG ĐÃ ĐỦ NẾU ĐẠT TỐI ĐA SỐ LƯỢNG
                        if (lichKhamTarget.currentSL >= lichKhamTarget.maxSL)
                        {
                            lichKhamTarget.TrangThaiLK = LichKham.TrangThaiLichKham.DaDu;
                        }

                        _context.Update(lichKhamTarget);
                        _context.Add(dangKyKham);

                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Đăng ký khám thành công!";
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            await LoadViewDataLichKham(dangKyKham.mlk, dangKyKham.ooBN, showAllBN);
            return View(dangKyKham);
        }

        // 🛑 POST: DangKyKham/HuyDon/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HuyDon(int id)
        {
            var dangKyKham = await _context.DangKyKham
                .Include(d => d.bn01)
                .FirstOrDefaultAsync(d => d.maPhieu == id);

            if (dangKyKham == null)
            {
                TempData["ErrorMessage"] = "Phiếu đăng ký khám không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            if (!IsAdmin && (dangKyKham.bn01 == null || dangKyKham.bn01.mtk != CurrentAccId))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền hủy phiếu đăng ký này!";
                return RedirectToAction(nameof(Index));
            }

            if (dangKyKham.TrangThaiDK == DangKyKham.TrangThaiDangKy.DaKham)
            {
                TempData["ErrorMessage"] = "Không thể hủy ca khám đã hoàn tất!";
                return RedirectToAction(nameof(Index));
            }

            if (dangKyKham.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaHuy)
            {
                dangKyKham.TrangThaiDK = DangKyKham.TrangThaiDangKy.DaHuy;

                var lichKham = await _context.LichKham.FindAsync(dangKyKham.mlk);
                if (lichKham != null)
                {
                    if (lichKham.currentSL > 0)
                    {
                        lichKham.currentSL -= 1;
                    }

                    // 🛑 NẾU LỊCH ĐANG ĐÃ ĐỦ MÀ CÓ NGƯỜI HỦY -> MỞ LẠI THÀNH CÒN NHẬN ĐĂNG KÝ
                    if (lichKham.TrangThaiLK == LichKham.TrangThaiLichKham.DaDu && lichKham.currentSL < lichKham.maxSL)
                    {
                        lichKham.TrangThaiLK = LichKham.TrangThaiLichKham.ConNhanDK;
                    }

                    _context.Update(lichKham);
                }

                _context.Update(dangKyKham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã hủy phiếu đăng ký khám #{id} thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: DangKyKham/Edit/5?showAllBN=true
        public async Task<IActionResult> Edit(int? id, bool showAllBN = false)
        {
            if (id == null) return NotFound();

            var dangKyKham = await _context.DangKyKham
                .Include(d => d.bn01)
                .FirstOrDefaultAsync(d => d.maPhieu == id);

            if (dangKyKham == null) return NotFound();

            if (!IsAdmin)
            {
                bool isMyBenhNhan = await _context.BenhNhan.AnyAsync(b => b.maBN == dangKyKham.ooBN && b.mtk == CurrentAccId);
                if (!isMyBenhNhan)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa phiếu đăng ký này!";
                    return RedirectToAction(nameof(Index));
                }

                if (dangKyKham.TrangThaiDK != DangKyKham.TrangThaiDangKy.ChoXacNhan)
                {
                    TempData["ErrorMessage"] = "Phiếu đăng ký đã qua bước Chờ Xác Nhận. Bạn không thể tự chỉnh sửa, vui lòng liên hệ phòng khám!";
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                if (dangKyKham.bn01 != null && dangKyKham.bn01.mtk != CurrentAccId)
                {
                    showAllBN = true;
                }
            }

            await LoadViewDataLichKham(dangKyKham.mlk, dangKyKham.ooBN, showAllBN);
            return View(dangKyKham);
        }

        // POST: DangKyKham/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("maPhieu,ooBN,mlk,ngayDK,LiDo,TrangThaiDK")] DangKyKham dangKyKham, bool showAllBN = false)
        {
            if (id != dangKyKham.maPhieu) return NotFound();

            var oldDangKy = await _context.DangKyKham.AsNoTracking().FirstOrDefaultAsync(d => d.maPhieu == id);
            if (oldDangKy == null) return NotFound();

            if (!IsAdmin)
            {
                bool isMyBenhNhan = await _context.BenhNhan.AnyAsync(b => b.maBN == dangKyKham.ooBN && b.mtk == CurrentAccId);
                if (!isMyBenhNhan || oldDangKy.TrangThaiDK != DangKyKham.TrangThaiDangKy.ChoXacNhan)
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa thông tin phiếu đăng ký này!";
                    return RedirectToAction(nameof(Index));
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (oldDangKy.TrangThaiDK != dangKyKham.TrangThaiDK)
                    {
                        var lichKham = await _context.LichKham.FindAsync(dangKyKham.mlk);
                        if (lichKham != null)
                        {
                            if (dangKyKham.TrangThaiDK == DangKyKham.TrangThaiDangKy.DaHuy && oldDangKy.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaHuy)
                            {
                                if (lichKham.currentSL > 0) lichKham.currentSL -= 1;
                                if (lichKham.TrangThaiLK == LichKham.TrangThaiLichKham.DaDu && lichKham.currentSL < lichKham.maxSL)
                                {
                                    lichKham.TrangThaiLK = LichKham.TrangThaiLichKham.ConNhanDK;
                                }
                            }
                            else if (oldDangKy.TrangThaiDK == DangKyKham.TrangThaiDangKy.DaHuy && dangKyKham.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaHuy)
                            {
                                lichKham.currentSL += 1;
                                if (lichKham.currentSL >= lichKham.maxSL)
                                {
                                    lichKham.TrangThaiLK = LichKham.TrangThaiLichKham.DaDu;
                                }
                            }

                            _context.Update(lichKham);
                        }
                    }

                    _context.Update(dangKyKham);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Cập nhật thành công phiếu đăng ký #{id}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DangKyKhamExists(dangKyKham.maPhieu)) return NotFound();
                    else throw;
                }
            }

            await LoadViewDataLichKham(dangKyKham.mlk, dangKyKham.ooBN, showAllBN);
            return View(dangKyKham);
        }

        // POST: DangKyKham/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dangKyKham = await _context.DangKyKham
                .Include(d => d.bn01)
                .FirstOrDefaultAsync(d => d.maPhieu == id);

            if (dangKyKham != null)
            {
                if (!IsAdmin && (dangKyKham.bn01 == null || dangKyKham.bn01.mtk != CurrentAccId))
                {
                    TempData["ErrorMessage"] = "Bạn không có quyền xóa phiếu đăng ký này!";
                    return RedirectToAction(nameof(Index));
                }

                if (dangKyKham.TrangThaiDK != DangKyKham.TrangThaiDangKy.DaHuy)
                {
                    var lichKham = await _context.LichKham.FindAsync(dangKyKham.mlk);
                    if (lichKham != null)
                    {
                        if (lichKham.currentSL > 0) lichKham.currentSL -= 1;
                        if (lichKham.TrangThaiLK == LichKham.TrangThaiLichKham.DaDu && lichKham.currentSL < lichKham.maxSL)
                        {
                            lichKham.TrangThaiLK = LichKham.TrangThaiLichKham.ConNhanDK;
                        }
                        _context.Update(lichKham);
                    }
                }

                _context.DangKyKham.Remove(dangKyKham);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa phiếu đăng ký khám thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DangKyKhamExists(int id)
        {
            return _context.DangKyKham.Any(e => e.maPhieu == id);
        }
    }
}