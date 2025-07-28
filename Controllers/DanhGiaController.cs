using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CinemaManagement.Data;
using CinemaManagement.Models;
using CinemaManagement.ViewModels;
using System.Security.Claims;
using System.IO;

namespace CinemaManagement.Controllers
{
    public class DanhGiaController : Controller
    {
        private static readonly object _logLock = new object();
        private readonly CinemaDbContext _context;

        public DanhGiaController(CinemaDbContext context)
        {
            _context = context;
        }

        private void WriteErrorLog(string message)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "error_log.txt");
            var logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [DANHGIA] {message}\n";
            lock (_logLock)
            {
                System.IO.File.AppendAllText(logPath, logLine);
            }
        }

        // GET: DanhGia/Create/{maPhim}
        public async Task<IActionResult> Create(string maPhim)
        {
            WriteErrorLog($"[CREATE] Bắt đầu Create với maPhim: {maPhim}");
            WriteErrorLog($"[CREATE] Session keys: {string.Join(",", HttpContext.Session.Keys)}");
            
            if (string.IsNullOrEmpty(maPhim))
            {
                WriteErrorLog($"[CREATE] maPhim rỗng, return NotFound");
                return NotFound();
            }

            var phim = await _context.Phims
                .FirstOrDefaultAsync(p => p.MaPhim == maPhim);

            if (phim == null)
            {
                WriteErrorLog($"[CREATE] Không tìm thấy phim với maPhim: {maPhim}");
                return NotFound();
            }

            WriteErrorLog($"[CREATE] Tìm thấy phim: {phim.TenPhim}");

            // Lấy thông tin khách hàng từ session
            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            WriteErrorLog($"[CREATE] maKhachHang từ session: {maKhachHang}");
            
            if (string.IsNullOrEmpty(maKhachHang))
            {
                WriteErrorLog($"[CREATE] Không có maKhachHang trong session, redirect to Login");
                return RedirectToAction("Login", "Auth");
            }

            var khachHang = await _context.KhachHangs
                .FirstOrDefaultAsync(k => k.maKhachHang == maKhachHang);

            if (khachHang == null)
            {
                WriteErrorLog($"[CREATE] Không tìm thấy khách hàng với maKhachHang: {maKhachHang}");
                return NotFound();
            }

            WriteErrorLog($"[CREATE] Tìm thấy khách hàng: {khachHang.HoTen}");

            // Kiểm tra xem khách hàng đã đánh giá phim này chưa
            var existingDanhGia = await _context.DanhGias
                .FirstOrDefaultAsync(d => d.maKhachHang == maKhachHang && d.MaPhim == maPhim);

            if (existingDanhGia != null)
            {
                WriteErrorLog($"[CREATE] Đã có đánh giá trước đó, redirect to Edit với id: {existingDanhGia.Id}");
                // Nếu đã đánh giá, chuyển đến trang chỉnh sửa
                return RedirectToAction("Edit", new { id = existingDanhGia.Id });
            }

            WriteErrorLog($"[CREATE] Chưa có đánh giá, tạo viewModel mới");

            var viewModel = new DanhGiaViewModel
            {
                MaPhim = phim.MaPhim!,
                TenPhim = phim.TenPhim!,
                maKhachHang = khachHang.maKhachHang!,
                HoTenKhachHang = khachHang.HoTen!
            };

            WriteErrorLog($"[CREATE] Trả về View với viewModel: MaPhim={viewModel.MaPhim}, TenPhim={viewModel.TenPhim}, maKhachHang={viewModel.maKhachHang}");
            return View(viewModel);
        }

        // POST: DanhGia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhGiaViewModel viewModel)
        {
            WriteErrorLog($"[CREATE_POST] Bắt đầu POST Create với viewModel: MaPhim={viewModel.MaPhim}, maKhachHang={viewModel.maKhachHang}, SoSao={viewModel.SoSao}");
            WriteErrorLog($"[CREATE_POST] ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                WriteErrorLog($"[CREATE_POST] ModelState không hợp lệ, errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return View(viewModel);
            }

            try
            {
                var danhGia = new DanhGia
                {
                    maKhachHang = viewModel.maKhachHang,
                    MaPhim = viewModel.MaPhim,
                    SoSao = viewModel.SoSao,
                    NoiDungBinhLuan = viewModel.NoiDungBinhLuan,
                    NgayDanhGia = DateTime.Now,
                    DaXemPhim = true // Giả sử khách hàng đã xem phim khi đánh giá
                };

                WriteErrorLog($"[CREATE_POST] Tạo đánh giá mới: maKhachHang={danhGia.maKhachHang}, MaPhim={danhGia.MaPhim}, SoSao={danhGia.SoSao}");

                _context.Add(danhGia);
                await _context.SaveChangesAsync();

                WriteErrorLog($"[CREATE_POST] Lưu đánh giá thành công với Id: {danhGia.Id}");

                TempData["SuccessMessage"] = "Đánh giá của bạn đã được ghi nhận!";
                return RedirectToAction("ChiTietPhim", "KhachHang", new { maPhim = viewModel.MaPhim });
            }
            catch (Exception ex)
            {
                WriteErrorLog($"[CREATE_POST] Lỗi khi lưu đánh giá: {ex.Message}");
                WriteErrorLog($"[CREATE_POST] Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu đánh giá. Vui lòng thử lại.";
                return View(viewModel);
            }
        }

        // GET: DanhGia/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            WriteErrorLog($"[EDIT] Bắt đầu Edit với id: {id}");
            
            var danhGia = await _context.DanhGias
                .Include(d => d.KhachHang)
                .Include(d => d.Phim)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (danhGia == null)
            {
                WriteErrorLog($"[EDIT] Không tìm thấy đánh giá với id: {id}");
                return NotFound();
            }

            WriteErrorLog($"[EDIT] Tìm thấy đánh giá: maKhachHang={danhGia.maKhachHang}, MaPhim={danhGia.MaPhim}, SoSao={danhGia.SoSao}");

            // Kiểm tra quyền chỉnh sửa
            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            WriteErrorLog($"[EDIT] maKhachHang từ session: {maKhachHang}");
            
            if (string.IsNullOrEmpty(maKhachHang))
            {
                WriteErrorLog($"[EDIT] Không có maKhachHang trong session, redirect to Login");
                return RedirectToAction("Login", "Auth");
            }
            
            if (danhGia.maKhachHang != maKhachHang)
            {
                WriteErrorLog($"[EDIT] Không có quyền chỉnh sửa: danhGia.maKhachHang={danhGia.maKhachHang}, session.maKhachHang={maKhachHang}");
                return Forbid();
            }

            var viewModel = new DanhGiaViewModel
            {
                MaPhim = danhGia.MaPhim,
                TenPhim = danhGia.Phim.TenPhim!,
                maKhachHang = danhGia.maKhachHang,
                HoTenKhachHang = danhGia.KhachHang.HoTen!,
                SoSao = danhGia.SoSao,
                NoiDungBinhLuan = danhGia.NoiDungBinhLuan
            };

            WriteErrorLog($"[EDIT] Trả về View với viewModel: MaPhim={viewModel.MaPhim}, SoSao={viewModel.SoSao}");
            return View(viewModel);
        }

        // POST: DanhGia/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DanhGiaViewModel viewModel)
        {
            WriteErrorLog($"[EDIT_POST] Bắt đầu POST Edit với id: {id}, MaPhim: {viewModel.MaPhim}, SoSao: {viewModel.SoSao}");
            
            WriteErrorLog($"[EDIT_POST] ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                WriteErrorLog($"[EDIT_POST] ModelState không hợp lệ, errors: {string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage))}");
                return View(viewModel);
            }

            try
            {
                // Tìm đánh giá theo ID thực tế từ database
                var danhGia = await _context.DanhGias
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (danhGia == null)
                {
                    WriteErrorLog($"[EDIT_POST] Không tìm thấy đánh giá với id: {id}");
                    return NotFound();
                }

                // Kiểm tra quyền chỉnh sửa
                var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
                if (danhGia.maKhachHang != maKhachHang)
                {
                    WriteErrorLog($"[EDIT_POST] Không có quyền chỉnh sửa: danhGia.maKhachHang={danhGia.maKhachHang}, session.maKhachHang={maKhachHang}");
                    return Forbid();
                }

                WriteErrorLog($"[EDIT_POST] Tìm thấy đánh giá để cập nhật: Id={danhGia.Id}, SoSao cũ={danhGia.SoSao}");

                danhGia.SoSao = viewModel.SoSao;
                danhGia.NoiDungBinhLuan = viewModel.NoiDungBinhLuan;
                danhGia.NgayDanhGia = DateTime.Now;

                _context.Update(danhGia);
                await _context.SaveChangesAsync();

                WriteErrorLog($"[EDIT_POST] Cập nhật đánh giá thành công: SoSao mới={danhGia.SoSao}");

                TempData["SuccessMessage"] = "Đánh giá của bạn đã được cập nhật!";
                return RedirectToAction("ChiTietPhim", "KhachHang", new { maPhim = viewModel.MaPhim });
            }
            catch (Exception ex)
            {
                WriteErrorLog($"[EDIT_POST] Lỗi khi cập nhật đánh giá: {ex.Message}");
                WriteErrorLog($"[EDIT_POST] Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật đánh giá. Vui lòng thử lại.";
                return View(viewModel);
            }
        }

        // GET: DanhGia/List/{maPhim}
        public async Task<IActionResult> List(string maPhim)
        {
            WriteErrorLog($"[LIST] Bắt đầu List với maPhim: {maPhim}");
            
            if (string.IsNullOrEmpty(maPhim))
            {
                WriteErrorLog($"[LIST] maPhim rỗng, return NotFound");
                return NotFound();
            }

            var phim = await _context.Phims
                .Include(p => p.DanhGias)
                .ThenInclude(d => d.KhachHang)
                .FirstOrDefaultAsync(p => p.MaPhim == maPhim);

            if (phim == null)
            {
                WriteErrorLog($"[LIST] Không tìm thấy phim với maPhim: {maPhim}");
                return NotFound();
            }

            WriteErrorLog($"[LIST] Tìm thấy phim: {phim.TenPhim}, số đánh giá: {phim.DanhGias.Count}");

            var viewModel = new DanhGiaListViewModel
            {
                MaPhim = phim.MaPhim!,
                TenPhim = phim.TenPhim!,
                DiemTrungBinh = phim.DiemTrungBinh,
                TongSoDanhGia = phim.TongSoDanhGia,
                DanhGias = phim.DanhGias
                    .OrderByDescending(d => d.NgayDanhGia)
                    .Select(d => new DanhGiaItemViewModel
                    {
                        Id = d.Id,
                        SoSao = d.SoSao,
                        NoiDungBinhLuan = d.NoiDungBinhLuan,
                        NgayDanhGia = d.NgayDanhGia,
                        HoTenKhachHang = d.KhachHang.HoTen!,
                        DaXemPhim = d.DaXemPhim
                    })
                    .ToList()
            };

            WriteErrorLog($"[LIST] Trả về View với {viewModel.DanhGias.Count} đánh giá");
            return View(viewModel);
        }

        // POST: DanhGia/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            WriteErrorLog($"[DELETE] Bắt đầu Delete với id: {id}");
            
            var danhGia = await _context.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                WriteErrorLog($"[DELETE] Không tìm thấy đánh giá với id: {id}");
                return NotFound();
            }

            WriteErrorLog($"[DELETE] Tìm thấy đánh giá: maKhachHang={danhGia.maKhachHang}, MaPhim={danhGia.MaPhim}");

            // Kiểm tra quyền xóa
            var maKhachHang = HttpContext.Session.GetString("MaKhachHang");
            WriteErrorLog($"[DELETE] maKhachHang từ session: {maKhachHang}");
            
            if (danhGia.maKhachHang != maKhachHang)
            {
                WriteErrorLog($"[DELETE] Không có quyền xóa: danhGia.maKhachHang={danhGia.maKhachHang}, session.maKhachHang={maKhachHang}");
                return Forbid();
            }

            try
            {
                _context.DanhGias.Remove(danhGia);
                await _context.SaveChangesAsync();

                WriteErrorLog($"[DELETE] Xóa đánh giá thành công");
                TempData["SuccessMessage"] = "Đánh giá đã được xóa!";
                return RedirectToAction("ChiTietPhim", "KhachHang", new { maPhim = danhGia.MaPhim });
            }
            catch (Exception ex)
            {
                WriteErrorLog($"[DELETE] Lỗi khi xóa đánh giá: {ex.Message}");
                WriteErrorLog($"[DELETE] Stack trace: {ex.StackTrace}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi xóa đánh giá. Vui lòng thử lại.";
                return RedirectToAction("ChiTietPhim", "KhachHang", new { maPhim = danhGia.MaPhim });
            }
        }
    }
} 
