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

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

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

var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".apk"] = "application/vnd.android.package-archive";
contentTypeProvider.Mappings[".json"] = "application/json";

app.UseDefaultFiles();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider,
    OnPrepareResponse = ctx =>
    {
        var fileName = ctx.File.Name.ToLowerInvariant();
        if (fileName.EndsWith(".apk"))
        {
            ctx.Context.Response.Headers["Content-Disposition"] = "attachment; filename=\"ChauThanhEV.apk\"";
        }
        else if (fileName.EndsWith(".png") || fileName.EndsWith(".jpg") || fileName.EndsWith(".jpeg") ||
            fileName.EndsWith(".svg") || fileName.EndsWith(".webp") || fileName.EndsWith(".ico") ||
            fileName.EndsWith(".woff") || fileName.EndsWith(".woff2"))
        {
            ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=31536000, immutable";
        }
        else
        {
            ctx.Context.Response.Headers["Cache-Control"] = "no-cache";
            ctx.Context.Response.Headers["Pragma"] = "no-cache";
        }
    }
});

app.UseCors();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
