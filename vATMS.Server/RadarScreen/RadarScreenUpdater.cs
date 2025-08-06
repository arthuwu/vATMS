// This is the "point of connection" with React - for ALL clients

using Microsoft.AspNetCore.SignalR;
using vATMS.Aircraft;
using vATMS.Hubs;

/// <summary>
/// A new RadarScreen instance is created for every SignalR connection
/// It is reponsible for all computation related to rendering things on the radar screen
/// </summary>
public class RadarScreenUpdater
{
    private readonly IHubContext<RadarScreenHub> _hubContext;

    public RadarScreenUpdater(IHubContext<RadarScreenHub> hubContext)
    {
        _hubContext = hubContext;
        Console.WriteLine("DEBUG: new RadarScreenUpdater instance");
    }

    public async Task UpdateAircraftPosition(Aircraft ac)
    {
        await _hubContext.Clients.All.SendAsync("AircraftPositionUpdate", ac.FlightPlan.Callsign, ac.Position.Lat, ac.Position.Lon); // maybe use an export-only Aircraft type?
    }
}