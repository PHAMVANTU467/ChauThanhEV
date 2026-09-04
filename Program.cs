using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Hỗ trợ dynamic PORT từ Railway / PaaS nếu có
var envPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(envPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{envPort}");
}

// Tắt ReloadOnChange để tránh lỗi inotify file watcher trên môi trường Linux Container (Render/Railway)
foreach (var source in builder.Configuration.Sources.OfType<Microsoft.Extensions.Configuration.FileConfigurationSource>())
{
    source.ReloadOnChange = false;
}

builder.Services.AddControllersWithViews();

// Nguồn dữ liệu mock DUY NHẤT dùng chung cho toàn bộ hệ thống (Dashboard + các trang quản lý).
builder.Services.AddSingleton<MockDataService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.Name = "ChauThanhEV.Auth";
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        var fileName = ctx.File.Name.ToLowerInvariant();
        if (fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg") ||
            fileName.EndsWith(".svg") || fileName.EndsWith(".webp") || fileName.EndsWith(".ico") ||
            fileName.EndsWith(".woff") || fileName.EndsWith(".woff2"))
        {
            // Cho phép browser và CDN cache tài nguyên ảnh, font (1 năm)
            // Nhờ asp-append-version, khi file thay đổi query ?v=... sẽ tự động cập nhật ngay
            ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
        }
        else
        {
            // Với CSS/JS, dùng no-cache để trình duyệt kiểm tra ETag 304 Not Modified thay vì tải lại toàn bộ
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
        }
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
