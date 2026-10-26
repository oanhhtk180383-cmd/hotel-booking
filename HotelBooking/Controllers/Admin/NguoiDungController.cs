using System.Security.Claims;
using HotelBooking.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers.Admin;
[Authorize(Roles = "Admin")]
public class NguoiDungController(HotelDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? email, int page = 1)
    {
        ViewBag.Email = email;
        var q = db.NguoiDung.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(email))
            q = q.Where(x => x.Email.Contains(email));
        return View(await PagedList<NguoiDung>.CreateAsync(q.OrderByDescending(x => x.NgayTao).ThenByDescending(x => x.MaNguoiDung), page, 10));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        if (id == int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!))
        {
            TempData["Error"] = "Không được tự khóa chính mình.";
            return RedirectToAction(nameof(Index));
        }

        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var u = await db.NguoiDung.FindAsync(id);
        if (u is null)
            return NotFound();
        u.HoatDong = !u.HoatDong;
        u.TokenDatLaiMatKhau = null;
        u.HanTokenDatLai = null;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["Success"] = "Đã cập nhật tài khoản.";
        return RedirectToAction(nameof(Index));
    }
}
