// This is the "point of connection" with React - for ALL clients INBOUND

using Microsoft.AspNetCore.SignalR;

namespace vATMS.Hubs;

/// <summary>
/// RadarScreenHub inherits the base SignalR hub to provide specific functionality
/// </summary>
public class RadarScreenHub : Hub
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RadarScreenManager _radarScreenManager;
    private readonly SimulationManager _simulationManager;

    /// <summary>
    /// Constructor of the RadarScreenHub class. Initialises the service provider and the screen manager
    /// </summary>
    /// <param name="serviceProvider"></param>
    public RadarScreenHub(IServiceProvider serviceProvider, RadarScreenManager radarScreenManager, SimulationManager simulationManager)
    {
        _serviceProvider = serviceProvider;
        _radarScreenManager = radarScreenManager;
        _simulationManager = simulationManager;
    }

    public async Task NewMessage(string username, string message) // Test
    {
        await Clients.All.SendAsync("messageReceived", username, message);
    }

    public async Task IncrementTestValue(string callsign, int value) // Test
    {
        await Clients.All.SendAsync("testValueIncremented", callsign, value + 1);
    }

    /// <summary>
    /// Called when a client connects to the SignalR hub
    /// Creates a new instance of RadarScreen and stores it in _radarScreens
    /// </summary>
    /// <returns>The base OnConnectedAsync function</returns>
    public override Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;
        Console.WriteLine($"DEBUG: new connectionId {connectionId}");
        RadarScreen newRadarScreen = _serviceProvider.GetService<RadarScreen>()
                                    ?? throw new InvalidOperationException("RadarScreen service did not register.");

        _radarScreenManager.AddRadarScreen(connectionId, newRadarScreen);
        if (!_simulationManager.IsSimulationStarted())
        {
            _simulationManager.StartSimulation();
        }

        return base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects from the SignalR hub
    /// </summary>
    /// <param name="exception"></param>
    /// <returns>The base OnDisconnectedAsync function</returns>
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        string connectionId = Context.ConnectionId;
        _radarScreenManager.RemoveRadarScreen(connectionId);

        return base.OnDisconnectedAsync(exception);
    }

}