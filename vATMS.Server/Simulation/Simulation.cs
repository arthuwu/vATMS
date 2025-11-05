using vATMS.Aircraft;
using vATMS.NavData;

/// <summary>
/// A new Simulation instance is created when SimulationManager.StartSimulation() is called.
/// SimulationManager is an ASP.NET singleton, so there will always only be one instance of Simulation.
/// A Simulation instance is responsible for all "refreshes" of data (aircraft position)
/// </summary>
public class Simulation
{
    // int? rate, int? pilotQuality
    private readonly System.Threading.Timer _timer;
    private readonly System.Threading.Timer _trajTimer;
    private readonly TargetGenerator _targetGenerator;
    private readonly RadarScreenUpdater _radarScreenUpdater;
    private readonly NavigationData _navigationData;
    private readonly ConflictDetection _conflictDetection;

    public List<Aircraft> Aircraft;
    // list<Controllers> controllers;

    /// <summary>
    /// Constructor for the Simulation instance
    /// </summary>
    /// <param name="radarScreenUpdater">The ASP.NET RadarScreenUpdater service</param>
    public Simulation(RadarScreenUpdater radarScreenUpdater, NavigationData navigationData, ConflictDetection conflictDetection)
    {
        Aircraft = new List<Aircraft>(); // Aircraft is the plural form of Aircraft (lol)
        _navigationData = navigationData;
        _conflictDetection = conflictDetection;

        _targetGenerator = new TargetGenerator(25, Aircraft, _navigationData); // The default spawn rate is 25. This value will be (re)set once loading has complete - the user is able to change the spawn rate during their session
        _radarScreenUpdater = radarScreenUpdater;

        _timer = new System.Threading.Timer(UpdateOnTimerElapsed, null, 0, 1000); // every second
        _trajTimer = new System.Threading.Timer(UpdateOnTrajTimerElapsed, null, 0, 15100); // every 15.1 seconds (to not coincide with _timer)

        Console.WriteLine("DEBUG: new Simulation instance");
    }

    private async void UpdateOnTimerElapsed(object? state)
    {
        foreach (Aircraft ac in Aircraft)
        {
            // do stuff to aircraft
            // remember to check if it can be displayed using RadarHead, but otherwise
            //despawn if too far from centre
            // Console.WriteLine($"DEBUG: UpdateOnTimerElapsed, {ac.FlightPlan.Callsign} at {ac.Position.Lat}, {ac.Position.Lon}");
            ac.UpdatePosition();
        }

        await _radarScreenUpdater.UpdateAircraftPosition(Aircraft);
    }

    private async void UpdateOnTrajTimerElapsed(object? state)
    {
        foreach (Aircraft ac in Aircraft)
        {
            ac.PredictTrajectory();
        }

        List<(Aircraft, Aircraft)> conflictList = _conflictDetection.GetBroadphaseConflicts();

        /*
        foreach ((Aircraft ac1, Aircraft ac2) in conflictList)
        {
            Console.WriteLine($"DEBUG: {ac1.FlightPlan.Callsign} is in conflict with {ac2.FlightPlan.Callsign}");
        }
        */

        await _radarScreenUpdater.UpdateAircraftPosition(Aircraft);
        await _radarScreenUpdater.UpdateConflictPairs(conflictList);
    }

    public Aircraft? FindAircraft(string selectedAircraftId)
    {
        return Aircraft.FirstOrDefault(a => a.FlightPlan.Callsign == selectedAircraftId);
    }
}