using HotelBooking.Models;
using HotelBooking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers.Admin;
[Authorize(Roles = "Admin")]
public class LoaiPhongController(HotelDbContext db, HotelService service) : Controller
{
    public async Task<IActionResult> Index(int page = 1) => View(await PagedList<LoaiPhong>.CreateAsync(db.LoaiPhong.AsNoTracking().OrderBy(x => x.TenLoaiPhong).ThenBy(x => x.MaLoaiPhong), page, 10));
    public IActionResult Create() => View("Edit", new LoaiPhong());
    public async Task<IActionResult> Edit(int id)
    {
        var m = await db.LoaiPhong.FindAsync(id);
        return m is null || m.DaXoa ? NotFound() : View(m);
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind("TenLoaiPhong,MoTa,GiaMoiDem,SoKhachToiDa,TienNghi,HinhAnh")] LoaiPhong m)
    {
        ValidateImage(m.HinhAnh);
        if (!ModelState.IsValid)
            return View("Edit", m);
        db.Add(m);
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã thêm loại phòng.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("TenLoaiPhong,MoTa,GiaMoiDem,SoKhachToiDa,TienNghi,HinhAnh")] LoaiPhong m)
    {
        m.MaLoaiPhong = id;
        ValidateImage(m.HinhAnh);
        if (!ModelState.IsValid)
            return View(m);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var entity = await db.LoaiPhong.SingleOrDefaultAsync(x => x.MaLoaiPhong == id);
        if (entity is null)
            return NotFound();
        entity.TenLoaiPhong = m.TenLoaiPhong;
        entity.MoTa = m.MoTa;
        entity.GiaMoiDem = m.GiaMoiDem;
        entity.SoKhachToiDa = m.SoKhachToiDa;
        entity.TienNghi = m.TienNghi;
        entity.HinhAnh = m.HinhAnh;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["Success"] = "Đã cập nhật loại phòng.";
        return RedirectToAction(nameof(Index));
    }

    void ValidateImage(string? path)
    {
        if (!string.IsNullOrEmpty(path) && !(path.StartsWith('/') && !path.StartsWith("//")) && !(Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.Scheme == "https"))
            ModelState.AddModelError("HinhAnh", "Ảnh phải là đường dẫn /images/... hoặc URL HTTPS.");
    }

    [HttpPost]
    public async Task<IActionResult> Xoa(int id)
    {
        try
        {
            await service.DeleteRoom(id, true);
            TempData["Success"] = "Đã xóa mềm loại phòng và các phòng thuộc loại đó.";
        }
        catch (InvalidOperationException e)
        {
            TempData["Error"] = e.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
