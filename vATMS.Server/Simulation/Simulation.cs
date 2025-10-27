using vATMS.Aircraft;

/// <summary>
/// A new Simulation instance is created when SimulationManager.StartSimulation() is called.
/// SimulationManager is an ASP.NET singleton, so there will always only be one instance of Simulation.
/// A Simulation instance is responsible for all "refreshes" of data (aircraft position)
/// </summary>
public class Simulation
{
    // int? rate, int? pilotQuality
    private readonly System.Threading.Timer _timer;
    private readonly TargetGenerator _targetGenerator;
    private readonly RadarScreenUpdater _radarScreenUpdater;
    private readonly NavigationData _navigationData;

    private List<Aircraft> _aircraft;
    // list<Controllers> controllers;

    /// <summary>
    /// Constructor for the Simulation instance
    /// </summary>
    /// <param name="radarScreenUpdater">The ASP.NET RadarScreenUpdater service</param>
    public Simulation(RadarScreenUpdater radarScreenUpdater)
    {
        _aircraft = new List<Aircraft>();
        _navigationData = new NavigationData();

        _targetGenerator = new TargetGenerator(1, 1, _aircraft, _navigationData); // TODO: test, demo values
        _radarScreenUpdater = radarScreenUpdater;

        _timer = new System.Threading.Timer(UpdateOnTimerElapsed, null, 0, 1000); // every second

        Console.WriteLine("DEBUG: new Simulation instance");
    }

    private async void UpdateOnTimerElapsed(object? state)
    {
        foreach (Aircraft ac in _aircraft)
        {
            // do stuff to aircraft
            // remember to check if it can be displayed using RadarHead, but otherwise
            //despawn if too far from centre
            // Console.WriteLine($"DEBUG: UpdateOnTimerElapsed, {ac.FlightPlan.Callsign} at {ac.Position.Lat}, {ac.Position.Lon}");
            ac.updatePosition();
        }

        await _radarScreenUpdater.UpdateAircraftPosition(_aircraft);
    }

    public Aircraft? FindAircraft(string selectedAircraftId)
    {
        return _aircraft.FirstOrDefault(a => a.FlightPlan.Callsign == selectedAircraftId);
    }
}