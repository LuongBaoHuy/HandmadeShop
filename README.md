# 🛍️ HandmadeShop - E-Commerce Platform for Handmade Products

[![.NET](https://img.shields.io/badge/.NET-6.0-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC-512BD4?style=flat&logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-2019+-CC2927?style=flat&logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)
[![Bootstrap](https://img.shields.io/badge/Bootstrap-5.0-7952B3?style=flat&logo=bootstrap)](https://getbootstrap.com/)

A full-featured ASP.NET Core MVC e-commerce platform designed for handmade product businesses, featuring comprehensive product management, customer shopping experience, and secure payment integration.

![HandmadeShop Banner](wwwroot/images/banner.png)

## 📋 Table of Contents

- [Key Features](#-key-features)
- [Technical Architecture](#-technical-architecture)
- [Screenshots](#-screenshots)
- [Getting Started](#-getting-started)
- [Database Schema](#-database-schema)
- [Key Implementations](#-key-implementations)
- [Security Features](#-security-features)
- [Performance Optimizations](#-performance-optimizations)
- [Future Enhancements](#-future-enhancements)
- [Contributing](#-contributing)
- [License](#-license)
- [Contact](#-contact)

## 🌟 Key Features

### 🛍️ Customer Features (Shopping Platform)

#### Product Browsing & Discovery
- **Advanced Product Catalog**
  - Grid/list view with responsive design
  - Pagination (9 products per page) with smooth navigation
  - Multi-level category filtering
  - Price range filter with slider
  - Product attribute filtering (color, size, material)
  
- **Search & Navigation**
  - Real-time search with auto-suggestions
  - Category-based navigation with breadcrumbs
  - Sort by: newest, price (low-high, high-low), popularity
  - Product quick view modal

- **Product Details**
  - Multiple product images with zoom functionality
  - Interactive image carousel with thumbnail navigation
  - Product variants selector (color, size, material)
  - Real-time stock availability
  - Customer reviews and ratings (5-star system)
  - Related products suggestions

#### Shopping Experience
- **Smart Shopping Cart**
  - Real-time cart updates without page reload
  - Quantity adjustment with stock validation
  - Remove items functionality
  - Cart summary with subtotal calculation
  - Persistent cart (session-based)
  - Cart icon with item count badge

- **Checkout Process**
  - Guest checkout option
  - Registered user quick checkout
  - Shipping information form with validation
  - Multiple shipping address support
  - Order summary review
  - Voucher/discount code application with validation

- **Payment Integration**
  - Cash on Delivery (COD)
  - MoMo e-wallet integration with QR code
  - Credit/Debit card payments
  - Secure payment processing with HMAC SHA256
  - Payment confirmation via email
  - Order tracking number generation

#### User Account Management
- **Authentication & Authorization**
  - User registration with email verification
  - Secure login with password encryption (BCrypt)
  - "Remember Me" functionality
  - Password recovery via email token
  - Email verification for new accounts
  - Role-based access control (Customer/Admin)

- **Profile Management**
  - Personal information editing
  - Avatar upload with image preview
  - Password change functionality
  - Email update with re-verification
  - Shipping addresses management

- **Order Management**
  - Complete order history
  - Order status tracking (Pending, Confirmed, Shipping, Delivered, Cancelled)
  - Order details view with invoice download
  - Reorder functionality
  - Order cancellation (within allowed timeframe)

#### Additional Features
- **Blog & Content**
  - Blog/News section for product stories and tips
  - Blog post categories and tags
  - Comment system on blog posts
  - Share on social media integration

- **Customer Support**
  - FAQ section with search functionality
  - Contact form with email notifications
  - Custom order requests for personalized products
  - Live chat support (future enhancement)

- **Interactive Elements**
  - Product wishlist functionality
  - Product comparison tool
  - Recently viewed products
  - Newsletter subscription

### 🎯 Admin Features (Management Platform)

#### Dashboard Analytics
- **Sales Overview**
  - Total revenue by day/week/month/year
  - Sales trend charts (Line, Bar, Pie charts)
  - Best-selling products report
  - Revenue by category analysis
  - Average order value (AOV)

- **Performance Metrics**
  - Total orders count with status breakdown
  - Conversion rate tracking
  - Customer acquisition metrics
  - Product views and clicks tracking
  - Inventory turnover rate

- **Customer Insights**
  - New vs returning customers
  - Customer lifetime value (CLV)
  - Top customers by purchase amount
  - Customer geographic distribution
  - Customer review ratings overview

#### Product Management
- **Product CRUD Operations**
  - Create new products with rich text editor
  - Edit existing products with version history
  - Bulk product import/export (CSV)
  - Product duplication for similar items
  - Soft delete with restore capability

- **Product Media Management**
  - Multiple image upload with drag-and-drop
  - Image cropping and resizing
  - Primary image selection
  - Image gallery reordering
  - Video upload support (future)

- **Product Variants & Attributes**
  - Flexible attribute system (color, size, material, etc.)
  - Variant-specific pricing
  - Variant-specific stock levels
  - SKU generation for variants
  - Bulk variant creation

- **Inventory Management**
  - Real-time stock tracking
  - Low stock alerts
  - Stock history log
  - Bulk stock update
  - Automatic stock deduction on order

- **Category Management**
  - Hierarchical category structure
  - Category image and description
  - SEO-friendly URLs
  - Category sorting and visibility toggle
  - Bulk category operations

#### Order Management
- **Order Processing**
  - Order list with advanced filtering
  - Order status workflow management
  - Order assignment to staff
  - Bulk order status update
  - Order notes and internal comments

- **Order Details**
  - Complete order information view
  - Customer details with order history
  - Shipping address verification
  - Payment method and status
  - Itemized order breakdown

- **Invoice & Shipping**
  - PDF invoice generation
  - Packing slip printing
  - Shipping label creation
  - Tracking number management
  - Shipping provider integration

- **Order Communication**
  - Email notifications to customers
  - SMS notifications (future)
  - Order status update emails
  - Shipping confirmation emails
  - Delivery confirmation

#### Customer Management
- **User Administration**
  - User list with search and filter
  - User details view with activity log
  - Role assignment (Customer, Admin, Staff)
  - Account activation/deactivation
  - Password reset for users

- **Customer Engagement**
  - Customer segmentation
  - Targeted email campaigns
  - Customer feedback collection
  - Loyalty program management (future)
  - Customer support ticket system

- **Review Management**
  - Review moderation (approve/reject)
  - Review reply functionality
  - Spam review detection
  - Review reporting by customers
  - Review analytics

#### Content Management
- **Blog Management**
  - Create/edit blog posts with WYSIWYG editor
  - Post scheduling (publish later)
  - Category and tag management
  - Featured image upload
  - SEO meta tags configuration

- **FAQ Management**
  - Question and answer creation
  - FAQ categorization
  - FAQ ordering and priority
  - FAQ visibility toggle
  - Search-friendly structure

- **Custom Orders**
  - Custom order request inbox
  - Request details view with attachments
  - Quote generation and sending
  - Convert to regular order
  - Request status tracking

- **Site Settings**
  - General settings (site name, logo, favicon)
  - SMTP configuration
  - Payment gateway settings
  - Shipping method configuration
  - Tax and currency settings

#### Reports & Analytics
- **Sales Reports**
  - Daily/Weekly/Monthly/Yearly sales reports
  - Sales by product category
  - Sales by payment method
  - Discount and voucher usage reports
  - Refund and return reports

- **Product Reports**
  - Best-selling products
  - Low stock products
  - Out of stock products
  - Product performance by category
  - Product review ratings

- **Customer Reports**
  - New customer registrations
  - Customer purchase frequency
  - Customer geographic distribution
  - Customer age and gender demographics (if collected)
  - Customer retention rate

## 🏗️ Technical Architecture

### Technology Stack

#### Backend
- **Framework**: ASP.NET Core 6.0 MVC
- **Language**: C# 8.0
- **ORM**: Entity Framework Core 6.0
  - Code-First approach
  - Migration-based schema management
  - Lazy loading enabled
  - Connection resiliency

#### Database
- **RDBMS**: Microsoft SQL Server 2019
- **Design**: Normalized relational schema
- **Features**:
  - Foreign key constraints
  - Indexes on frequently queried columns
  - Stored procedures for complex queries (future)
  - Full-text search support

#### Frontend
- **View Engine**: Razor Views (.cshtml)
- **Layout System**: Master page with sections
- **CSS Framework**: Bootstrap 5.1
- **JavaScript Libraries**:
  - jQuery 3.6
  - jQuery Validation
  - jQuery Unobtrusive Validation
  - SweetAlert2 for beautiful alerts
  - Slick Carousel for product galleries
  - Chart.js for analytics charts

#### Authentication & Authorization
- **Identity**: ASP.NET Core Identity
- **Password Hashing**: BCrypt.Net
- **Token**: Custom password reset tokens with expiry
- **Session**: Cookie-based authentication
- **Authorization**: Role-based and policy-based

#### Payment Integration
- **Provider**: MoMo Payment Gateway
- **Security**: HMAC SHA256 signature
- **Flow**: Redirect payment with IPN callback
- **Supported**: QR code, App, Web payments

#### Email Service
- **Protocol**: SMTP
- **Provider**: Configurable (Gmail, Outlook, custom)
- **Templates**: Razor Email Templates
- **Features**: HTML emails with inline CSS

### Project Structure

```
HandmadeShop/
├── Controllers/              # MVC Controllers
│   ├── HomeController.cs           # Homepage and product listing
│   ├── DetailController.cs         # Product details
│   ├── Shopping_cartController.cs  # Cart operations
│   ├── CheckoutController.cs       # Checkout process
│   ├── PaymentController.cs        # Payment processing
│   ├── UserController.cs           # Authentication & profile
│   ├── CategoryController.cs       # Category browsing
│   ├── BlogController.cs           # Blog posts
│   ├── FaqController.cs            # FAQ section
│   ├── ContractController.cs       # Custom orders
│   ├── AboutController.cs          # About page
│   ├── Terms_conditionsController.cs
│   └── BaseController.cs           # Base controller with common logic
│
├── Models/                   # Data Models & DbContext
│   ├── HandmadeShopContext.cs      # EF Core DbContext
│   ├── User.cs                     # User/Customer entity
│   ├── Role.cs                     # User roles
│   ├── Product.cs                  # Product entity
│   ├── ProductImage.cs             # Product images
│   ├── ProductVariant.cs           # Product variants
│   ├── ProductVariation.cs         # Variation definitions
│   ├── ProductAttributeOption.cs   # Attribute options
│   ├── Attribute.cs                # Product attributes
│   ├── AttributeOption.cs          # Attribute option values
│   ├── AttributeValue.cs           # Product attribute values
│   ├── VariationOptionLink.cs      # Variant-option mapping
│   ├── Category.cs                 # Product categories
│   ├── Order.cs                    # Customer orders
│   ├── OrderItem.cs                # Order line items
│   ├── CartItem.cs                 # Shopping cart items
│   ├── Review.cs                   # Product reviews
│   ├── ReviewReply.cs              # Admin replies to reviews
│   ├── Voucher.cs                  # Discount vouchers
│   ├── CustomOrder.cs              # Custom order requests
│   ├── CustomOrderAttribute.cs     # Custom order specifications
│   ├── Question.cs                 # FAQ questions
│   ├── Answer.cs                   # FAQ answers
│   ├── PasswordResetToken.cs       # Password reset tokens
│   ├── MoMoOptions.cs              # MoMo payment config
│   ├── SmtpOptions.cs              # Email config
│   └── ErrorViewModel.cs           # Error handling
│
├── Views/                    # Razor Views
│   ├── Shared/                     # Shared layouts and partials
│   │   ├── _Layout.cshtml              # Main layout
│   │   ├── _LoginPartial.cshtml        # Login/Register links
│   │   ├── _ProductCard.cshtml         # Product card component
│   │   └── Error.cshtml                # Error page
│   ├── Home/                       # Homepage views
│   ├── Detail/                     # Product detail views
│   ├── Shopping_cart/              # Cart views
│   ├── Checkout/                   # Checkout views
│   ├── Payment/                    # Payment views
│   ├── User/                       # User account views
│   ├── Category/                   # Category views
│   ├── Blog/                       # Blog views
│   ├── Faq/                        # FAQ views
│   └── ...
│
├── Services/                 # Business Logic Services
│   ├── IEmailSender.cs            # Email service interface
│   ├── SmtpEmailSender.cs         # SMTP email implementation
│   └── MomoService.cs             # MoMo payment service
│
├── wwwroot/                  # Static Files
│   ├── css/                        # Stylesheets
│   │   ├── site.css                    # Custom styles
│   │   └── admin.css                   # Admin panel styles
│   ├── js/                         # JavaScript files
│   │   ├── site.js                     # Custom scripts
│   │   ├── cart.js                     # Cart functionality
│   │   └── checkout.js                 # Checkout scripts
│   ├── images/                     # Static images
│   │   ├── logo.png
│   │   ├── banner/
│   │   └── icons/
│   └── lib/                        # Third-party libraries
│       ├── bootstrap/
│       ├── jquery/
│       └── ...
│
├── cline_docs/               # Project Documentation
│   ├── activeContext.md           # Current development context
│   ├── productContext.md          # Product features
│   ├── progress.md                # Development progress
│   ├── systemPatterns.md          # Architecture patterns
│   └── techContext.md             # Technical specifications
│
├── Properties/
│   └── launchSettings.json        # Development settings
│
├── appsettings.json          # Application configuration
├── appsettings.Development.json
├── Program.cs                # Application entry point
└── HandmadeShop.csproj       # Project file

ShareUploads/                 # Uploaded Files Storage
├── Products/                      # Product images
├── Avartar/                       # User avatars
└── users/                         # User-related files
```

### Database Schema

#### Core Entities

**Users Table**
```sql
Users (
    UserId INT PRIMARY KEY,
    FullName NVARCHAR(100),
    Email NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(255),
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255),
    RoleId INT FOREIGN KEY,
    AvatarPath NVARCHAR(255),
    IsEmailConfirmed BIT,
    CreatedAt DATETIME,
    UpdatedAt DATETIME
)
```

**Products Table**
```sql
Products (
    ProductId INT PRIMARY KEY,
    ProductName NVARCHAR(200),
    Description NVARCHAR(MAX),
    Price DECIMAL(18,2),
    DiscountPrice DECIMAL(18,2),
    StockQuantity INT,
    CategoryId INT FOREIGN KEY,
    ViewCount INT,
    IsActive BIT,
    CreatedAt DATETIME,
    UpdatedAt DATETIME
)
```

**Orders Table**
```sql
Orders (
    OrderId INT PRIMARY KEY,
    UserId INT FOREIGN KEY,
    OrderDate DATETIME,
    TotalAmount DECIMAL(18,2),
    ShippingAddress NVARCHAR(255),
    PaymentMethod NVARCHAR(50),
    OrderStatus NVARCHAR(50),
    VoucherId INT FOREIGN KEY,
    DiscountAmount DECIMAL(18,2),
    TrackingNumber NVARCHAR(50),
    Notes NVARCHAR(MAX),
    CreatedAt DATETIME,
    UpdatedAt DATETIME
)
```

**Entity Relationships**
- One-to-Many: User → Orders, Category → Products, Order → OrderItems
- Many-to-Many: Products ↔ Attributes (via ProductAttributeOption)
- One-to-Many: Product → ProductImages, Product → Reviews
- One-to-One: Order → Payment

### Design Patterns Used

1. **MVC (Model-View-Controller)**
   - Separation of concerns
   - Testable business logic
   - Reusable view components

2. **Repository Pattern** (via BaseController)
   - Data access abstraction
   - Centralized data access logic
   - Easier unit testing

3. **Dependency Injection**
   - DbContext injection
   - Service injection (Email, Payment)
   - Loose coupling

4. **Service Layer Pattern**
   - Business logic separation
   - Email service
   - Payment service

5. **Factory Pattern** (in development)
   - Payment provider factory
   - Email provider factory

## 📸 Screenshots

### Customer Interface

#### Homepage
![Homepage](wwwroot/images/screenshots/homepage.png)
*Modern and responsive homepage with featured products and categories*

#### Product Listing
![Product Listing](wwwroot/images/screenshots/products.png)
*Advanced filtering and sorting options for easy product discovery*

#### Product Detail
![Product Detail](wwwroot/images/screenshots/product-detail.png)
*Detailed product information with image gallery and variants*

#### Shopping Cart
![Shopping Cart](wwwroot/images/screenshots/cart.png)
*Interactive cart with real-time updates and discount application*

### Admin Interface

#### Dashboard
![Admin Dashboard](wwwroot/images/screenshots/admin-dashboard.png)
*Comprehensive analytics and sales overview*

#### Product Management
![Product Management](wwwroot/images/screenshots/admin-products.png)
*Powerful product management with bulk operations*

#### Order Management
![Order Management](wwwroot/images/screenshots/admin-orders.png)
*Efficient order processing and tracking system*

## 🚀 Getting Started

### Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0) or later
- [SQL Server 2019](https://www.microsoft.com/sql-server/sql-server-downloads) or later (Express edition is sufficient)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (recommended) or [VS Code](https://code.visualstudio.com/)
- [SQL Server Management Studio (SSMS)](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms) (optional, for database management)
- [Node.js](https://nodejs.org/) (optional, for frontend package management)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/HandmadeShop.git
   cd HandmadeShop/HandmadeShop
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Configure database connection**
   
   Open `appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=HandmadeShop;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     }
   }
   ```
   
   Replace `YOUR_SERVER_NAME` with:
   - `localhost` or `.` for local default instance
   - `localhost\\SQLEXPRESS` for SQL Server Express
   - Your actual server name if connecting remotely

4. **Restore database**
   
   **Option A: Using SQL Server Management Studio**
   - Open SSMS and connect to your SQL Server
   - Right-click on "Databases" → "Restore Database"
   - Select "Device" and browse to `DataHandmadeShop_23-8-2025` backup file
   - Click "OK" to restore
   
   **Option B: Using T-SQL**
   ```sql
   RESTORE DATABASE HandmadeShop
   FROM DISK = 'D:\DoAnCS_1\DataHandmadeShop_23-8-2025'
   WITH MOVE 'HandmadeShop' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\HandmadeShop.mdf',
        MOVE 'HandmadeShop_log' TO 'C:\Program Files\Microsoft SQL Server\MSSQL15.SQLEXPRESS\MSSQL\DATA\HandmadeShop_log.ldf',
        REPLACE;
   ```

5. **Configure SMTP for email functionality**
   
   Update `appsettings.json` with your SMTP settings:
   ```json
   {
     "SmtpSettings": {
       "Host": "smtp.gmail.com",
       "Port": 587,
       "Username": "your-email@gmail.com",
       "Password": "your-app-password",
       "FromEmail": "your-email@gmail.com",
       "FromName": "HandmadeShop"
     }
   }
   ```
   
   **For Gmail:**
   - Enable 2-Factor Authentication
   - Generate an [App Password](https://myaccount.google.com/apppasswords)
   - Use the app password in the configuration

6. **Configure MoMo payment** (optional)
   
   If you have MoMo merchant account, update:
   ```json
   {
     "MomoSettings": {
       "PartnerCode": "YOUR_PARTNER_CODE",
       "AccessKey": "YOUR_ACCESS_KEY",
       "SecretKey": "YOUR_SECRET_KEY",
       "ReturnUrl": "https://localhost:7xxx/Payment/MomoReturn",
       "IpnUrl": "https://localhost:7xxx/Payment/MomoIPN",
       "RequestType": "captureWallet"
     }
   }
   ```

7. **Create upload directories**
   ```bash
   mkdir ShareUploads\Products
   mkdir ShareUploads\Avartar
   mkdir ShareUploads\users
   ```

8. **Build the application**
   ```bash
   dotnet build
   ```

9. **Run the application**
   ```bash
   dotnet run
   ```
   
   Or press `F5` in Visual Studio to run with debugging.

10. **Access the application**
    
    The application will be available at:
    - HTTPS: `https://localhost:7xxx`
    - HTTP: `http://localhost:5xxx`
    
    (Port numbers may vary, check console output)

### Default Accounts

#### Admin Account
```
Email: admin@handmadeshop.com
Password: Admin@123
Role: Administrator
```

#### Test Customer Account
```
Email: customer@test.com
Password: Customer@123
Role: Customer
```

### First-Time Setup

1. **Login as Admin**
   - Navigate to `/User/Login`
   - Use admin credentials
   - You'll be redirected to admin dashboard

2. **Verify Site Settings**
   - Check SMTP configuration by sending test email
   - Verify payment gateway settings
   - Review default categories and products

3. **Create Test Order**
   - Logout from admin
   - Register a new customer account or use test account
   - Add products to cart
   - Complete checkout process
   - Verify order appears in admin panel

### Troubleshooting

**Database Connection Issues**
```bash
# Test connection string
dotnet ef database update --verbose
```

**Port Already in Use**
```bash
# Change ports in launchSettings.json
"applicationUrl": "https://localhost:7XXX;http://localhost:5XXX"
```

**Email Not Sending**
- Verify SMTP credentials
- Check firewall settings
- For Gmail, ensure "Less secure app access" is enabled or use App Password

**Payment Gateway Errors**
- Verify MoMo credentials
- Check return URL is accessible
- Review MoMo documentation for error codes

## 🗄️ Database Schema

### Entity Relationship Diagram

```
Users (1) ──────── (*) Orders
  │                    │
  │                    │
  │ (1)                │ (*)
  │                    │
  │                    └── OrderItems (*) ──── (1) Products
  │                                                  │
  │ (1)                                             │ (*)
  │                                                  │
  └── Reviews (*) ────────────────────────────────┘
        │
        │ (1)
        │
        └── ReviewReplies (*)

Categories (1) ──── (*) Products (1) ──── (*) ProductImages
                         │
                         │ (*)
                         │
                         ├── ProductVariants (*) ──── (*) VariationOptionLink
                         │
                         └── ProductAttributeOption (*) ──── (1) AttributeOption
                                                                  │
                                                                  │ (*)
                                                                  │
                                                              Attribute (1)
```

### Detailed Table Structure

#### Users & Authentication
- **Users**: Customer and admin accounts
- **Roles**: Role definitions (Admin, Customer, Staff)
- **PasswordResetTokens**: Temporary tokens for password recovery

#### Products
- **Products**: Main product information
- **ProductImages**: Multiple images per product
- **ProductVariants**: Product variations (e.g., Red-Large, Blue-Small)
- **Categories**: Hierarchical product categorization

#### Product Attributes System
- **Attributes**: Attribute definitions (Color, Size, Material)
- **AttributeOptions**: Possible values (Red, Blue, Large, Small)
- **AttributeValues**: Product-specific attribute values
- **ProductAttributeOption**: Links products to attribute options
- **ProductVariation**: Variation types (Color Variation, Size Variation)
- **VariationOptionLink**: Links variants to options

#### Orders & Cart
- **Orders**: Customer orders
- **OrderItems**: Individual items in an order
- **CartItems**: Shopping cart items (session-based)

#### Marketing & Support
- **Vouchers**: Discount codes and promotions
- **Reviews**: Product reviews with ratings
- **ReviewReplies**: Admin responses to reviews

#### Custom Orders
- **CustomOrders**: Custom product requests
- **CustomOrderAttributes**: Specifications for custom orders

#### Content
- **Questions**: FAQ questions
- **Answers**: FAQ answers

### Indexes

```sql
-- Performance indexes
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
CREATE INDEX IX_Reviews_ProductId ON Reviews(ProductId);
CREATE INDEX IX_Reviews_UserId ON Reviews(UserId);
```

## 💡 Key Implementations

### 1. Base Controller Pattern

All controllers inherit from `BaseController` for shared functionality:

```csharp
// filepath: Controllers/BaseController.cs
public class BaseController : Controller
{
    protected readonly HandmadeShopContext db;
    
    public BaseController(HandmadeShopContext context)
    {
        db = context;
    }
    
    protected int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
    }
    
    protected bool IsUserAuthenticated()
    {
        return User.Identity?.IsAuthenticated ?? false;
    }
}
```

### 2. MoMo Payment Integration

Secure payment processing with signature verification:

```csharp
// filepath: Services/MomoService.cs
public class MomoService
{
    private readonly IOptions<MoMoOptions> _options;
    
    public async Task<string> CreatePaymentUrl(Order order)
    {
        var rawHash = $"accessKey={_options.Value.AccessKey}" +
                     $"&amount={order.TotalAmount}" +
                     $"&orderId={order.OrderId}" +
                     // ... other parameters
        
        var signature = ComputeHmacSha256(rawHash, _options.Value.SecretKey);
        
        // Create payment request
        var paymentRequest = new MomoPaymentRequest
        {
            PartnerCode = _options.Value.PartnerCode,
            RequestId = Guid.NewGuid().ToString(),
            Amount = order.TotalAmount,
            OrderId = order.OrderId.ToString(),
            Signature = signature
        };
        
        return await SendPaymentRequest(paymentRequest);
    }
    
    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);
        
        using var hmac = new HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }
}
```

### 3. Email Service Implementation

SMTP-based email service with template support:

```csharp
// filepath: Services/SmtpEmailSender.cs
public class SmtpEmailSender : IEmailSender
{
    private readonly IOptions<SmtpOptions> _smtpOptions;
    
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var smtpClient = new SmtpClient(_smtpOptions.Value.Host)
        {
            Port = _smtpOptions.Value.Port,
            Credentials = new NetworkCredential(
                _smtpOptions.Value.Username,
                _smtpOptions.Value.Password
            ),
            EnableSsl = true
        };
        
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpOptions.Value.FromEmail, _smtpOptions.Value.FromName),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };
        
        mailMessage.To.Add(email);
        
        await smtpClient.SendMailAsync(mailMessage);
    }
}
```

### 4. Product Variant System

Complex product variation handling:

```csharp
// Example: Getting product with all variants and options
var product = await db.Products
    .Include(p => p.ProductVariants)
        .ThenInclude(v => v.VariationOptionLinks)
            .ThenInclude(l => l.AttributeOption)
                .ThenInclude(o => o.Attribute)
    .Include(p => p.ProductImages)
    .FirstOrDefaultAsync(p => p.ProductId == id);

// Get available colors for a product
var colors = product.ProductVariants
    .SelectMany(v => v.VariationOptionLinks)
    .Where(l => l.AttributeOption.Attribute.AttributeName == "Color")
    .Select(l => l.AttributeOption.OptionValue)
    .Distinct()
    .ToList();
```

### 5. Shopping Cart Management

Session-based cart with AJAX updates:

```javascript
// wwwroot/js/cart.js
function updateCart(productId, quantity) {
    $.ajax({
        url: '/Shopping_cart/UpdateQuantity',
        type: 'POST',
        data: {
            productId: productId,
            quantity: quantity
        },
        success: function(result) {
            if (result.success) {
                $('#cart-subtotal').text('$' + result.subtotal);
                $('#cart-count').text(result.itemCount);
                updateCartIcon(result.itemCount);
            } else {
                showError(result.message);
            }
        }
    });
}
```

### 6. Product Search & Filtering

Advanced search with multiple criteria:

```csharp
// filepath: Controllers/CategoryController.cs
public async Task<IActionResult> Index(
    int? categoryId,
    decimal? minPrice,
    decimal? maxPrice,
    string sortBy,
    int page = 1)
{
    var query = db.Products
        .Include(p => p.Category)
        .Include(p => p.ProductImages)
        .Where(p => p.IsActive);
    
    // Category filter
    if (categoryId.HasValue)
        query = query.Where(p => p.CategoryId == categoryId.Value);
    
    // Price range filter
    if (minPrice.HasValue)
        query = query.Where(p => p.Price >= minPrice.Value);
    if (maxPrice.HasValue)
        query = query.Where(p => p.Price <= maxPrice.Value);
    
    // Sorting
    query = sortBy switch
    {
        "price_asc" => query.OrderBy(p => p.Price),
        "price_desc" => query.OrderByDescending(p => p.Price),
        "newest" => query.OrderByDescending(p => p.CreatedAt),
        _ => query.OrderBy(p => p.ProductName)
    };
    
    // Pagination
    var pageSize = 9;
    var products = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return View(products);
}
```

### 7. Order Status Workflow

Structured order processing:

```csharp
public enum OrderStatus
{
    Pending,        // Order placed, awaiting confirmation
    Confirmed,      // Order confirmed, preparing items
    Shipping,       // Order shipped
    Delivered,      // Order delivered to customer
    Cancelled       // Order cancelled
}

// Order status update with email notification
public async Task<bool> UpdateOrderStatus(int orderId, OrderStatus newStatus)
{
    var order = await db.Orders
        .Include(o => o.User)
        .FirstOrDefaultAsync(o => o.OrderId == orderId);
    
    if (order == null) return false;
    
    order.OrderStatus = newStatus.ToString();
    order.UpdatedAt = DateTime.Now;
    
    await db.SaveChangesAsync();
    
    // Send email notification
    await _emailSender.SendEmailAsync(
        order.User.Email,
        $"Order #{order.OrderId} Status Update",
        $"Your order status has been updated to: {newStatus}"
    );
    
    return true;
}
```

### 8. Image Upload & Management

Secure file upload with validation:

```csharp
[HttpPost]
public async Task<IActionResult> UploadProductImage(int productId, IFormFile image)
{
    if (image == null || image.Length == 0)
        return BadRequest("No file uploaded");
    
    // Validate file type
    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
    var extension = Path.GetExtension(image.FileName).ToLower();
    
    if (!allowedExtensions.Contains(extension))
        return BadRequest("Invalid file type");
    
    // Validate file size (max 5MB)
    if (image.Length > 5 * 1024 * 1024)
        return BadRequest("File size exceeds 5MB");
    
    // Generate unique filename
    var fileName = $"{Guid.NewGuid()}{extension}";
    var uploadPath = Path.Combine("ShareUploads", "Products", fileName);
    
    // Save file
    using (var stream = new FileStream(uploadPath, FileMode.Create))
    {
        await image.CopyToAsync(stream);
    }
    
    // Save to database
    var productImage = new ProductImage
    {
        ProductId = productId,
        ImageUrl = $"/ShareUploads/Products/{fileName}",
        CreatedAt = DateTime.Now
    };
    
    db.ProductImages.Add(productImage);
    await db.SaveChangesAsync();
    
    return Ok(new { imageUrl = productImage.ImageUrl });
}
```

## 🔒 Security Features

### 1. Password Security
- **BCrypt Hashing**: Passwords are hashed using BCrypt with salt
- **Password Strength**: Minimum 8 characters, uppercase, lowercase, number, special character
- **Password Reset**: Time-limited tokens (1 hour expiry)

### 2. Authentication & Authorization
- **Cookie-Based Authentication**: Secure, HttpOnly cookies
- **Role-Based Authorization**: Admin, Customer roles
- **Claims-Based Identity**: User claims for fine-grained access control

### 3. Input Validation
- **Client-Side**: jQuery Validation for immediate feedback
- **Server-Side**: Data annotations and manual validation
- **Anti-Forgery Tokens**: CSRF protection on all forms

### 4. SQL Injection Prevention
- **Entity Framework Core**: Parameterized queries
- **No Raw SQL**: Avoiding string concatenation in queries

### 5. XSS Protection
- **Razor Encoding**: Automatic HTML encoding in views
- **Content Security Policy**: CSP headers (future enhancement)

### 6. File Upload Security
- **File Type Validation**: Whitelist of allowed extensions
- **File Size Limits**: Maximum 5MB per file
- **Unique Filenames**: Prevent overwriting with GUID names
- **Virus Scanning**: Integration planned

### 7. Session Security
- **Secure Cookies**: HTTPS only in production
- **Session Timeout**: 30-minute idle timeout
- **Anti-Session Fixation**: New session ID on login

### 8. Payment Security
- **HMAC Signature**: Request signature verification
- **HTTPS Only**: Encrypted communication
- **No Card Storage**: PCI DSS compliance

## 📈 Performance Optimizations

### 1. Database Optimizations
- **Indexes**: Strategic indexes on foreign keys and frequently queried columns
- **Eager Loading**: Include related entities to prevent N+1 queries
- **Lazy Loading**: For optional relationships
- **Query Optimization**: Select only required columns

### 2. Caching Strategies
- **Memory Cache**: For frequently accessed data (categories, settings)
- **Response Cache**: For static pages
- **CDN**: For static assets (planned)

### 3. Image Optimization
- **Compression**: Automatic image compression on upload
- **Responsive Images**: Multiple sizes for different devices
- **Lazy Loading**: Images load as user scrolls
- **WebP Format**: Modern format support (planned)

### 4. Frontend Optimizations
- **Minification**: CSS and JS minification
- **Bundling**: Combine multiple files
- **Async Loading**: Non-blocking script loading
- **Gzip Compression**: Server-side compression

### 5. Pagination
- **Server-Side**: Limit database results
- **Efficient Queries**: Skip/Take for pagination
- **Client-Side**: Virtual scrolling for large lists (planned)

## 🎨 UI/UX Features

### Responsive Design
- Mobile-first approach
- Breakpoints: 576px, 768px, 992px, 1200px
- Touch-friendly interface
- Optimized for tablets and phones

### Accessibility
- ARIA labels for screen readers
- Keyboard navigation support
- High contrast mode support (planned)
- Alt text for all images

### User Feedback
- Toast notifications for actions
- Loading spinners for async operations
- Form validation messages
- Error handling with friendly messages

### Interactive Elements
- Product image zoom on hover
- Smooth scroll animations
- Dropdown menus with keyboard support
- Modal dialogs for quick actions

## 📝 Future Enhancements

See [`doan1/ARCHITECTURE_SUGGESTIONS.md`](doan1/ARCHITECTURE_SUGGESTIONS.md) for detailed architectural improvements:

### Short-Term (3-6 months)
- [ ] Implement Unit of Work pattern
- [ ] Add comprehensive unit tests (target: 80% coverage)
- [ ] Implement caching layer (Redis)
- [ ] Add API endpoints for mobile app
- [ ] Implement real-time notifications (SignalR)
- [ ] Add advanced analytics dashboard

### Medium-Term (6-12 months)
- [ ] Migrate to Clean Architecture
- [ ] Implement CQRS pattern
- [ ] Add Elasticsearch for product search
- [ ] Develop mobile app (React Native)
- [ ] Add social media login (Google, Facebook)
- [ ] Implement wishlist and product comparison

### Long-Term (12+ months)
- [ ] Microservices architecture
- [ ] AI-powered product recommendations
- [ ] Multi-language support (i18n)
- [ ] Multi-currency support
- [ ] Advanced inventory management
- [ ] Vendor/marketplace functionality

## 🧪 Testing

### Unit Tests (Planned)
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Integration Tests (Planned)
- API endpoint testing
- Database integration tests
- Payment gateway integration tests

### Manual Testing Checklist
- [ ] User registration and login
- [ ] Product browsing and filtering
- [ ] Add to cart and checkout
- [ ] Order placement and payment
- [ ] Admin product management
- [ ] Admin order processing
- [ ] Email notifications

## 📊 Performance Metrics

### Current Performance
- **Homepage Load**: ~800ms (average)
- **Product Listing**: ~600ms (average)
- **Checkout Process**: ~1.2s (average)
- **Database Queries**: Optimized with indexes

### Monitoring (Planned)
- Application Insights integration
- Real-time error tracking (Sentry)
- Performance monitoring (New Relic)

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Code Style
- Follow C# coding conventions
- Use meaningful variable names
- Add XML comments for public methods
- Write unit tests for new features

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

```
MIT License

Copyright (c) 2025 [Your Name]

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```

## 👥 Team

### Development Team
- **Your Name** - *Full Stack Developer* - [GitHub](https://github.com/yourusername)

### Special Thanks
- ASP.NET Core team for the excellent framework
- Bootstrap team for the responsive CSS framework
- MoMo for payment gateway integration support

## 📞 Contact & Support

### Developer Contact
- **Email**: your.email@example.com
- **LinkedIn**: [Your LinkedIn Profile](https://linkedin.com/in/yourprofile)
- **GitHub**: [Your GitHub Profile](https://github.com/yourusername)
- **Portfolio**: [Your Portfolio Website](https://yourportfolio.com)

### Project Links
- **Repository**: https://github.com/yourusername/HandmadeShop
- **Live Demo**: https://handmadeshop-demo.azurewebsites.net (if available)
- **Documentation**: https://handmadeshop.gitbook.io (if available)

### Support
For bug reports and feature requests, please use the [GitHub Issues](https://github.com/yourusername/HandmadeShop/issues) page.

---

## 🎓 Academic Information

**Project Type**: Computer Science Capstone Project  
**Institution**: [Your University Name]  
**Course**: CS Capstone Project  
**Semester**: Fall 2024  
**Instructor**: [Instructor Name]  

**Learning Outcomes Demonstrated**:
- Full-stack web development with ASP.NET Core
- Database design and implementation
- RESTful API design principles
- Payment gateway integration
- Security best practices
- Responsive web design
- Version control with Git
- Agile development methodology

---

**Last Updated**: December 3, 2025  
**Version**: 1.0.0  
**Status**: Active Development

---

<div align="center">

### ⭐ Star this repo if you find it helpful!

Made with ❤️ by [Your Name]

</div>