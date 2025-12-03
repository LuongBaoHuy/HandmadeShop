using HandmadeShop.Models;
using HandmadeShop.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

using Microsoft.Extensions.FileProviders;  
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Kết nối SQL Entity Famework: Scaffold-DbContext "Data Source=BaoHuy;Initial Catalog=HandmadeShop;Integrated Security=True;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models

// Cập nhật SQL Entity Famework: Scaffold-DbContext "Data Source=BaoHuy;Initial Catalog=HandmadeShop;Integrated Security=True;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Models -Force

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

builder.Services.AddDbContext<HandmadeShopContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Sign_in/Sign_in";
        options.LogoutPath = "/Sign_in/Logout";
        options.AccessDeniedPath = "/Sign_in/Sign_in";
    });

// Đăng ký options và HttpClient
builder.Services.Configure<MomoOptions>(builder.Configuration.GetSection("Momo"));
builder.Services.AddHttpClient();
builder.Services.AddScoped<MomoService>();
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Cấu hình cho phép truy cập ảnh trong ShareUploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "..", "ShareUploads")),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
