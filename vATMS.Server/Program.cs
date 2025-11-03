using ElectronNET.API;
using ElectronNET.API.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
using vATMS.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddElectron();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.WriteIndented = true;
        options.PayloadSerializerOptions.IncludeFields = true;
    })
;

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

builder.Services.AddSingleton<RadarScreenManager>(); // Creates a new instance
builder.Services.AddSingleton<SimulationManager>(); // Creates a new instance
builder.Services.AddSingleton<RadarScreenUpdater>(); // Creates a new instance
builder.Services.AddSingleton<NavigationData>(); // Creates a new instance
builder.Services.AddSingleton<ConflictDetection>(); // Creates a new instance
builder.Services.AddScoped<RadarScreen>();
builder.WebHost.UseElectron(args);


// if (HybridSupport.IsElectronActive)
//     await CreateElectronWindow();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseCors();

app.MapHub<RadarScreenHub>("/api/hub");

// This is a single API endpoint for the main menu. The main menu is NOT SignalR (that will start the simulation automatically, which we don't want)
app.MapGet("/api/get-available-sectors", () =>
{
    using (var fs = new FileStream(Path.Combine(builder.Environment.WebRootPath, "Sector\\SectorLogon.json"), FileMode.Open, FileAccess.Read, FileShare.Read))
    using (var sr = new StreamReader(fs))
    {
        string sectorJson = sr.ReadToEnd();
        return sectorJson;
    }
});

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
{
    await CreateElectronWindow();
}

await app.WaitForShutdownAsync();


async Task CreateElectronWindow()
{
    BrowserWindow window = await Electron.WindowManager.CreateWindowAsync(
        new BrowserWindowOptions
        {
            Width = 1280,
            Height = 720,
            AutoHideMenuBar = true,
            //Frame = false,
            WebPreferences = new WebPreferences
            {
                NodeIntegration = false, // force node off, otherwise electron won't be able to use eventsource, which breaks signalR entirely
                ContextIsolation = true,
            }
        });

    window.OnClosed += () => Electron.App.Quit();
}