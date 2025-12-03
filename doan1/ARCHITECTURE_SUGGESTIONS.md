# CÁC CÁCH TỔ CHỨC LẠI KIẾN TRÚC PROJECT

## 🏛️ KIẾN TRÚC HIỆN TẠI (Monolithic MVC)
```
doan1/
├── Controllers/        # MVC Controllers
├── Views/             # Razor Views  
├── Models/            # Domain Models
├── Data/              # DbContext + Configurations
├── Services/          # Business Services
└── wwwroot/           # Static files
```

## 🚀 KIẾN TRÚC ĐỀ XUẤT 1: LAYERED ARCHITECTURE

### Tạo các Class Library projects riêng:

```bash
# 1. Core Layer (Domain Models)
dotnet new classlib -n doan1.Core
dotnet new classlib -n doan1.Domain

# 2. Data Access Layer
dotnet new classlib -n doan1.Data
dotnet new classlib -n doan1.Infrastructure

# 3. Business Logic Layer
dotnet new classlib -n doan1.Business
dotnet new classlib -n doan1.Application

# 4. Web Layer (giữ nguyên)
# doan1.Web (project MVC hiện tại)
```

### Cấu trúc thư mục:
```
Solution/
├── src/
│   ├── doan1.Core/                 # Domain Layer
│   │   ├── Entities/               # Domain Models
│   │   ├── Interfaces/             # Repository Interfaces
│   │   └── ValueObjects/           # Value Objects
│   │
│   ├── doan1.Data/                 # Data Access Layer
│   │   ├── Context/                # DbContext
│   │   ├── Repositories/           # Repository Implementations
│   │   ├── Configurations/         # Entity Configurations
│   │   └── Migrations/             # EF Migrations
│   │
│   ├── doan1.Business/             # Business Logic Layer
│   │   ├── Services/               # Business Services
│   │   ├── DTOs/                   # Data Transfer Objects
│   │   └── Validators/             # Business Validation
│   │
│   └── doan1.Web/                  # Presentation Layer
│       ├── Controllers/            # MVC Controllers
│       ├── Views/                  # Razor Views
│       ├── ViewModels/             # View Models
│       └── wwwroot/                # Static files
```

## 🎯 KIẾN TRÚC ĐỀ XUẤT 2: CLEAN ARCHITECTURE

### Cấu trúc Clean Architecture:
```
Solution/
├── Core/                           # Enterprise Business Rules
│   ├── doan1.Domain/               # Entities + Business Rules
│   └── doan1.Application/          # Use Cases + Interfaces
│
├── Infrastructure/                 # External Concerns
│   ├── doan1.Infrastructure.Data/  # Database + EF Core
│   └── doan1.Infrastructure.Services/ # External Services
│
└── Presentation/                   # User Interface
    └── doan1.Web/                  # MVC Web App
```

### Dependency Flow:
```
Web → Application → Domain
Infrastructure → Application
Infrastructure → Domain
```

## 🔧 LỢI ÍCH CUA VIỆC TÁCH LAYERS

### ✅ **Separation of Concerns**
- Mỗi layer có trách nhiệm riêng biệt
- Dễ maintain và debug
- Code organization tốt hơn

### ✅ **Testability**
- Unit test dễ dàng hơn
- Mock dependencies
- Isolated testing

### ✅ **Scalability**
- Thêm features mới dễ dàng
- Replace components độc lập
- Team collaboration tốt hơn

### ✅ **Reusability**
- Business logic tái sử dụng được
- Data layer có thể dùng cho nhiều UI
- API và Web app cùng business logic

## 🚧 CÁCH MIGRATE HIỆN TẠI

### **Bước 1: Tạo projects mới**
```bash
# Tạo solution mới
dotnet new sln -n HandmadeShop

# Tạo các projects
dotnet new classlib -n HandmadeShop.Core
dotnet new classlib -n HandmadeShop.Data  
dotnet new classlib -n HandmadeShop.Business
dotnet new mvc -n HandmadeShop.Web

# Add vào solution
dotnet sln add **/*.csproj
```

### **Bước 2: Move Models**
```
doan1/Models/ → HandmadeShop.Core/Entities/
```

### **Bước 3: Move Data Context**
```
doan1/Data/ → HandmadeShop.Data/Context/
```

### **Bước 4: Tạo Repository Pattern**
```csharp
// HandmadeShop.Core/Interfaces/IRepository.cs
public interface IRepository<T> where T : class
{
    Task<T> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// HandmadeShop.Data/Repositories/Repository.cs
public class Repository<T> : IRepository<T> where T : class
{
    private readonly HandmadeShopContext _context;
    private readonly DbSet<T> _dbSet;
    
    // Implementation...
}
```

### **Bước 5: Move Services**
```
doan1/Services/ → HandmadeShop.Business/Services/
```

### **Bước 6: Update Dependencies**
```csharp
// HandmadeShop.Web.csproj
<ProjectReference Include="..\HandmadeShop.Business\HandmadeShop.Business.csproj" />
<ProjectReference Include="..\HandmadeShop.Data\HandmadeShop.Data.csproj" />

// HandmadeShop.Business.csproj  
<ProjectReference Include="..\HandmadeShop.Core\HandmadeShop.Core.csproj" />

// HandmadeShop.Data.csproj
<ProjectReference Include="..\HandmadeShop.Core\HandmadeShop.Core.csproj" />
```

## 🎨 TÍNH NĂNG NÂNG CAO CÓ THỂ THÊM

### **1. CQRS Pattern** (Command Query Responsibility Segregation)
```csharp
// Commands (Write operations)
public interface ICommand<TResult> { }
public interface ICommandHandler<TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<TResult> Handle(TCommand command);
}

// Queries (Read operations)  
public interface IQuery<TResult> { }
public interface IQueryHandler<TQuery, TResult> where TQuery : IQuery<TResult>
{
    Task<TResult> Handle(TQuery query);
}
```

### **2. Mediator Pattern**
```bash
# Install MediatR
dotnet add package MediatR
dotnet add package MediatR.Extensions.Microsoft.DependencyInjection
```

### **3. AutoMapper**
```bash
# Install AutoMapper
dotnet add package AutoMapper
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
```

### **4. FluentValidation**
```bash
# Install FluentValidation
dotnet add package FluentValidation
dotnet add package FluentValidation.AspNetCore
```

## 📚 KẾT LUẬN

### **Recommended Approach:**
1. **Bắt đầu với Layered Architecture** (đơn giản hơn)
2. **Migrate từng phần một** (không làm hết một lúc)
3. **Giữ nguyên MVC structure** cho presentation layer
4. **Tách Data layer** trước tiên
5. **Sau đó tách Business layer**

### **Timeline đề xuất:**
- **Week 1**: Tạo projects + move Models
- **Week 2**: Move Data context + Repository pattern
- **Week 3**: Move Services + Business logic
- **Week 4**: Testing + refining

Việc tách layers sẽ làm project **professional hơn**, **dễ maintain hơn**, và **scalable hơn**!
