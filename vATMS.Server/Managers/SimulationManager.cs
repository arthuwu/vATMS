// This is the entry point to the simulation
using vATMS.Aircraft;
using vATMS.NavData;

/// <summary>
/// This is a WebApplicationBuilder.Services singleton class.
/// SimulationManager is responsible for storing all the status of the Simulation instance, such that it does not have to start when the server does.
/// </summary>
public class SimulationManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IWebHostEnvironment _webEnv;
    private static Simulation? _simulation = null;

    public Simulation? CurrentSimulation => _simulation; // explicit get operator

    public SimulationManager(IServiceProvider serviceProvider, IWebHostEnvironment webEnv)
    {
        _serviceProvider = serviceProvider;
        _webEnv = webEnv;
    }

    /// <summary>
    /// Starts the simulation is it has not been started
    /// </summary>
    public void StartSimulation()
    {
        if (_simulation == null)
        {
            _simulation = new Simulation(
                _webEnv,
                _serviceProvider.GetService<RadarScreenUpdater>()!,
                _serviceProvider.GetService<NavigationData>()!,
                _serviceProvider.GetService<ConflictDetection>()!
            );
        }
    }

    /// <summary>
    /// Determines if the simulation has been started
    /// </summary>
    /// <returns>true if it has been started</returns>
    public bool IsSimulationStarted()
    {
        return (_simulation != null) ? true : false;
    }

    /// <summary>
    /// Ends the simulation
    /// </summary>
    public void EndSimulation()
    {
        if (_simulation == null) return;

        _simulation.EndSimulation();
        _simulation = null;
    }
}
