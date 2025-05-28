<<<<<<< HEAD
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using PromoRandom.Services;
using System.Globalization;
=======
﻿using PromoRandom.Services;
>>>>>>> origin/main

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

<<<<<<< HEAD
// HttpClient � ���� ������
builder.Services.AddHttpClient();
builder.Services.AddTransient<WinnerNotificationService>();
=======
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// ✅ Регистрируем WinnerNotificationService через AddHttpClient
builder.Services.AddHttpClient<WinnerNotificationService>();

builder.Services.AddAuthentication("MyCookieAuth")
    .AddCookie("MyCookieAuth", options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "MyAppAuthCookie";
        options.SlidingExpiration = false;
    });

builder.Services.AddAuthorization();
>>>>>>> origin/main

// �����������
builder.Services.AddLocalization(opts => opts.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization()
    .AddDataAnnotationsLocalization();

// �������������� ��������
var supportedCultures = new[]
{
    new CultureInfo("ru"),
    new CultureInfo("tg")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("tg"); // �� ��������� � ����������
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;

    // ������� ��������� ����� ����� ����� ?culture=
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

<<<<<<< HEAD
// �����: ����������� ����� ������������
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

=======
>>>>>>> origin/main
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
