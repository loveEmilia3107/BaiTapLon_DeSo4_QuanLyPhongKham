using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyPhongKham.Models;

namespace QuanLyPhongKham.Controllers
{
    public class TaiKhoanController : BaseController
    {
        private readonly QuanLyPhongKhamDB _context;

        public TaiKhoanController(QuanLyPhongKhamDB context)
        {
            _context = context;
        }

        // GET: TaiKhoan hoặc TaiKhoan?all=true
        public async Task<IActionResult> Index(bool all = false)
        {
            if (CurrentAccId <= 0)
            {
                return RedirectToAction(nameof(Login));
            }

            var query = _context.TaiKhoan.AsQueryable();

            // 🛑 PHÂN QUYỀN GÓC NHÌN:
            // Admin bật all=true -> Load toàn bộ tài khoản
            if (IsAdmin && all)
            {
                ViewBag.IsShowAll = true;
            }
            else
            {
                // Mặc định (Bệnh nhân thường hoặc Admin xem cá nhân) -> Chỉ lấy tài khoản hiện tại
                query = query.Where(t => t.maTK == CurrentAccId);
                ViewBag.IsShowAll = false;
            }

            var dsTaiKhoan = await query.ToListAsync();
            return View(dsTaiKhoan);
        }

        // GET: TaiKhoan/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoan.FirstOrDefaultAsync(m => m.maTK == id);
            if (taiKhoan == null) return NotFound();

            // Bẫy phân quyền: chỉ Admin hoặc chủ tài khoản mới được xem
            if (!IsAdmin && taiKhoan.maTK != CurrentAccId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem thông tin tài khoản này!";
                return RedirectToAction(nameof(Index));
            }

            return View(taiKhoan);
        }

        // GET: TaiKhoan/Create
        public IActionResult Create()
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền tạo tài khoản trực tiếp!";
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        // POST: TaiKhoan/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("username,password,sdt,email,role,TrangThaiTK")] TaiKhoan taiKhoan)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền tạo tài khoản!";
                return RedirectToAction(nameof(Index));
            }

            if (await _context.TaiKhoan.AnyAsync(t => t.username == taiKhoan.username))
            {
                ModelState.AddModelError("username", "Tên đăng nhập này đã tồn tại!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Tạo tài khoản '{taiKhoan.username}' thành công!";
                return RedirectToAction(nameof(Index), new { all = true });
            }

            return View(taiKhoan);
        }

        // GET: TaiKhoan/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan == null) return NotFound();

            if (!IsAdmin && taiKhoan.maTK != CurrentAccId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài khoản này!";
                return RedirectToAction(nameof(Index));
            }

            return View(taiKhoan);
        }

        // POST: TaiKhoan/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("maTK,username,password,sdt,email,role,TrangThaiTK")] TaiKhoan taiKhoan)
        {
            if (id != taiKhoan.maTK) return NotFound();

            var oldAcc = await _context.TaiKhoan.AsNoTracking().FirstOrDefaultAsync(t => t.maTK == id);
            if (oldAcc == null) return NotFound();

            if (!IsAdmin && oldAcc.maTK != CurrentAccId)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chỉnh sửa tài khoản này!";
                return RedirectToAction(nameof(Index));
            }

            // Bệnh nhân thường không được tự sửa Role hay Trạng thái tài khoản của mình
            if (!IsAdmin)
            {
                taiKhoan.role = oldAcc.role;
                taiKhoan.TrangThaiTK = oldAcc.TrangThaiTK;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(taiKhoan);
                    await _context.SaveChangesAsync();

                    // Cập nhật lại Session Username nếu tự đổi tên tài khoản cá nhân
                    if (id == CurrentAccId)
                    {
                        HttpContext.Session.SetString("Username", taiKhoan.username);
                    }

                    TempData["SuccessMessage"] = "Cập nhật thông tin tài khoản thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TaiKhoanExists(taiKhoan.maTK)) return NotFound();
                    else throw;
                }
            }

            return View(taiKhoan);
        }

        // POST: TaiKhoan/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!IsAdmin)
            {
                TempData["ErrorMessage"] = "Chỉ Admin mới có quyền xóa tài khoản!";
                return RedirectToAction(nameof(Index));
            }

            if (id == CurrentAccId)
            {
                TempData["ErrorMessage"] = "Không thể tự xóa tài khoản Admin đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            var taiKhoan = await _context.TaiKhoan.FindAsync(id);
            if (taiKhoan != null)
            {
                // Kiểm tra ràng buộc bệnh nhân/đơn hàng trước khi xóa
                bool hasBenhNhan = await _context.BenhNhan.AnyAsync(b => b.mtk == id);
                if (hasBenhNhan)
                {
                    TempData["ErrorMessage"] = $"KHÔNG THỂ XÓA: Tài khoản '{taiKhoan.username}' đang liên kết với dữ liệu Hồ sơ Bệnh nhân!";
                    return RedirectToAction(nameof(Index), new { all = true });
                }

                _context.TaiKhoan.Remove(taiKhoan);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa tài khoản '{taiKhoan.username}' thành công!";
            }

            return RedirectToAction(nameof(Index), new { all = true });
        }

        private bool TaiKhoanExists(int id)
        {
            return _context.TaiKhoan.Any(e => e.maTK == id);
        }

        // GET: TaiKhoan/Register
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        // POST: TaiKhoan/Register
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register([Bind("username,password,sdt,email")] TaiKhoan taiKhoan)
        {
            taiKhoan.role = 1;
            taiKhoan.TrangThaiTK = TaiKhoan.TrangThaiTaiKhoan.DangHoatDong;

            ModelState.Remove("role");
            ModelState.Remove("TrangThaiTK");

            if (await _context.TaiKhoan.AnyAsync(t => t.username == taiKhoan.username))
            {
                ModelState.AddModelError("username", "Tên đăng nhập này đã được sử dụng!");
            }

            if (ModelState.IsValid)
            {
                _context.Add(taiKhoan);
                await _context.SaveChangesAsync();

                HttpContext.Session.SetInt32("AccID", taiKhoan.maTK);
                HttpContext.Session.SetString("Username", taiKhoan.username);
                HttpContext.Session.SetInt32("Role", taiKhoan.role);

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công!";
                return RedirectToAction("Index", "Home");
            }

            return View(taiKhoan);
        }

        // GET: TaiKhoan/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        // POST: TaiKhoan/Login
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError("", "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu!");
                return View();
            }

            var acc = await _context.TaiKhoan
                .FirstOrDefaultAsync(t => t.username == username && t.password == password);

            if (acc == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác!");
                return View();
            }

            // 🛑 BẪY KIỂM TRA ENUM TRẠNG THÁI TÀI KHOẢN (BỊ KHÓA)
            if (acc.TrangThaiTK == TaiKhoan.TrangThaiTaiKhoan.BiKhoa)
            {
                ViewBag.LockErrorMessage = $"Tài khoản '{acc.username}' đã bị khóa do vi phạm điều khoản dịch vụ & chính sách an toàn của hệ thống MedCare. Vui lòng liên hệ Admin để biết thêm chi tiết!";
                return View();
            }

            // ĐĂNG NHẬP THÀNH CÔNG -> LƯU SESSION
            HttpContext.Session.SetInt32("AccID", acc.maTK);
            HttpContext.Session.SetString("Username", acc.username);
            HttpContext.Session.SetInt32("Role", acc.role);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        // POST: TaiKhoan/DeleteSelf
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSelf()
        {
            int currentId = CurrentAccId;
            if (currentId <= 0)
            {
                return RedirectToAction(nameof(Login));
            }

            // 🛑 1. Kiểm tra ràng buộc Hồ sơ Bệnh nhân
            bool hasBenhNhan = await _context.BenhNhan.AnyAsync(b => b.mtk == currentId);
            if (hasBenhNhan)
            {
                TempData["ErrorMessage"] = "KHÔNG THỂ XÓA TÀI KHOẢN: Tài khoản của bạn hiện vẫn còn chứa dữ liệu Hồ sơ Bệnh nhân! Vui lòng xóa các hồ sơ bệnh nhân trước.";
                return RedirectToAction(nameof(Index));
            }

            var taiKhoan = await _context.TaiKhoan.FindAsync(currentId);
            if (taiKhoan != null)
            {
                _context.TaiKhoan.Remove(taiKhoan);
                await _context.SaveChangesAsync();

                // 🛑 2. Xóa sạch Session và đá về trang Đăng nhập
                HttpContext.Session.Clear();
                TempData["SuccessMessage"] = "Đã xóa tài khoản cá nhân thành công!";
                return RedirectToAction(nameof(Login));
            }

            return RedirectToAction(nameof(Index));
        }
        // GET: TaiKhoan/DoiMatKhau
        public IActionResult DoiMatKhau()
        {
            if (CurrentAccId <= 0)
            {
                return RedirectToAction(nameof(Login));
            }
            return View();
        }

        // POST: TaiKhoan/DoiMatKhau
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (CurrentAccId <= 0)
            {
                return RedirectToAction(nameof(Login));
            }

            if (ModelState.IsValid)
            {
                var taiKhoan = await _context.TaiKhoan.FindAsync(CurrentAccId);
                if (taiKhoan == null)
                {
                    return RedirectToAction(nameof(Login));
                }

                // 🛑 BẮT BẮT BẮT: Kiểm tra mật khẩu cũ có đúng với DB không
                if (taiKhoan.password != model.MatKhauCu)
                {
                    ModelState.AddModelError("MatKhauCu", "Mật khẩu hiện tại không chính xác!");
                    return View(model);
                }

                // Cập nhật mật khẩu mới
                taiKhoan.password = model.MatKhauMoi;
                _context.Update(taiKhoan);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }
    }
}