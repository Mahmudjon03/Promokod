using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using PromoRandom.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// HttpClient и твой сервис
builder.Services.AddHttpClient();
builder.Services.AddTransient<WinnerNotificationService>();

// Локализация
builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// Поддерживаемые культуры
var supportedCultures = new[]
{
    new CultureInfo("ru"),
    new CultureInfo("tg")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("tg"); // По умолчанию — таджикский
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // добавим поддержку смены языка через ?culture=
    options.RequestCultureProviders =
    [
        new QueryStringRequestCultureProvider()
    ];
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ВАЖНО: локализация перед авторизацией
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
