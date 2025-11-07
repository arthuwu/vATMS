// This is the entry point to the simulation
using vATMS.Aircraft;
using vATMS.NavData;
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

    public bool IsSimulationStarted()
    {
        return (_simulation != null) ? true : false;
    }

    public void EndSimulation()
    {
        if (_simulation == null) return;

        _simulation.EndSimulation();
        _simulation = null;
    }
}
