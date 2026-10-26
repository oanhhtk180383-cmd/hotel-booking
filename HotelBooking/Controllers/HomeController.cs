using HotelBooking.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers;
public class HomeController(HotelDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.LoaiPhong.AsNoTracking().OrderBy(x => x.GiaMoiDem).ToListAsync());
    public IActionResult Error() => View();
}
