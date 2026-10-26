using HotelBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers.Admin;
[Authorize(Roles = "Admin")]
public class DashboardController(HotelDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var counts = await db.DatPhong.GroupBy(x => x.TrangThai).Select(g => new { Status = g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Status, x => x.Count);
        var revenue = await db.DatPhong.Where(x => x.TrangThai == "HoanThanh").GroupBy(x => new { x.NgayTra.Year, x.NgayTra.Month }).OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month).Select(g => new Revenue(g.Key.Year, g.Key.Month, g.Sum(x => x.TongTien))).ToListAsync();
        return View(new DashboardModel(counts, revenue));
    }
}
