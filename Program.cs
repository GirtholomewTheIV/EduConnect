using EduConnect.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<EduConnectDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("EduConnect")));

builder.Services.AddAuthentication("EduConnectCookie")
    .AddCookie("EduConnectCookie", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Home/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    EduConnectDbContext db = scope.ServiceProvider.GetRequiredService<EduConnectDbContext>();
    db.Database.EnsureCreated();

    // Demo accounts with known passwords must never be created outside development.
    if (app.Environment.IsDevelopment())
    {
        DbSeeder.Seed(db);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
