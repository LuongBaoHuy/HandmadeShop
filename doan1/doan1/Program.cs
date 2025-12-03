using Microsoft.EntityFrameworkCore;
using doan1.Data;
using doan1.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework
builder.Services.AddDbContext<HandmadeShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add File Upload Service
builder.Services.AddScoped<IFileUploadService, FileUploadService>();

// Persist DataProtection keys so cookie không bị vô hiệu khi app restart / migrate DB
var keysPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtectionKeys");
Directory.CreateDirectory(keysPath);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("HandmadeShopApp");

// Add Authentication (kéo dài thời gian sống cookie, đặt tên riêng, đảm bảo không mất sau restart)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = ".HandmadeShop.Auth"; // tên cookie cố định
        options.ExpireTimeSpan = TimeSpan.FromDays(30); // thời gian đăng nhập dài hơn
        options.SlidingExpiration = true; // tự động gia hạn khi hoạt động
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("AdminOrManager", policy => policy.RequireRole("Admin", "Manager"));
});

var app = builder.Build();

// Map /uploads -> thư mục ShareUploads ngoài project
var handmadeRoot = Directory.GetParent(builder.Environment.ContentRootPath)!.Parent!.FullName;
var sharedRoot = Path.Combine(handmadeRoot, "ShareUploads");
if (Directory.Exists(sharedRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(sharedRoot),
        RequestPath = "/uploads"
    });
}

// Map wwwroot mặc định
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
