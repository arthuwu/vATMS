// This is the "point of connection" with React - for ALL clients INBOUND

using ElectronNET.API;
using ElectronNET.API.Entities;
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
    /// <param name="radarScreenManager"></param>
    /// <param name="simulationManager"></param>
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

    public async Task OpenSelectionWindow(string sectorId)
    {
        if (!HybridSupport.IsElectronActive)
            return;

        BrowserWindow window = await Electron.WindowManager.CreateWindowAsync(
            new BrowserWindowOptions
            {
                Width = 1280,
                Height = 720,
                AutoHideMenuBar = true,
                WebPreferences = new WebPreferences
                {
                    NodeIntegration = false, // force node off, otherwise electron won't be able to use eventsource, which breaks signalR entirely
                    ContextIsolation = true,
                }
            }
        );

        window.LoadURL($"http://localhost:8001/ifacts-selection-window/?sectorId={sectorId}");
    }

    public async Task SetSectorSelectedAircraftID(string sectorId, string selectedAircraftId)
    {
        await Clients.All.SendAsync("sectorSelectedId", sectorId, selectedAircraftId);
        // TODO: associate selection window with a specific RadarScreen instance s.t. no redundant broadcasting occurs
    }

    public async Task SetSelectedAircraftAssignedData(string selectedAircraftId, string data, string value)
    {
        if (_simulationManager.IsSimulationStarted())
        {
            Simulation simulation = _simulationManager.CurrentSimulation!; // if IsSimulationStarted is true, this will never be null
            Aircraft.Aircraft? aircraft = simulation.FindAircraft(selectedAircraftId);
            if (aircraft == null) return;  // and return early if no match

            aircraft.FlightPlan.flightPlanAssignedData.GetType().GetProperty(data)?.SetValue(aircraft.FlightPlan.flightPlanAssignedData, Convert.ToInt32(value));
        }

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

        // TODO: logic to associate a selection window with a radarScreen instance, instead of being its own instance.
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