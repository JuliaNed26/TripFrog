using TripFrogMVC.Middleware;
using TripFrogMVC.Services;
using TripFrogMVC.Services.WebApiClients;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddMemoryCache();

string webApiUrl = builder.Configuration.GetSection("WebApiUrls:Https").Value!;
builder.Services.AddSingleton(_ => new WebApiInfoService(webApiUrl));
builder.Services.AddSingleton<BlobsApiClient>();
builder.Services.AddSingleton<RefreshTokenApiClient>();
builder.Services.AddSingleton<MemoryCacheManagerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseMiddleware<TokenExpirationCheckMiddleware>();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();



