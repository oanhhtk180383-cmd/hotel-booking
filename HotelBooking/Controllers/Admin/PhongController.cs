using HotelBooking.Models;
using HotelBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers.Admin;
[Authorize(Roles = "Admin")]
public class PhongController(HotelDbContext db, HotelService service) : Controller
{
    async Task Types() => ViewBag.Types = new SelectList(await db.LoaiPhong.AsNoTracking().ToListAsync(), "MaLoaiPhong", "TenLoaiPhong");
    public async Task<IActionResult> Index(int page = 1) => View(await PagedList<Phong>.CreateAsync(db.Phong.Include(x => x.LoaiPhong).AsNoTracking().OrderBy(x => x.SoPhong).ThenBy(x => x.MaPhong), page, 10));
    public async Task<IActionResult> Create()
    {
        await Types();
        return View("Edit", new Phong());
    }

    public async Task<IActionResult> Edit(int id)
    {
        var m = await db.Phong.SingleOrDefaultAsync(x => x.MaPhong == id);
        if (m is null)
            return NotFound();
        await Types();
        return View(m);
    }

    [HttpPost]
    public Task<IActionResult> Create([Bind("MaLoaiPhong,SoPhong,TrangThai")] Phong m) => Save(0, m);
    [HttpPost]
    public Task<IActionResult> Edit(int id, [Bind("MaLoaiPhong,SoPhong,TrangThai")] Phong m) => Save(id, m);
    async Task<IActionResult> Save(int id, Phong m)
    {
        m.MaPhong = id;
        m.SoPhong = m.SoPhong.Trim();
        await Types();
        if (!ModelState.IsValid)
            return View("Edit", m);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        if (!await db.LoaiPhong.AnyAsync(x => x.MaLoaiPhong == m.MaLoaiPhong))
            ModelState.AddModelError("MaLoaiPhong", "Loại phòng không hợp lệ.");
        if (await db.Phong.AnyAsync(x => x.SoPhong == m.SoPhong && x.MaPhong != id))
            ModelState.AddModelError("SoPhong", "Số phòng đã tồn tại.");
        var entity = id == 0 ? new Phong() : await db.Phong.SingleOrDefaultAsync(x => x.MaPhong == id);
        if (entity is null)
            return NotFound();
        if (id != 0 && entity.MaLoaiPhong != m.MaLoaiPhong && await service.HasActive(db.Phong.Where(x => x.MaPhong == id)))
            ModelState.AddModelError("MaLoaiPhong", "Không thể đổi loại phòng khi còn đặt phòng đang hoạt động.");
        if (!ModelState.IsValid)
            return View("Edit", m);
        entity.MaLoaiPhong = m.MaLoaiPhong;
        entity.SoPhong = m.SoPhong;
        entity.TrangThai = m.TrangThai;
        if (id == 0)
            db.Add(entity);
        try
        {
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("SoPhong", "Không thể lưu phòng. Hãy kiểm tra số phòng và loại phòng.");
            return View("Edit", m);
        }

        TempData["Success"] = "Đã lưu phòng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            await service.DeleteRoom(id, false);
            TempData["Success"] = "Đã xóa mềm phòng.";
        }
        catch (InvalidOperationException e)
        {
            TempData["Error"] = e.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
