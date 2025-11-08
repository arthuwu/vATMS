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

    public async Task UpdateAircraftPosition(List<Aircraft> aircraftList)
    {
        //Console.WriteLine("updateAircraftPosition called");
        await _hubContext.Clients.All.SendAsync("AircraftPositionUpdate", aircraftList); // maybe use an export-only Aircraft type?
    }

    public async Task UpdateConflictPairs(List<PredictedConflictPair> conflictList)
    {
        //Console.WriteLine("updateConflictPairs called");
        await _hubContext.Clients.All.SendAsync("ConflictPairsUpdate", conflictList);
    }
}