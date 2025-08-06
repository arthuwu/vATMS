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

    public RadarScreen(IHubContext<RadarScreenHub> hubContext)
    {
        _hubContext = hubContext;
        Console.WriteLine("DEBUG: new RadarScreen instance");
    }

}