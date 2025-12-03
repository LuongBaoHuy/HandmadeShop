# Technical Context

## Technical Context
- Dự án web bán hàng đồ handmade, xây dựng trên nền tảng ASP.NET Core MVC.
- Hỗ trợ các chức năng thương mại điện tử cơ bản: quản lý sản phẩm, giỏ hàng, thanh toán, blog, quản lý người dùng.
- Hệ thống hướng tới mở rộng với AI, tích hợp thanh toán, tối ưu trải nghiệm người dùng.

## Technologies Used
- ASP.NET Core 8.0 (MVC)
- Entity Framework Core (ORM, SQL Server)
- Bootstrap 5 (UI/UX)
- jQuery (hiệu ứng giao diện, validation)
- Razor Views (template engine)
- Identity hoặc custom authentication
- Visual Studio, SQL Server Management Studio

## Development Setup
- Yêu cầu: .NET 8 SDK, SQL Server, Visual Studio 2022 trở lên.
- Clone repo, mở solution `HandmadeShop.sln`.
- Cấu hình chuỗi kết nối database trong `appsettings.json`:
  - `"ConnectionStrings": { "DefaultConnection": "Server=...;Database=...;User Id=...;Password=...;" }`
- Chạy lệnh migrate để tạo database:  
  - `Update-Database` trong Package Manager Console.
- (Tùy chọn) Thiết lập biến môi trường cho các API key nếu tích hợp AI, thanh toán:
  - `OPENAI_API_KEY`, `STRIPE_API_KEY`, v.v.

## Technical Constraints
- Validation dữ liệu với Data Annotations.
- Đảm bảo mã sạch, dễ bảo trì.
- Chỉ sử dụng các thư viện phổ biến, dễ mở rộng.
- Chưa có streaming cho AI hoặc payment.
- Chưa có CI/CD tự động.

## Future Needs
- Docker hóa ứng dụng để dễ deploy.
- Tích hợp AI (OpenAI API) cho chat, gợi ý sản phẩm.
- Tối ưu hóa hiệu năng với caching (Redis).
- CI/CD pipeline tự động build & deploy.
- Tích hợp các phương thức thanh toán mới.
- Hỗ trợ PWA hoặc mobile app. 