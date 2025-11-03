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

    public async void SetupMaps()
    {
        using (var fs1 = new FileStream(Path.Combine(_webEnv.WebRootPath, "Maps\\EG_LAC_PC_SECTOR.geojson"), FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sr1 = new StreamReader(fs1))
        {
            string sectorMaps = sr1.ReadToEnd();
            await _hubContext.Clients.Client(ConnectionId).SendAsync("SetupSectorMap", sectorMaps);
            sr1.Close();
            fs1.Close();
        }

        using (var fs2 = new FileStream(Path.Combine(_webEnv.WebRootPath, "Maps\\CAS_background.geojson"), FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var sr2 = new StreamReader(fs2))
        {
            string baseMaps = sr2.ReadToEnd();
            await _hubContext.Clients.Client(ConnectionId).SendAsync("SetupBaseMap", baseMaps);
            sr2.Close();
            fs2.Close();
        }
    }
}