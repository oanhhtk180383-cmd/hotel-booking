using HotelBooking.Models;
using HotelBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers.Admin;
[Authorize(Roles = "Admin")]
public class DatPhongController(HotelDbContext db, HotelService service) : Controller
{
    public async Task<IActionResult> Index(string? trangThai, int page = 1)
    {
        ViewBag.Status = trangThai;
        var q = db.DatPhong.IgnoreQueryFilters().AsNoTracking().Include(x => x.NguoiDung).Include(x => x.Phong).ThenInclude(x => x.LoaiPhong).AsQueryable();
        if (!string.IsNullOrEmpty(trangThai))
            q = q.Where(x => x.TrangThai == trangThai);
        return View(await PagedList<DatPhong>.CreateAsync(q.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaDatPhong), page, 10));
    }

    [HttpPost]
    public Task<IActionResult> XacNhan(int id) => Change(id, "DaXacNhan");
    [HttpPost]
    public Task<IActionResult> HoanThanh(int id) => Change(id, "HoanThanh");
    [HttpPost]
    public Task<IActionResult> Huy(int id) => Change(id, "DaHuy");
    async Task<IActionResult> Change(int id, string status)
    {
        try
        {
            await service.ChangeStatus(id, status);
            TempData["Success"] = "Đã cập nhật trạng thái.";
        }
        catch (InvalidOperationException e)
        {
            TempData["Error"] = e.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
