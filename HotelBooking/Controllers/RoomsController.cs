using HotelBooking.Models;
using HotelBooking.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers;
public class RoomsController(HotelDbContext db, HotelService service) : Controller
{
    public async Task<IActionResult> Index(int page = 1) => View(await PagedList<LoaiPhong>.CreateAsync(db.LoaiPhong.AsNoTracking().OrderBy(x => x.GiaMoiDem).ThenBy(x => x.MaLoaiPhong), page, 6));
    public async Task<IActionResult> Search(SearchModel m, int page = 1)
    {
        ViewBag.Search = m;
        ModelState.Remove(nameof(page));
        if (ModelState.IsValid)
        {
            if (m.NgayNhan.Date < HotelClock.Today)
                ModelState.AddModelError(nameof(m.NgayNhan), "Ngày nhận phòng phải từ hôm nay.");
            if (m.NgayTra.Date <= m.NgayNhan.Date)
                ModelState.AddModelError(nameof(m.NgayTra), "Ngày trả phòng phải sau ngày nhận phòng.");
        }

        if (!ModelState.IsValid)
            return View(PagedList<RoomResult>.Empty(6));
        var available = service.Available(m);
        var query = db.LoaiPhong.AsNoTracking()
            .Where(t => available.Any(p => p.MaLoaiPhong == t.MaLoaiPhong))
            .OrderBy(t => t.GiaMoiDem).ThenBy(t => t.MaLoaiPhong)
            .Select(t => new RoomResult(t, available.Count(p => p.MaLoaiPhong == t.MaLoaiPhong)));
        return View(await PagedList<RoomResult>.CreateAsync(query, page, 6));
    }

    public async Task<IActionResult> Details(int id, SearchModel search)
    {
        var type = await db.LoaiPhong.AsNoTracking().SingleOrDefaultAsync(x => x.MaLoaiPhong == id);
        if (type is null)
            return NotFound();
        ViewBag.Search = search;
        return View(new RoomDetail(type, await db.DanhGia.AsNoTracking().Include(x => x.NguoiDung).Where(x => x.MaLoaiPhong == id).OrderByDescending(x => x.NgayTao).ToListAsync()));
    }
}
