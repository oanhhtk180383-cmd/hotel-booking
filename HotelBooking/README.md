# HotelBooking — Website đặt phòng khách sạn

## 1. Tổng quan dự án

HotelBooking là website tiếng Việt giúp khách tìm phòng, kiểm tra phòng trống theo ngày và số khách, tạo đặt phòng, theo dõi lịch sử lưu trú và gửi đánh giá. Quản trị viên quản lý loại phòng, phòng, đặt phòng và tài khoản người dùng.

| Nhóm người dùng | Chức năng |
|---|---|
| Khách chưa đăng nhập | Xem loại phòng, tìm phòng theo thời gian lưu trú và số khách, xem chi tiết và đánh giá; đăng ký, đăng nhập và đặt lại mật khẩu |
| Khách hàng | Cập nhật hồ sơ, đổi mật khẩu, đặt phòng, xem hoặc hủy đặt phòng đủ điều kiện, gửi đánh giá sau khi lưu trú |
| Quản trị viên | Xem tổng quan đặt phòng và doanh thu; quản lý loại phòng, phòng, trạng thái đặt phòng và tài khoản người dùng |

Danh sách loại phòng và kết quả tìm kiếm hiển thị tối đa 6 mục mỗi trang; đặt phòng của khách hiển thị 5 mục; các danh sách quản trị hiển thị 10 mục. Thanh toán được ghi nhận theo lựa chọn **Tại khách sạn** hoặc **Chuyển khoản**.

Khi vừa cài đặt, cơ sở dữ liệu mới chưa có danh mục phòng và lịch đặt. Có thể nhập thông tin từ trang quản trị hoặc chạy bước khởi tạo dữ liệu bên dưới. Thư mục dự án có gói `HotelBooking_Database.zip` chứa script SQL với cấu trúc và dữ liệu hiện tại; giải nén rồi làm theo `HUONG_DAN_KHOI_PHUC.txt` để khôi phục. Nếu muốn tạo database mới, chạy mục 4 rồi mục 5. Tệp LocalDB đang chạy (`.mdf`) được SQL Server quản lý bên ngoài thư mục source.

### Công nghệ và dữ liệu

- **Backend:** C#, ASP.NET Core MVC trên .NET 10.
- **Giao diện:** Razor View (`.cshtml`), HTML, CSS, Bootstrap 5 và JavaScript.
- **Truy cập dữ liệu:** Entity Framework Core 10 và SQL Server.
- **Đăng nhập:** Cookie Authentication; mật khẩu được băm bằng `PasswordHasher` của ASP.NET Core Identity.
- **Cơ sở dữ liệu:** 5 bảng `NguoiDung`, `LoaiPhong`, `Phong`, `DatPhong`, `DanhGia`.
- **Xóa mềm:** loại phòng và phòng được đánh dấu xóa để giữ lịch sử đặt phòng.

Chức năng đặt lại mật khẩu xác minh email và số điện thoại trong hệ thống; ứng dụng không gửi email. Dự án khởi tạo cơ sở dữ liệu bằng `EnsureCreated`; hiện không dùng EF Core migrations.

## 2. Yêu cầu cài đặt

1. **.NET 10 SDK**, phiên bản 10.0.100 trở lên. Cấu hình SDK cho phép dùng feature band mới hơn của .NET 10. Chỉ cài Runtime là chưa đủ để build dự án.
2. **SQL Server Express LocalDB** trên Windows, hoặc một SQL Server mà máy chạy ứng dụng có thể kết nối.
3. **Visual Studio 2026 phiên bản 18.0 trở lên** với workload **ASP.NET and web development**, nếu muốn mở và chạy bằng Visual Studio.
4. Kết nối Internet khi restore các gói NuGet lần đầu.

Kiểm tra SDK trong PowerShell:

```powershell
dotnet --list-sdks
dotnet --version
```

## 3. Cấu hình kết nối SQL Server

Mặc định, `appsettings.json` dùng SQL Server LocalDB:

```json
"ConnectionStrings": {
  "HotelDatabase": "Server=(localdb)\\MSSQLLocalDB;Database=HotelBooking;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Nếu dùng SQL Server khác, thay giá trị `HotelDatabase` bằng máy chủ, tên database và thông tin xác thực phù hợp. Ví dụ Windows Authentication với SQL Server Express:

```json
"ConnectionStrings": {
  "HotelDatabase": "Server=.\\SQLEXPRESS;Database=HotelBooking;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Trong JSON, dấu gạch chéo ngược `\` được viết thành `\\`. Với LocalDB, có thể khởi động instance bằng:

```powershell
sqllocaldb start MSSQLLocalDB
```

## 4. Khởi tạo database và tài khoản quản trị

Mở PowerShell tại thư mục chứa `HotelBooking.sln`. Dùng một database mới dành cho ứng dụng khi khởi tạo lần đầu. Đặt email và mật khẩu Admin qua biến môi trường:

```powershell
$env:SeedAdmin__Email = 'admin@hotel.local'
$env:SeedAdmin__Password = 'ThayBangMatKhauCuaBan123!'
dotnet run --project HotelBooking.csproj --no-launch-profile -- --init-db
Remove-Item Env:SeedAdmin__Email -ErrorAction SilentlyContinue
Remove-Item Env:SeedAdmin__Password -ErrorAction SilentlyContinue
```

Chọn mật khẩu có ít nhất 6 ký tự. Lệnh tạo database và 5 bảng khi database chưa tồn tại, sau đó thêm tài khoản Admin nếu email đó chưa có. Lệnh không đổi mật khẩu của tài khoản đã tồn tại. `EnsureCreated` không cập nhật cấu trúc của database cũ; nếu database hiện có cấu trúc khác, hãy cấu hình một database mới cho ứng dụng.

## 5. Khởi tạo dữ liệu hệ thống (tùy chọn)

Sau khi khởi tạo database, có thể tạo danh mục phòng, tài khoản khách hàng, lịch đặt phòng và đánh giá để website sẵn sàng sử dụng:

```powershell
$env:InitialData__GuestPassword = 'KhachHang@2026!'
dotnet run --project HotelBooking.csproj --no-launch-profile -- --seed-initial-data
Remove-Item Env:InitialData__GuestPassword -ErrorAction SilentlyContinue
```

Lệnh bổ sung 4 loại phòng, tối đa 12 phòng, tài khoản khách `khachhang@hotel.local`, 6 lịch đặt và tối đa 3 đánh giá. Mật khẩu tài khoản khách là giá trị đã đặt cho `InitialData__GuestPassword`; lệnh không đổi mật khẩu tài khoản đã tồn tại. Khi nâng cấp database cũ, tài khoản `khach@hotel.local` được đổi sang địa chỉ mới và đặt lại mật khẩu theo giá trị cấu hình.

## 6. Build và chạy website

Chạy các lệnh từ thư mục chứa `HotelBooking.sln`:

```powershell
dotnet restore HotelBooking.sln
dotnet build HotelBooking.sln
dotnet run --project HotelBooking.csproj --launch-profile http
```

Khi terminal báo `Now listening on: http://localhost:5119`, mở các trang:

| Trang | Địa chỉ |
|---|---|
| Trang chủ | [http://localhost:5119/](http://localhost:5119/) |
| Đăng nhập | [http://localhost:5119/Account/Login](http://localhost:5119/Account/Login) |
| Danh sách phòng | [http://localhost:5119/Rooms](http://localhost:5119/Rooms) |
| Quản trị | [http://localhost:5119/Admin](http://localhost:5119/Admin) |

Trang quản trị yêu cầu đăng nhập bằng tài khoản Admin đã khởi tạo ở mục 4. Nếu chưa khởi tạo dữ liệu, vào **Quản trị → Loại phòng** để thêm loại phòng, sau đó vào **Phòng** để tạo phòng.

Giữ terminal mở trong lúc sử dụng website. Nhấn **Ctrl+C** để dừng ứng dụng.

### Chạy bằng Visual Studio

1. Mở `HotelBooking.sln`.
2. Chọn `HotelBooking` làm **Startup Project**.
3. Chọn profile **http**.
4. Nhấn **F5** hoặc **Ctrl+F5**.

## 7. Cấu trúc thư mục

```text
HotelBooking.sln                 Solution
HotelBooking.csproj              Project ASP.NET Core MVC
Program.cs                       Cấu hình và khởi động ứng dụng
Controllers/                     Controller cho khách hàng
  Admin/                         Controller quản trị
Models/                          Entity, ViewModel và DbContext
Services/                        Nghiệp vụ và dữ liệu khởi tạo
Views/                           Giao diện Razor MVC
  Admin/                         Giao diện quản trị
  Shared/                        Layout và thành phần dùng chung
wwwroot/                         CSS, JavaScript, thư viện và ảnh
Properties/                      Profile chạy ứng dụng
appsettings.json                 Chuỗi kết nối và cấu hình
```

## 8. Xử lý lỗi thường gặp

| Hiện tượng | Cách xử lý |
|---|---|
| `dotnet` báo thiếu SDK hoặc phiên bản SDK không phù hợp | Cài .NET 10 SDK theo `global.json`, sau đó mở lại terminal |
| Không kết nối được database | Kiểm tra SQL Server/LocalDB đang chạy và chuỗi `HotelDatabase` trong `appsettings.json` |
| Báo thiếu quyền tạo database hoặc bảng | Cấp quyền phù hợp cho tài khoản SQL Server đang dùng |
| Đăng nhập Admin không thành công | Dùng đúng email và mật khẩu đã nhập khi khởi tạo; lệnh khởi tạo không đặt lại mật khẩu tài khoản cũ |
| Trang phòng chưa có nội dung | Thêm loại phòng và phòng trong trang quản trị, hoặc chạy bước khởi tạo dữ liệu |
| Cổng 5119 đang được sử dụng | Dừng tiến trình website cũ bằng **Ctrl+C**, rồi chạy lại |
| Build báo không thể ghi đè tệp chương trình | Dừng website đang chạy rồi chạy `dotnet build` lại |
| Website dừng trước khi báo `Now listening on` | Đọc lỗi đầu tiên trong terminal; kiểm tra SDK và kết nối database |
