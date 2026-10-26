using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Models;
public class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<NguoiDung> NguoiDung => Set<NguoiDung>();
    public DbSet<LoaiPhong> LoaiPhong => Set<LoaiPhong>();
    public DbSet<Phong> Phong => Set<Phong>();
    public DbSet<DatPhong> DatPhong => Set<DatPhong>();
    public DbSet<DanhGia> DanhGia => Set<DanhGia>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<NguoiDung>().ToTable("NguoiDung").HasKey(x => x.MaNguoiDung);
        b.Entity<LoaiPhong>().ToTable("LoaiPhong").HasKey(x => x.MaLoaiPhong);
        b.Entity<Phong>().ToTable("Phong").HasKey(x => x.MaPhong);
        b.Entity<DatPhong>().ToTable("DatPhong").HasKey(x => x.MaDatPhong);
        b.Entity<DanhGia>().ToTable("DanhGia").HasKey(x => x.MaDanhGia);
        b.Entity<NguoiDung>().HasIndex(x => x.Email).IsUnique();
        b.Entity<NguoiDung>().Property(x => x.SoDienThoai).HasColumnType("varchar(15)");
        b.Entity<NguoiDung>().Property(x => x.MatKhauMaHoa).HasMaxLength(255);
        b.Entity<NguoiDung>().Property(x => x.VaiTro).HasMaxLength(20).HasDefaultValue("KhachHang");
        b.Entity<NguoiDung>().Property(x => x.TokenDatLaiMatKhau).HasMaxLength(100);
        b.Entity<NguoiDung>().Property(x => x.HanTokenDatLai).HasColumnType("datetime");
        b.Entity<NguoiDung>().Property(x => x.HoatDong).HasDefaultValue(true);
        b.Entity<LoaiPhong>().Property(x => x.GiaMoiDem).HasColumnType("decimal(18,2)");
        b.Entity<LoaiPhong>().Property(x => x.DaXoa).HasDefaultValue(false);
        b.Entity<LoaiPhong>().HasQueryFilter(x => !x.DaXoa);
        b.Entity<Phong>().HasQueryFilter(x => !x.DaXoa);
        b.Entity<Phong>().Property(x => x.SoPhong).HasColumnType("varchar(10)");
        b.Entity<Phong>().Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("SanSang");
        b.Entity<Phong>().Property(x => x.DaXoa).HasDefaultValue(false);
        b.Entity<Phong>().HasIndex(x => x.SoPhong).IsUnique().HasFilter("[DaXoa] = 0").HasDatabaseName("UX_Phong_SoPhong");
        b.Entity<Phong>().HasOne(x => x.LoaiPhong).WithMany().HasForeignKey(x => x.MaLoaiPhong);
        b.Entity<DatPhong>().Property(x => x.NgayNhan).HasColumnType("date");
        b.Entity<DatPhong>().Property(x => x.NgayTra).HasColumnType("date");
        b.Entity<DatPhong>().Property(x => x.TongTien).HasColumnType("decimal(18,2)");
        b.Entity<DatPhong>().Property(x => x.PhuongThucThanhToan).HasMaxLength(30).HasDefaultValue("TaiKhachSan");
        b.Entity<DatPhong>().Property(x => x.TrangThai).HasMaxLength(20).HasDefaultValue("ChoXacNhan");
        b.Entity<DatPhong>().Property(x => x.GhiChu).HasMaxLength(500);
        b.Entity<DatPhong>().HasOne(x => x.Phong).WithMany().HasForeignKey(x => x.MaPhong);
        b.Entity<DatPhong>().HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung);
        b.Entity<DatPhong>().HasOne(x => x.DanhGia).WithOne().HasForeignKey<DanhGia>(x => x.MaDatPhong);
        b.Entity<DanhGia>().HasIndex(x => x.MaDatPhong).IsUnique();
        b.Entity<DanhGia>().HasOne(x => x.NguoiDung).WithMany().HasForeignKey(x => x.MaNguoiDung);
        b.Entity<DanhGia>().HasOne<LoaiPhong>().WithMany().HasForeignKey(x => x.MaLoaiPhong);
        foreach (var t in b.Model.GetEntityTypes())
        {
            if (t.FindProperty("NgayTao")is not null)
                b.Entity(t.ClrType).Property("NgayTao").HasColumnType("datetime").HasDefaultValueSql("GETDATE()");
            foreach (var fk in t.GetForeignKeys())
                fk.DeleteBehavior = DeleteBehavior.Restrict;
        }

        b.Entity<NguoiDung>().ToTable(t => t.HasCheckConstraint("CK_NguoiDung_VaiTro", "VaiTro IN ('Admin','KhachHang')"));
        b.Entity<LoaiPhong>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_LoaiPhong_Gia", "GiaMoiDem > 0");
            t.HasCheckConstraint("CK_LoaiPhong_Khach", "SoKhachToiDa > 0");
        });
        b.Entity<Phong>().ToTable(t => t.HasCheckConstraint("CK_Phong_TrangThai", "TrangThai IN ('SanSang','BaoTri')"));
        b.Entity<DatPhong>().ToTable(t =>
        {
            t.HasCheckConstraint("CK_DatPhong_Ngay", "NgayTra > NgayNhan");
            t.HasCheckConstraint("CK_DatPhong_Khach", "SoKhach > 0");
            t.HasCheckConstraint("CK_DatPhong_ThanhToan", "PhuongThucThanhToan IN ('TaiKhachSan','ChuyenKhoan')");
            t.HasCheckConstraint("CK_DatPhong_TrangThai", "TrangThai IN ('ChoXacNhan','DaXacNhan','HoanThanh','DaHuy')");
        });
        b.Entity<DanhGia>().ToTable(t => t.HasCheckConstraint("CK_DanhGia_Sao", "SoSao BETWEEN 1 AND 5"));
    }
}
