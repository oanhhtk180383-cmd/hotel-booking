using System.Security.Claims;
using HotelBooking.Models;
using HotelBooking.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
    options.ModelValidatorProviders.Insert(0, new VietnameseValidationProvider());
    options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "Vui lòng nhập giá trị hợp lệ.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((_, field) => $"Giá trị {field} không hợp lệ.");
    options.ModelBindingMessageProvider.SetValueIsInvalidAccessor(_ => "Giá trị không hợp lệ.");
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(field => $"{field} phải là số.");
    options.ModelBindingMessageProvider.SetNonPropertyAttemptedValueIsInvalidAccessor(_ => "Giá trị không hợp lệ.");
});
builder.Services.Configure<Microsoft.AspNetCore.Mvc.Razor.RazorViewEngineOptions>(options =>
{
    options.ViewLocationFormats.Insert(0, "/Views/Admin/{1}/{0}.cshtml");
});
builder.Services.AddDbContext<HotelDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("HotelDatabase")));
builder.Services.AddScoped<IPasswordHasher<NguoiDung>, PasswordHasher<NguoiDung>>();
builder.Services.AddScoped<HotelService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
{
    o.LoginPath = "/Account/Login";
    o.AccessDeniedPath = "/Account/AccessDenied";
    o.ExpireTimeSpan = TimeSpan.FromHours(8);
    o.Events.OnValidatePrincipal = async context =>
    {
        var db = context.HttpContext.RequestServices.GetRequiredService<HotelDbContext>();
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var user = int.TryParse(id, out var key) ? await db.NguoiDung.AsNoTracking().SingleOrDefaultAsync(x => x.MaNguoiDung == key) : null;
        if (user is null || !user.HoatDong || user.VaiTro != context.Principal?.FindFirstValue(ClaimTypes.Role) || user.MatKhauMaHoa != context.Principal?.FindFirstValue("Credential"))
            context.RejectPrincipal();
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();
if (args.Contains("--seed-initial-data"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    await InitialDataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<IPasswordHasher<NguoiDung>>(), builder.Configuration["InitialData:GuestPassword"] ?? "");
    Console.WriteLine("Đã chuẩn bị danh mục phòng, tài khoản khách hàng, lịch đặt phòng và đánh giá.");
    return;
}

if (args.Contains("--init-db"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
    var email = builder.Configuration["SeedAdmin:Email"];
    var password = builder.Configuration["SeedAdmin:Password"];
    if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || password.Length < 6)
        throw new InvalidOperationException("Cấu hình SeedAdmin:Email và SeedAdmin:Password (ít nhất 6 ký tự) trước khi khởi tạo.");
    email = email.Trim().ToLowerInvariant();
    await db.Database.EnsureCreatedAsync();
    if (!await db.NguoiDung.AnyAsync(x => x.Email == email))
    {
        var admin = new NguoiDung
        {
            HoTen = "Quản trị viên",
            Email = email,
            SoDienThoai = "0900000000",
            VaiTro = "Admin"
        };
        admin.MatKhauMaHoa = scope.ServiceProvider.GetRequiredService<IPasswordHasher<NguoiDung>>().HashPassword(admin, password);
        db.Add(admin);
        await db.SaveChangesAsync();
    }

    Console.WriteLine("Đã khởi tạo 5 bảng và tài khoản Admin.");
    return;
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    "admin",
    "Admin/{controller=Dashboard}/{action=Index}/{id?}",
    constraints: new { controller = "Dashboard|DatPhong|LoaiPhong|NguoiDung|Phong" });
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program
{
}
