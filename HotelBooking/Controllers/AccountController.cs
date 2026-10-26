using System.Security.Claims;
using HotelBooking.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Controllers;
public class AccountController(HotelDbContext db, IPasswordHasher<NguoiDung> hasher) : Controller
{
    int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public IActionResult Register() => View(new RegisterModel());
    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel m)
    {
        if (!ModelState.IsValid)
            return View(m);
        m.Email = m.Email.Trim().ToLowerInvariant();
        if (await db.NguoiDung.AnyAsync(x => x.Email == m.Email))
            ModelState.AddModelError("Email", "Email đã được sử dụng");
        if (!ModelState.IsValid)
            return View(m);
        var u = new NguoiDung
        {
            HoTen = m.HoTen.Trim(),
            Email = m.Email,
            SoDienThoai = m.SoDienThoai
        };
        u.MatKhauMaHoa = hasher.HashPassword(u, m.MatKhauMoi);
        db.Add(u);
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("Email", "Email đã được sử dụng");
            return View(m);
        }

        TempData["Success"] = "Đăng ký thành công. Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    public IActionResult Login(string? returnUrl) => View(new LoginModel { ReturnUrl = returnUrl });
    [HttpPost]
    public async Task<IActionResult> Login(LoginModel m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var u = await db.NguoiDung.SingleOrDefaultAsync(x => x.Email == m.Email.Trim().ToLowerInvariant());
        if (u is null || hasher.VerifyHashedPassword(u, u.MatKhauMaHoa, m.MatKhau) == PasswordVerificationResult.Failed)
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
        else if (!u.HoatDong)
            ModelState.AddModelError("", "Tài khoản đã bị khóa.");
        if (!ModelState.IsValid)
            return View(m);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, u!.MaNguoiDung.ToString()), new Claim(ClaimTypes.Name, u.HoTen), new Claim(ClaimTypes.Role, u.VaiTro), new Claim("Credential", u.MatKhauMaHoa) }, CookieAuthenticationDefaults.AuthenticationScheme)));
        if (Url.IsLocalUrl(m.ReturnUrl))
            return LocalRedirect(m.ReturnUrl!);
        return u.VaiTro == "Admin"
            ? RedirectToRoute("admin", new { controller = "Dashboard", action = "Index" })
            : RedirectToAction("Index", "Home");
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgotPassword() => View(new ForgotModel());
    [HttpPost]
    public async Task<IActionResult> ForgotPassword(ForgotModel m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var u = await db.NguoiDung.SingleOrDefaultAsync(x => x.Email == m.Email.Trim().ToLowerInvariant() && x.SoDienThoai == m.SoDienThoai && x.HoatDong);
        if (u is null)
        {
            ModelState.AddModelError("", "Email hoặc số điện thoại không đúng");
            return View(m);
        }

        u.TokenDatLaiMatKhau = Guid.NewGuid().ToString("N");
        u.HanTokenDatLai = HotelClock.Now.AddMinutes(15);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(ResetPassword), new { token = u.TokenDatLaiMatKhau });
    }

    public IActionResult ResetPassword(string token) => View(new ResetModel { Token = token });
    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetModel m)
    {
        if (!ModelState.IsValid)
            return View(m);
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var u = await db.NguoiDung.SingleOrDefaultAsync(x => x.TokenDatLaiMatKhau == m.Token && x.HanTokenDatLai > HotelClock.Now && x.HoatDong);
        if (u is null)
        {
            ModelState.AddModelError("", "Token sai hoặc hết hạn. Vui lòng xác minh lại.");
            return View(m);
        }

        u.MatKhauMaHoa = hasher.HashPassword(u, m.MatKhauMoi);
        u.TokenDatLaiMatKhau = null;
        u.HanTokenDatLai = null;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["Success"] = "Đã đặt lại mật khẩu.";
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var u = await db.NguoiDung.FindAsync(UserId);
        return View(new ProfileModel { Email = u!.Email, HoTen = u.HoTen, SoDienThoai = u.SoDienThoai });
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Profile(ProfileModel m)
    {
        var u = await db.NguoiDung.FindAsync(UserId);
        m.Email = u!.Email;
        ModelState.Remove(nameof(m.Email));
        if (!ModelState.IsValid)
            return View(m);
        u.HoTen = m.HoTen.Trim();
        u.SoDienThoai = m.SoDienThoai;
        await db.SaveChangesAsync();
        TempData["Success"] = "Đã cập nhật hồ sơ.";
        return RedirectToAction(nameof(Profile));
    }

    [Authorize]
    public IActionResult ChangePassword() => View(new PasswordModel());
    [Authorize, HttpPost]
    public async Task<IActionResult> ChangePassword(PasswordModel m)
    {
        if (!ModelState.IsValid)
            return View(m);
        var u = await db.NguoiDung.FindAsync(UserId);
        if (hasher.VerifyHashedPassword(u!, u!.MatKhauMaHoa, m.MatKhauCu) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "Mật khẩu cũ không đúng.");
            return View(m);
        }

        u.MatKhauMaHoa = hasher.HashPassword(u, m.MatKhauMoi);
        u.TokenDatLaiMatKhau = null;
        u.HanTokenDatLai = null;
        await db.SaveChangesAsync();
        await HttpContext.SignOutAsync();
        TempData["Success"] = "Đã đổi mật khẩu. Vui lòng đăng nhập lại.";
        return RedirectToAction(nameof(Login));
    }

    public IActionResult AccessDenied()
    {
        Response.StatusCode = 403;
        return View();
    }
}
