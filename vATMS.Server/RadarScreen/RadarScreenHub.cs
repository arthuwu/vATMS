// This is the "point of connection" with React - for ALL clients INBOUND
using vATMS.NavData;
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

    // TODO: for everything below: replace sectorId with RadarScreenId, as one RadarScreen may be controlling multiple sectors

    /// <summary>
    /// Opens a selection window (SelectionWindow React component displayed in a separate electron window) 
    /// </summary>
    /// <param name="sectorId">The sector ID of the logged on sector of the RadarScreen calling the function</param>
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

        window.LoadURL($"http://localhost:{Context.GetHttpContext()?.Connection.LocalPort}/ifacts-selection-window/?sector={sectorId}");
    }

    /// <summary>
    /// Called when a RadarScreen's selectedId changes.
    /// </summary>
    /// <param name="sectorId">The sector ID of the logged on sector of the RadarScreen calling the function</param>
    /// <param name="selectedAircraftId"></param>
    /// <returns></returns>
    public async Task SetSectorSelectedAircraftID(string sectorId, string selectedAircraftId)
    {
        foreach (RadarScreen rs in _radarScreenManager.GetRadarScreens(sectorId))
        {
            await Clients.Client(rs.ConnectionId).SendAsync("sectorSelectedId", sectorId, selectedAircraftId);
        }
    }


    /// <summary>
    /// Called when a RadarScreen/SelectionWindow changes an Aircraft's .FlightPlan.FlightPlanAssignedData
    /// </summary>
    /// <param name="selectedAircraftId">The ID of the aircraft currently selected by the RadarScreen calling the function</param>
    /// <param name="data">The name of the FlightPlanAssignedData item to be changed, e.g. "CFL"</param>
    /// <param name="value">The new value of the FlightPlanAssignedData</param>
    /// <returns></returns>
    public async Task SetSelectedAircraftAssignedData(string selectedAircraftId, string data, string value)
    {
        if (_simulationManager.IsSimulationStarted())
        {
            Simulation simulation = _simulationManager.CurrentSimulation!; // if IsSimulationStarted is true, this will never be null
            Aircraft.Aircraft? aircraft = simulation.FindAircraft(selectedAircraftId);
            if (aircraft == null) return;  // and return early if no match

            if (value.All(char.IsDigit))
                aircraft.FlightPlan.flightPlanAssignedData.GetType().GetProperty(data)?.SetValue(aircraft.FlightPlan.flightPlanAssignedData, Convert.ToInt32(value));
            else
                aircraft.FlightPlan.flightPlanAssignedData.GetType().GetProperty(data)?.SetValue(aircraft.FlightPlan.flightPlanAssignedData, value);
        }

    }

    public async Task SetSimulationRate(string rate)
    {
        if (_simulationManager.IsSimulationStarted())
        {
            int newRate;
            int.TryParse(rate, out newRate);
            _simulationManager.CurrentSimulation!.UpdateSpawnRate(newRate);
        }
    }

    /// <summary>
    /// Called when a client connects to the SignalR hub
    /// Creates a new instance of RadarScreen and stores it in _radarScreens
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        string connectionId = Context.ConnectionId;
        string sectorId = Context.GetHttpContext()?.Request.Query["sector"].ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(sectorId))
            throw new ArgumentException("Missing required parameter ('sector')");

        Console.WriteLine($"DEBUG: new connectionId {connectionId}");
        RadarScreen newRadarScreen = ActivatorUtilities.CreateInstance<RadarScreen>(_serviceProvider, connectionId, sectorId)
                                    ?? throw new InvalidOperationException("RadarScreen service did not register.");

        _radarScreenManager.AddRadarScreen(sectorId, newRadarScreen);

        if (!_simulationManager.IsSimulationStarted())
        {
            _simulationManager.StartSimulation();
        }

        await Clients.Client(connectionId).SendAsync("simulationReady");
        await base.OnConnectedAsync();
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