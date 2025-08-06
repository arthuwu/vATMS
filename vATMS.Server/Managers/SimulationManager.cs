// This is the entry point to the simulation
using vATMS.Aircraft;
public class SimulationManager
{
    private readonly IServiceProvider _serviceProvider;
    private static Simulation? _simulation = null;

    public SimulationManager(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public void StartSimulation()
    {
        if (_simulation == null)
        {
            _simulation = new Simulation(_serviceProvider.GetService<RadarScreenUpdater>()!);
        }
    }

    public bool IsSimulationStarted()
    {
        return (_simulation != null) ? true : false;
    }

    public void EndSimulation()
    {
        _simulation = null;
    }
}
