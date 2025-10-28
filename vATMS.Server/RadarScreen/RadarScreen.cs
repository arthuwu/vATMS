// This is the "point of connection" with React - for ONE client

using Microsoft.AspNetCore.SignalR;
using vATMS.Hubs;

/// <summary>
/// A new RadarScreen instance is created for every SignalR connection
/// It is reponsible for all computation related to rendering things on the radar screen
/// </summary>
public class RadarScreen
{
    private readonly IHubContext<RadarScreenHub> _hubContext;
    private readonly IWebHostEnvironment _webEnv;
    public string ConnectionId { get; init; }

    public RadarScreen(IHubContext<RadarScreenHub> hubContext, IWebHostEnvironment webEnv, string connectionId)
    {
        _hubContext = hubContext;
        _webEnv = webEnv;
        ConnectionId = connectionId;

        Console.WriteLine(connectionId);
        Console.WriteLine("DEBUG: new RadarScreen instance");

        SetupMaps();
    }

    public void SetupMaps()
    {
        using (var fs = new FileStream(Path.Combine(_webEnv.WebRootPath, "Maps\\EG_LAC_PC_SECTOR.geojson"), FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sr = new StreamReader(fs))
        {
            string sectorMaps = sr.ReadToEnd();
            _hubContext.Clients.Client(ConnectionId).SendAsync("SetupMap", sectorMaps);
        }
    }
}