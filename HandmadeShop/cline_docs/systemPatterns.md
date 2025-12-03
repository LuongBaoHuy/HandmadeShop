# System Patterns

## Current Patterns
- Kiến trúc MVC (Model-View-Controller) với ASP.NET Core.
- Sử dụng Entity Framework Core cho ORM.
- Reusable Razor Views cho các thành phần giao diện.
- Phân chia rõ ràng giữa controller, model, view.
- Sử dụng partial views cho các block giao diện lặp lại.

## Best Practices
- Tách biệt logic nghiệp vụ và logic giao diện.
- Đặt tên biến, hàm rõ ràng, tuân thủ C# naming conventions.
- Sử dụng Data Annotations cho validation.
- Xử lý lỗi tập trung (global error handling).
- Sử dụng async/await cho các thao tác I/O.
- Đảm bảo bảo mật thông tin người dùng (hash password, validate input).

## Future Patterns
- Áp dụng Repository Pattern cho quản lý dữ liệu.
- Sử dụng Unit of Work để quản lý transaction.
- Tích hợp caching (Redis/Memcached) cho dữ liệu truy xuất nhiều.
- Tách microservices cho các module lớn (ví dụ: thanh toán, AI chat).
- Áp dụng CQRS cho các nghiệp vụ phức tạp.
- CI/CD pipeline tự động deploy. 