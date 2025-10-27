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
    public string ConnectionId { get; init; }

    public RadarScreen(IHubContext<RadarScreenHub> hubContext, string connectionId)
    {
        _hubContext = hubContext;
        ConnectionId = connectionId;

        Console.WriteLine(connectionId);
        Console.WriteLine("DEBUG: new RadarScreen instance");

        SetupMaps();
    }

    public void SetupMaps()
    {
        string sectorMaps = File.ReadAllText("Maps\\EG_LAC_PC_SECTOR.geojson");
        _hubContext.Clients.Client(ConnectionId).SendAsync("SetupMap", sectorMaps);
    }

}