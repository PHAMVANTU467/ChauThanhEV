using ChauThanhEV.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Tắt ReloadOnChange để tránh lỗi inotify file watcher trên môi trường Linux Container (Render)
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

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Yêu cầu trình duyệt luôn kiểm tra phiên bản mới của file tĩnh (CSS/JS)
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        ctx.Context.Response.Headers.Append("Pragma", "no-cache");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
