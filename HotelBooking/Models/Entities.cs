using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Models;
public class NguoiDung
{
    public int MaNguoiDung { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Họ tên")]
    public string HoTen { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), EmailAddress(ErrorMessage = "Email không hợp lệ."), StringLength(150, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Vui lòng nhập {0}."), RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại gồm 10 chữ số, bắt đầu bằng 0.")]
    [Display(Name = "Số điện thoại")]
    public string SoDienThoai { get; set; } = "";
    public string MatKhauMaHoa { get; set; } = "";
    public string VaiTro { get; set; } = "KhachHang";
    public string? TokenDatLaiMatKhau { get; set; }
    public DateTime? HanTokenDatLai { get; set; }
    public bool HoatDong { get; set; } = true;
    public DateTime NgayTao { get; set; }
}

public class LoaiPhong
{
    public int MaLoaiPhong { get; set; }

    [Required(ErrorMessage = "Nhập tên loại phòng."), StringLength(100, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Tên loại phòng")]
    public string TenLoaiPhong { get; set; } = "";
    public string? MoTa { get; set; }

    [Range(typeof(decimal), "0.01", "9999999999999999.99", ErrorMessage = "Giá phải lớn hơn 0.")]
    [Display(Name = "Giá mỗi đêm")]
    public decimal GiaMoiDem { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Số khách phải lớn hơn 0.")]
    [Display(Name = "Số khách tối đa")]
    public int SoKhachToiDa { get; set; } = 1;

    [StringLength(500, ErrorMessage = "{0} tối đa {1} ký tự.")]
    public string? TienNghi { get; set; }

    [StringLength(255, ErrorMessage = "{0} tối đa {1} ký tự.")]
    public string? HinhAnh { get; set; }
    public bool DaXoa { get; set; }
}

public class Phong
{
    public int MaPhong { get; set; }
    public int MaLoaiPhong { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập {0}."), StringLength(10, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Số phòng")]
    public string SoPhong { get; set; } = "";

    [Required(ErrorMessage = "Chọn trạng thái phòng."), RegularExpression("SanSang|BaoTri", ErrorMessage = "Trạng thái phòng không hợp lệ.")]
    public string TrangThai { get; set; } = "SanSang";
    public bool DaXoa { get; set; }
    public LoaiPhong? LoaiPhong { get; set; }
}

public class DatPhong
{
    public int MaDatPhong { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaPhong { get; set; }
    public DateTime NgayNhan { get; set; }
    public DateTime NgayTra { get; set; }
    public int SoKhach { get; set; }
    public decimal TongTien { get; set; }
    public string PhuongThucThanhToan { get; set; } = "TaiKhachSan";
    public string TrangThai { get; set; } = "ChoXacNhan";
    public string? GhiChu { get; set; }
    public DateTime NgayTao { get; set; }
    public Phong Phong { get; set; } = null !;
    public NguoiDung NguoiDung { get; set; } = null !;
    public DanhGia? DanhGia { get; set; }
}

public class DanhGia
{
    public int MaDanhGia { get; set; }
    public int MaDatPhong { get; set; }
    public int MaNguoiDung { get; set; }
    public int MaLoaiPhong { get; set; }

    [Range(1, 5, ErrorMessage = "Số sao phải từ 1 đến 5.")]
    [Display(Name = "Số sao")]
    public int SoSao { get; set; } = 5;

    [StringLength(1000, ErrorMessage = "{0} tối đa {1} ký tự.")]
    [Display(Name = "Nhận xét")]
    public string? NoiDung { get; set; }
    public DateTime NgayTao { get; set; }
    public NguoiDung NguoiDung { get; set; } = null !;
}
