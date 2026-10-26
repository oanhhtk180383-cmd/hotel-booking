using HotelBooking.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Services;
/// <summary>Creates the initial room catalogue, customer account, bookings, and reviews.</summary>
public static class InitialDataSeeder
{
    public static async Task SeedAsync(HotelDbContext db, IPasswordHasher<NguoiDung> hasher, string guestPassword)
    {
        if (string.IsNullOrWhiteSpace(guestPassword) || guestPassword.Length < 6)
            throw new InvalidOperationException("Đặt InitialData__GuestPassword có ít nhất 6 ký tự trước khi khởi tạo dữ liệu.");
        await using var tx = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var roomTypes = new[]
        {
            new LoaiPhong
            {
                TenLoaiPhong = "Tiêu chuẩn",
                GiaMoiDem = 650000,
                SoKhachToiDa = 2,
                MoTa = "Phòng tiêu chuẩn với một giường đôi, không gian sáng và thoáng. Phù hợp cho một người hoặc hai khách nghỉ ngơi trong chuyến đi.",
                TienNghi = "Wifi, điều hòa, TV, phòng tắm riêng",
                HinhAnh = "/images/paradise/room-2.jpg"
            },
            new LoaiPhong
            {
                TenLoaiPhong = "Deluxe",
                GiaMoiDem = 950000,
                SoKhachToiDa = 2,
                MoTa = "Phòng giường đôi rộng rãi, nội thất tông ấm và góc ngồi thư giãn. Một lựa chọn thoải mái cho hai khách.",
                TienNghi = "Wifi, điều hòa, TV, bàn làm việc, phòng tắm riêng",
                HinhAnh = "/images/paradise/room-1.jpg"
            },
            new LoaiPhong
            {
                TenLoaiPhong = "Hai giường đơn",
                GiaMoiDem = 850000,
                SoKhachToiDa = 2,
                MoTa = "Phòng với hai giường đơn riêng biệt, phù hợp cho bạn bè hoặc đồng nghiệp cùng lưu trú.",
                TienNghi = "Wifi, điều hòa, TV, hai giường đơn, phòng tắm riêng",
                HinhAnh = "/images/paradise/room-3.jpg"
            },
            new LoaiPhong
            {
                TenLoaiPhong = "Gia đình",
                GiaMoiDem = 1450000,
                SoKhachToiDa = 4,
                MoTa = "Không gian nghỉ ngơi cho gia đình hoặc nhóm nhỏ, với hai giường đôi và sức chứa tối đa bốn khách.",
                TienNghi = "Wifi, điều hòa, TV, hai giường đôi, phòng tắm riêng",
                HinhAnh = "/images/paradise/room-4.jpg"
            }
        };
        var previousNames = new[] { "Standard", "Deluxe", "Twin", "Family" };
        var rooms = new List<Phong>();
        for (var i = 0; i < roomTypes.Length; i++)
        {
            var type = await db.LoaiPhong.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenLoaiPhong == roomTypes[i].TenLoaiPhong);
            if (type is null)
            {
                type = await db.LoaiPhong.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.TenLoaiPhong == previousNames[i]);
                if (type is not null)
                {
                    type.TenLoaiPhong = roomTypes[i].TenLoaiPhong;
                    await db.SaveChangesAsync();
                }
            }
            if (type is null)
            {
                type = roomTypes[i];
                db.Add(type);
                await db.SaveChangesAsync();
            }

            if (type.DaXoa)
                continue;
            for (var j = 1; j <= 3; j++)
            {
                var number = $"{i + 1}0{j}";
                var room = await db.Phong.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.MaLoaiPhong == type.MaLoaiPhong && x.SoPhong == number);
                if (room is null && !await db.Phong.AnyAsync(x => x.SoPhong == number))
                {
                    room = new Phong
                    {
                        MaLoaiPhong = type.MaLoaiPhong,
                        SoPhong = number,
                        TrangThai = i == 3 && j == 3 ? "BaoTri" : "SanSang"
                    };
                    db.Add(room);
                    await db.SaveChangesAsync();
                }

                if (room is not null && !room.DaXoa)
                    rooms.Add(room);
            }
        }

        const string guestEmail = "khachhang@hotel.local";
        var customer = await db.NguoiDung.SingleOrDefaultAsync(x => x.Email == guestEmail);
        if (customer is null)
        {
            customer = await db.NguoiDung.SingleOrDefaultAsync(x => x.Email == "khach@hotel.local");
            if (customer is not null)
            {
                customer.Email = guestEmail;
                customer.MatKhauMaHoa = hasher.HashPassword(customer, guestPassword);
                await db.SaveChangesAsync();
            }
        }
        if (customer is null)
        {
            customer = new NguoiDung
            {
                HoTen = "Nguyễn Minh Anh",
                Email = guestEmail,
                SoDienThoai = "0912345678"
            };
            customer.MatKhauMaHoa = hasher.HashPassword(customer, guestPassword);
            db.Add(customer);
            await db.SaveChangesAsync();
        }

        var dates = new[]
        {
            HotelClock.Today.AddDays(-35),
            HotelClock.Today.AddDays(-12),
            HotelClock.Today.AddDays(-5),
            HotelClock.Today.AddDays(7),
            HotelClock.Today.AddDays(12),
            HotelClock.Today.AddDays(-2)
        };
        var states = new[]
        {
            "HoanThanh",
            "HoanThanh",
            "HoanThanh",
            "ChoXacNhan",
            "DaXacNhan",
            "DaHuy"
        };
        var customerHasBookings = await db.DatPhong.AnyAsync(x => x.MaNguoiDung == customer.MaNguoiDung);
        for (var i = 0; !customerHasBookings && i < dates.Length && rooms.Count > 0; i++)
        {
            var room = rooms[(i * 3) % rooms.Count];
            var type = await db.LoaiPhong.SingleAsync(x => x.MaLoaiPhong == room.MaLoaiPhong);
            var departure = dates[i].AddDays(2);
            if (room.TrangThai != "SanSang" || await db.DatPhong.AnyAsync(x => x.MaPhong == room.MaPhong && (x.TrangThai == "ChoXacNhan" || x.TrangThai == "DaXacNhan") && x.NgayNhan < departure && x.NgayTra > dates[i]))
                continue;
            var booking = new DatPhong
            {
                MaNguoiDung = customer.MaNguoiDung,
                MaPhong = room.MaPhong,
                NgayNhan = dates[i],
                NgayTra = departure,
                SoKhach = 2,
                TongTien = 2 * type.GiaMoiDem,
                TrangThai = states[i],
                PhuongThucThanhToan = i % 2 == 0 ? "TaiKhachSan" : "ChuyenKhoan",
                GhiChu = null,
                NgayTao = dates[i].AddDays(-3)
            };
            db.Add(booking);
            await db.SaveChangesAsync();
            if (states[i] == "HoanThanh")
            {
                db.Add(new DanhGia { MaDatPhong = booking.MaDatPhong, MaNguoiDung = customer.MaNguoiDung, MaLoaiPhong = room.MaLoaiPhong, SoSao = i == 1 ? 4 : 5, NoiDung = new[] { "Phòng sạch, tiện nghi đầy đủ. Mình đã có một kỳ nghỉ thoải mái.", "Không gian yên tĩnh, phù hợp nghỉ ngơi. Sẽ quay lại lần sau.", "Giường thoải mái, phòng rộng và sáng." }[i], NgayTao = departure });
                await db.SaveChangesAsync();
            }
        }

        await tx.CommitAsync();
    }
}
