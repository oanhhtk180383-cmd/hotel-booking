using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models;
public class LoginModel
{
    [Required(ErrorMessage = "Vui lòng nhập {0}."), EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string MatKhau { get; set; } = "";
    public string? ReturnUrl { get; set; }
}

public class RegisterModel
{
    [Required(ErrorMessage = "Vui lòng nhập {0}."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(150, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), MinLength(6, ErrorMessage = "Mật khẩu cần ít nhất 6 ký tự."), DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu xác nhận không khớp."), DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    public string XacNhan { get; set; } = "";
}

public class ForgotModel
{
    [Required(ErrorMessage = "Vui lòng nhập {0}."), EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";
}

public class ResetModel
{
    [Required(ErrorMessage = "Vui lòng nhập {0}.")]
    public string Token { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), MinLength(6, ErrorMessage = "Mật khẩu cần ít nhất 6 ký tự."), DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu xác nhận không khớp."), DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    public string XacNhan { get; set; } = "";
}

public class ProfileModel
{
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";
}

public class PasswordModel
{
    [Required(ErrorMessage = "Vui lòng nhập {0}."), DataType(DataType.Password)]
    [Display(Name = "Mật khẩu cũ")]
    public string MatKhauCu { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), MinLength(6, ErrorMessage = "Mật khẩu cần ít nhất 6 ký tự."), DataType(DataType.Password)]
    [Display(Name = "Mật khẩu mới")]
    public string MatKhauMoi { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), Compare(nameof(MatKhauMoi), ErrorMessage = "Mật khẩu xác nhận không khớp."), DataType(DataType.Password)]
    [Display(Name = "Xác nhận mật khẩu")]
    public string XacNhan { get; set; } = "";
}

public class SearchModel
{
    public DateTime NgayNhan { get; set; } = HotelClock.Today;
    public DateTime NgayTra { get; set; } = HotelClock.Today.AddDays(1);

    [Range(1, int.MaxValue, ErrorMessage = "Số khách phải lớn hơn 0.")]
    [Display(Name = "Số khách")]
    public int SoKhach { get; set; } = 1;
    public bool Valid => NgayNhan.Date >= HotelClock.Today && NgayTra.Date > NgayNhan.Date && SoKhach > 0;
}

public class BookingModel : SearchModel
{
    public int MaLoaiPhong { get; set; }

    [Required(ErrorMessage = "Chọn phương thức thanh toán."), RegularExpression("TaiKhachSan|ChuyenKhoan", ErrorMessage = "Phương thức thanh toán không hợp lệ.")]
    [Display(Name = "Phương thức thanh toán")]
    public string PhuongThucThanhToan { get; set; } = "TaiKhachSan";

    [StringLength(500, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Ghi chú")]
    public string? GhiChu { get; set; }
}

public record RoomResult(LoaiPhong LoaiPhong, int ConTrong);
public record RoomDetail(LoaiPhong LoaiPhong, List<DanhGia> DanhGia);
public record DashboardModel(Dictionary<string, int> SoLuong, List<Revenue> DoanhThu);
public record Revenue(int Nam, int Thang, decimal TongTien);
public static class HotelClock
{
    public static DateTime Now => TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "SE Asia Standard Time");
    public static DateTime Today => Now.Date;
}

public static class Labels
{
    public static string Status(string value) => value switch
    {
        "ChoXacNhan" => "Chờ xác nhận",
        "DaXacNhan" => "Đã xác nhận",
        "HoanThanh" => "Hoàn thành",
        "DaHuy" => "Đã hủy",
        "SanSang" => "Sẵn sàng",
        "BaoTri" => "Bảo trì",
        _ => value
    };
}
