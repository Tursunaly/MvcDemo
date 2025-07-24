using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Net;
using MvcAuthApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Настройка прокси для всех HTTP-запросов приложения
WebRequest.DefaultWebProxy = new WebProxy("http://proxy.halykbank.kg:8080", true)
{
    Credentials = CredentialCache.DefaultNetworkCredentials,
    BypassProxyOnLocal = true
};

// Подключение PostgreSQL через Npgsql
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка Data Protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\aspnet-keys"))
    .SetApplicationName("MvcAuthApp");

// Кэш и сессии
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

// HttpClient с прокси
builder.Services.AddHttpClient("ProxyClient").ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler
    {
        Proxy = WebRequest.DefaultWebProxy,
        UseProxy = true
    });

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Включаем сессии
app.UseSession();

// Настройка IP и порта
app.Urls.Clear();
app.Urls.Add("http://10.128.130.226:5000");

// Редирект с "/" на "/Account/Auth"
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.Redirect("/Account/Auth");
        return;
    }
    await next();
});

app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
