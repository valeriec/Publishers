using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configurar localización en español
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("es-ES") };
    options.DefaultRequestCulture = new RequestCulture("es-ES");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configuración de AuthService y HttpClient
var api1BaseUrl = builder.Configuration["Api1BaseUrl"] ?? "https://localhost:7001";
builder.Services.AddHttpClient<WebApp.Services.AuthService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true }
)
.AddTypedClient((httpClient, sp) => new WebApp.Services.AuthService(httpClient, api1BaseUrl));

// Configuración de ArticleService y HttpClient
var api2BaseUrl = builder.Configuration["Api2BaseUrl"] ?? "https://localhost:7002";
builder.Services.AddHttpClient<WebApp.Services.ArticleService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
})
.ConfigurePrimaryHttpMessageHandler(() =>
    new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true }
)
.AddTypedClient((httpClient, sp) => new WebApp.Services.ArticleService(httpClient, api2BaseUrl));

var app = builder.Build();

app.UseSession();

// Usar localización
app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();
app.MapRazorPages();

app.Run();
