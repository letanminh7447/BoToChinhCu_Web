using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using WebBoTo.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
    })
    .AddGoogle(options =>
    {
        // Khuyên dùng: Nên lưu 2 mã này trong appsettings.json cho bảo mật
        options.ClientId = "952715307773-p6pml0tsh8ufsr8r5haeecrmb9fq10gj.apps.googleusercontent.com";
        options.ClientSecret = "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";
    });

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();