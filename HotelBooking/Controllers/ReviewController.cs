using System.Security.Claims;
using HotelBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers;
[Authorize]
public class ReviewController(HotelDbContext db) : Controller
{
    int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Create(int id)
    {
        var d = await db.DatPhong.Include(x => x.DanhGia).SingleOrDefaultAsync(x => x.MaDatPhong == id && x.MaNguoiDung == UserId && x.TrangThai == "HoanThanh");
        if (d is null || d.DanhGia is not null)
            return NotFound();
        return View(new DanhGia { MaDatPhong = id });
    }

    [HttpPost]
    public async Task<IActionResult> Create([Bind("MaDatPhong,SoSao,NoiDung")] DanhGia m)
    {
        ModelState.Remove(nameof(m.NguoiDung));
        if (!ModelState.IsValid)
            return View(m);
        var d = await db.DatPhong.IgnoreQueryFilters().Include(x => x.Phong).Include(x => x.DanhGia).SingleOrDefaultAsync(x => x.MaDatPhong == m.MaDatPhong && x.MaNguoiDung == UserId && x.TrangThai == "HoanThanh");
        if (d is null || d.DanhGia is not null)
        {
            TempData["Error"] = "Đặt phòng chưa hoàn thành hoặc đã được đánh giá.";
            return RedirectToAction("Index", "Booking");
        }

        m.MaNguoiDung = UserId;
        m.MaLoaiPhong = d.Phong.MaLoaiPhong;
        db.Add(m);
        try
        {
            await db.SaveChangesAsync();
            TempData["Success"] = "Đã gửi đánh giá.";
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Đặt phòng này đã được đánh giá.";
        }

        return RedirectToAction("Index", "Booking");
    }
}
