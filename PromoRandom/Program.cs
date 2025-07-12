using Microsoft.AspNetCore.Localization;
using PromoRandom.Services;
using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using PromoRandom.Models; // ← обязательно для IOptions<>

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();
builder.Services.AddSingleton<DatabaseService>();

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Регистрируем привязку настроек для AdminUser (загружается из appsettings.json)
builder.Services.Configure<AdminUserOptions>(builder.Configuration.GetSection("AdminUser"));

builder.Services.AddHttpClient<WinnerNotificationService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Index";
        options.SlidingExpiration = true;
    });

// ✅ Локализация
builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

var supportedCultures = new[]
{
    new CultureInfo("ru"),
    new CultureInfo("tg")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("tg");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = [
        new CookieRequestCultureProvider(),
        new QueryStringRequestCultureProvider(),
    ];

});

var app = builder.Build();

// ✅ Включаем локализацию (ты забыл это!)
var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Site}/{action=Index}/{id?}");

app.Run();