using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using PromoRandom.Services;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession();

// HttpClient � ���� ������
builder.Services.AddHttpClient();
builder.Services.AddTransient<WinnerNotificationService>();

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

// �����: ����������� ����� ������������
var localizationOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(localizationOptions);

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
