using System.Security.Claims;
using HotelBooking.Models;
using HotelBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers;
[Authorize]
public class BookingController(HotelDbContext db, HotelService service) : Controller
{
    int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    IQueryable<DatPhong> Own() => db.DatPhong.IgnoreQueryFilters().AsNoTracking().Include(x => x.Phong).ThenInclude(x => x.LoaiPhong).Include(x => x.DanhGia).Where(x => x.MaNguoiDung == UserId);
    public async Task<IActionResult> Create(BookingModel m)
    {
        if (!m.Valid || !ModelState.IsValid)
        {
            TempData["Error"] = "Ngày hoặc số khách không hợp lệ.";
            return RedirectToAction("Search", "Rooms");
        }

        var room = await service.Available(m, m.MaLoaiPhong).Include(x => x.LoaiPhong).OrderBy(x => x.SoPhong).FirstOrDefaultAsync();
        if (room is null)
        {
            TempData["Error"] = "Không có phòng trống, hãy chọn ngày khác";
            return RedirectToAction("Search", "Rooms", new { m.NgayNhan, m.NgayTra, m.SoKhach });
        }

        ViewBag.Type = room.LoaiPhong;
        return View(m);
    }

    [HttpPost, ActionName("Create")]
    public async Task<IActionResult> CreatePost(BookingModel m)
    {
        if (!ModelState.IsValid || !m.Valid)
        {
            TempData["Error"] = "Dữ liệu đặt phòng không hợp lệ.";
            return RedirectToAction(nameof(Create), new { m.MaLoaiPhong, m.NgayNhan, m.NgayTra, m.SoKhach });
        }

        try
        {
            var id = await service.Create(UserId, m);
            return RedirectToAction(nameof(Success), new { id });
        }
        catch (InvalidOperationException e)
        {
            TempData["Error"] = e.Message;
        }
        catch (Exception e)when (e is DbUpdateException || e is Microsoft.Data.SqlClient.SqlException)
        {
            TempData["Error"] = "Có người đang đặt phòng cùng lúc. Vui lòng tìm và thử lại.";
        }

        return RedirectToAction("Search", "Rooms", new { m.NgayNhan, m.NgayTra, m.SoKhach });
    }

    public async Task<IActionResult> Index(int page = 1) => View(await PagedList<DatPhong>.CreateAsync(Own().OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaDatPhong), page, 5));
    public async Task<IActionResult> Details(int id)
    {
        var d = await Own().SingleOrDefaultAsync(x => x.MaDatPhong == id);
        return d is null ? NotFound() : View(d);
    }

    public async Task<IActionResult> Success(int id)
    {
        var d = await Own().SingleOrDefaultAsync(x => x.MaDatPhong == id);
        return d is null ? NotFound() : View(d);
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await service.ChangeStatus(id, "DaHuy", UserId);
            TempData["Success"] = "Đã hủy đặt phòng.";
        }
        catch (InvalidOperationException e)
        {
            TempData["Error"] = e.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
