# CHÂU THÀNH EV – Hệ thống Admin quản lý trạm sạc (ASP.NET Core MVC, .NET 8)

> 🌐 **TRẢI NGHIỆM WEBSITE TRỰC TUYẾN (LIVE DEMO):**
> 
> 🚀 **Railway URL (Khuyên dùng - Tốc độ cao):** [https://chauthanhev-production.up.railway.app](https://chauthanhev-production.up.railway.app)  
> 🔗 **Render URL:** [https://chauthanhev.onrender.com](https://chauthanhev.onrender.com)  
> 🔑 **Tài khoản demo:** `admin` &nbsp;|&nbsp; **Mật khẩu:** `admin123`  
> *(Trải nghiệm trực tiếp toàn bộ tính năng trên trình duyệt mà không cần tải hay cài đặt mã nguồn)*

---

Hệ thống quản trị trạm sạc xe điện đầy đủ 5 module, dùng chung MỘT nguồn dữ liệu
mock (`Services/MockDataService.cs`) để đảm bảo số liệu giữa Dashboard và các trang
quản lý luôn thống nhất:

1. **Trang chủ (Dashboard)** — trạng thái trụ/cổng sạc, tổng quan hôm nay/tháng này,
   biểu đồ doanh thu, người dùng hoạt động, doughnut trạng thái cổng sạc. Có bộ lọc
   Hôm nay / 7 ngày qua / 30 ngày qua (Chart.js).
2. **Quản lý trụ sạc** — danh sách, tìm kiếm, lọc Online/Offline/Fault, thêm/sửa/xóa,
   xem chi tiết từng cổng sạc.
3. **Quản lý đơn hàng** — 2 tab: Đơn hàng sạc / Đơn hàng nạp tiền, tìm kiếm, lọc theo
   trạng thái, xem chi tiết.
4. **Quản lý tài khoản** — danh sách khách hàng, thêm/sửa/xóa, tìm kiếm, lọc, điều
   chỉnh số dư ví (cộng/trừ tiền có ghi chú).
5. **Vận hành & bảo trì** — 3 tab: Lịch sử lỗi (xử lý lỗi → tự động mở lại cổng/trụ),
   Trụ Offline (kích hoạt lại), Đơn hàng bất thường (xử lý / điều chỉnh số tiền / hoàn
   tiền vào ví khách hàng).

Toàn bộ trang đều có: modal xem chi tiết, modal thêm/sửa, xác nhận trước khi xóa,
phân trang, Toast thông báo sau khi thao tác, và nút xuất CSV/Excel (giả lập bằng
JavaScript, không cần thư viện ngoài).

**Số liệu mẫu:** 28 trụ sạc (2 Offline), 72 cổng sạc (54 Available / 17 Charging /
1 Fault) — đúng theo yêu cầu. Mọi tỷ lệ % và % tăng/giảm kỳ trước/kỳ này đều được
tính tự động trong C# (`MockDataService.PercentChange`), không hard-code, làm tròn
2 chữ số thập phân và xử lý trường hợp mẫu số bằng 0.

## Yêu cầu

- **.NET SDK 8.0** trở lên: https://dotnet.microsoft.com/download
- Visual Studio Code + extension **C# Dev Kit** (hoặc extension "C#" của ms-dotnettools)

## Cách chạy

1. Mở thư mục `ChauThanhEV` bằng Visual Studio Code.
2. Mở Terminal (Ctrl + `) và chạy:

   ```bash
   dotnet restore
   dotnet run
   ```

3. Mở trình duyệt tại địa chỉ được in ra, mặc định: **http://localhost:5080**
4. Đăng nhập bằng tài khoản demo:
   - **Tài khoản:** `admin`
   - **Mật khẩu:** `admin123`

   (Tài khoản/mật khẩu demo được cấu hình trong `appsettings.json`, mục `DemoAccount`.
   Đăng nhập thành công sẽ được cấp cookie xác thực và chuyển tới Trang chủ;
   nếu chưa đăng nhập, mọi truy cập vào Trang chủ sẽ tự động quay lại trang Đăng nhập.)

   Có thể nhấn phím **F5** trong VS Code để chạy kèm debugger (đã có sẵn cấu hình
   `Properties/launchSettings.json`).

## Cấu trúc dự án

```
ChauThanhEV/
├── Controllers/
│   ├── AccountController.cs     # Đăng nhập / đăng xuất (Cookie Authentication)
│   ├── HomeController.cs        # Trang chủ (Dashboard)
│   ├── ChargersController.cs    # Quản lý trụ sạc
│   ├── OrdersController.cs      # Quản lý đơn hàng (sạc + nạp tiền)
│   ├── CustomersController.cs   # Quản lý tài khoản khách hàng
│   ├── OperationsController.cs  # Vận hành & bảo trì
│   └── ControllerExtensions.cs  # Helper hiển thị Toast dùng chung
├── Models/                      # Entity mock (Charger, Connector, Customer,
│                                 # ChargingOrder, TopUpOrder, FaultRecord,
│                                 # AbnormalOrder...) + các ViewModel từng trang
├── Services/
│   └── MockDataService.cs       # NGUỒN DỮ LIỆU DUY NHẤT: seed dữ liệu quan hệ
│                                 # logic + toàn bộ hàm tính toán/CRUD (Singleton)
├── Views/
│   ├── Account/Login.cshtml
│   ├── Home/Index.cshtml        # Dashboard
│   ├── Chargers/Index.cshtml
│   ├── Orders/Index.cshtml
│   ├── Customers/Index.cshtml
│   ├── Operations/Index.cshtml
│   └── Shared/
│       ├── _Layout.cshtml       # Sidebar + topbar + Toast container
│       ├── _Icon.cshtml         # Icon SVG dùng chung cho mọi trang
│       └── _Pager.cshtml        # Phân trang dùng chung cho mọi danh sách
├── wwwroot/
│   ├── css/site.css             # Toàn bộ giao diện (không dùng framework ngoài)
│   └── js/site.js               # Toast, modal, biểu đồ Chart.js, xuất CSV/Excel
├── appsettings.json
├── Program.cs                   # Đăng ký MockDataService làm Singleton
└── ChauThanhEV.csproj
```

## Ghi chú quan trọng khi triển khai thật

- Tài khoản đăng nhập hiện đang **hardcode trong appsettings.json** chỉ để demo giao diện.
  Khi đưa vào sản phẩm thật, hãy thay `AccountController.Login` bằng việc kiểm tra
  tài khoản/mật khẩu (đã băm) trong cơ sở dữ liệu, hoặc gọi API xác thực riêng.
- Toàn bộ dữ liệu (`Services/MockDataService.cs`) là **dữ liệu mẫu sinh trong bộ nhớ**,
  dùng `Random` có seed cố định để tái lập được, và sẽ **mất khi restart ứng dụng**.
  Khi triển khai thật, hãy thay `MockDataService` bằng lớp gọi CSDL/API thực tế nhưng
  giữ nguyên các phương thức tổng hợp (`GetDashboard`, `SearchChargers`...) để không
  phải viết lại Controller/View.
- Vì tất cả trang dùng chung một Singleton, các thao tác như "Xử lý lỗi", "Kích hoạt
  lại trụ", "Hoàn tiền"... sẽ **cập nhật ngay lập tức** trên Dashboard và các trang
  khác — đúng như yêu cầu dữ liệu phải thống nhất toàn hệ thống.
- Nút "Xuất CSV / Xuất Excel" xuất trực tiếp bảng đang hiển thị trên trình duyệt
  (không cần thư viện phía server); phù hợp cho mục đích demo/giả lập theo yêu cầu.
