using System.Data;
using HotelBooking.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Services;
public class HotelService(HotelDbContext db)
{
    public IQueryable<Phong> Available(SearchModel m, int? type = null) => db.Phong.Where(p => !p.LoaiPhong!.DaXoa && p.TrangThai == "SanSang" && p.LoaiPhong.SoKhachToiDa >= m.SoKhach && (type == null || p.MaLoaiPhong == type) && !db.DatPhong.Any(d => d.MaPhong == p.MaPhong && (d.TrangThai == "ChoXacNhan" || d.TrangThai == "DaXacNhan") && d.NgayNhan < m.NgayTra.Date && d.NgayTra > m.NgayNhan.Date));
    public async Task<int> Create(int userId, BookingModel m)
    {
        if (!m.Valid)
            throw new InvalidOperationException("Ngày nhận, ngày trả hoặc số khách không hợp lệ.");
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var user = await db.NguoiDung.SingleAsync(x => x.MaNguoiDung == userId);
        if (!user.HoatDong)
            throw new InvalidOperationException("Tài khoản đã bị khóa.");
        var room = await Available(m, m.MaLoaiPhong).Include(p => p.LoaiPhong).OrderBy(p => p.SoPhong).FirstOrDefaultAsync();
        if (room is null)
            throw new InvalidOperationException("Phòng vừa được đặt hoặc không còn phòng trống, hãy chọn ngày khác.");
        var booking = new DatPhong
        {
            MaNguoiDung = userId,
            MaPhong = room.MaPhong,
            NgayNhan = m.NgayNhan.Date,
            NgayTra = m.NgayTra.Date,
            SoKhach = m.SoKhach,
            TongTien = (m.NgayTra.Date - m.NgayNhan.Date).Days * room.LoaiPhong!.GiaMoiDem,
            PhuongThucThanhToan = m.PhuongThucThanhToan,
            GhiChu = m.GhiChu
        };
        db.Add(booking);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        return booking.MaDatPhong;
    }

    public async Task ChangeStatus(int id, string target, int? owner = null)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var d = await db.DatPhong.SingleOrDefaultAsync(x => x.MaDatPhong == id && (owner == null || x.MaNguoiDung == owner));
        if (d is null)
            throw new InvalidOperationException("Không tìm thấy đặt phòng.");
        var allowed = target switch
        {
            "DaXacNhan" => owner == null && d.TrangThai == "ChoXacNhan",
            "HoanThanh" => owner == null && d.TrangThai == "DaXacNhan" && HotelClock.Today >= d.NgayNhan,
            "DaHuy" => (d.TrangThai == "ChoXacNhan" || d.TrangThai == "DaXacNhan") && (owner == null || d.NgayNhan >= HotelClock.Today.AddDays(1)),
            _ => false
        };
        if (!allowed)
            throw new InvalidOperationException("Đặt phòng không đủ điều kiện chuyển trạng thái.");
        d.TrangThai = target;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    public Task<bool> HasActive(IQueryable<Phong> rooms) => db.DatPhong.AnyAsync(d => rooms.Select(p => p.MaPhong).Contains(d.MaPhong) && (d.TrangThai == "ChoXacNhan" || d.TrangThai == "DaXacNhan"));
    public async Task DeleteRoom(int id, bool type)
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        var rooms = db.Phong.Where(p => type ? p.MaLoaiPhong == id : p.MaPhong == id);
        if (await HasActive(rooms))
            throw new InvalidOperationException("Còn đặt phòng chờ xác nhận hoặc đã xác nhận. Hãy hoàn thành hoặc hủy trước khi xóa.");
        if (type)
        {
            var entity = await db.LoaiPhong.SingleOrDefaultAsync(x => x.MaLoaiPhong == id);
            if (entity is null)
                throw new InvalidOperationException("Không tìm thấy loại phòng.");
            entity.DaXoa = true;
        }

        var list = await rooms.ToListAsync();
        if (!type && list.Count == 0)
            throw new InvalidOperationException("Không tìm thấy phòng.");
        foreach (var p in list)
            p.DaXoa = true;
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
