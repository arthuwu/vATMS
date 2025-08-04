using ElectronNET.API;
using ElectronNET.API.Entities;
using vATMS.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddElectron();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

builder.WebHost.UseElectron(args);


// if (HybridSupport.IsElectronActive)
//     await CreateElectronWindow();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.MapHub<TargetGeneratorHub>("/api/hub");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}"
);

app.Use(async (context, next) =>
{
    Console.WriteLine($"Request: {context.Request.Path}");
    await next();
});

app.MapFallbackToFile("index.html");

await app.StartAsync();

if (HybridSupport.IsElectronActive)
    await CreateElectronWindow();

await app.WaitForShutdownAsync();


async Task CreateElectronWindow()
{
    BrowserWindow window = await Electron.WindowManager.CreateWindowAsync(
        new BrowserWindowOptions {
            Width = 1280,
            Height = 720,
            AutoHideMenuBar = true,
        WebPreferences = new WebPreferences {
                NodeIntegration = false, // force node off, otherwise electron won't be able to use eventsource, which breaks signalR entirely
                ContextIsolation = true, 
            }
        });

    window.OnClosed += () => Electron.App.Quit();
}
