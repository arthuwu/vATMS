using System.Collections.Concurrent;
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
    private readonly TargetGenerator _targetGenerator;
    private readonly RadarScreenUpdater _radarScreenUpdater;
    private readonly NavigationData _navigationData;
    private readonly ConflictDetection _conflictDetection;

    private readonly CancellationTokenSource _cts;
    private readonly Task _positionLoop;
    private readonly Task _trajectoryLoop;


    public ConcurrentDictionary<string, Aircraft> Aircraft;

    // list<Controllers> controllers;

    /// <summary>
    /// Constructor for the Simulation instance
    /// </summary>
    /// <param name="radarScreenUpdater">The ASP.NET RadarScreenUpdater service</param>
    public Simulation(IWebHostEnvironment webEnv, RadarScreenUpdater radarScreenUpdater, NavigationData navigationData, ConflictDetection conflictDetection)
    {
        Aircraft = new ConcurrentDictionary<string, Aircraft>(); // Aircraft is the plural form of Aircraft (lol)
        _navigationData = navigationData;
        _conflictDetection = conflictDetection;

        _targetGenerator = new TargetGenerator(webEnv, 25, Aircraft, _navigationData); // The default spawn rate is 25. This value will be (re)set once loading has complete - the user is able to change the spawn rate during their session
        _radarScreenUpdater = radarScreenUpdater;

        _cts = new CancellationTokenSource();
        _positionLoop = Task.Run(() => UpdatePositionLoop(_cts.Token));
        _trajectoryLoop = Task.Run(() => UpdateTrajectoryLoop(_cts.Token));

        Console.WriteLine("DEBUG: new Simulation instance");
    }

    private async Task UpdatePositionLoop(CancellationToken token)
    {
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(token))
        {
            foreach (Aircraft ac in Aircraft.Values)
            {
                // do stuff to aircraft
                // remember to check if it can be displayed using RadarHead, but otherwise
                // despawn if too far from centre
                // Console.WriteLine($"DEBUG: UpdateOnTimerElapsed, {ac.FlightPlan.Callsign} at {ac.Position.Lat}, {ac.Position.Lon}");
                ac.UpdatePosition();
            }

            await _radarScreenUpdater.UpdateAircraftPosition(Aircraft.Values.ToList());
        }
    }

    private async Task UpdateTrajectoryLoop(CancellationToken token)
    {
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(15.1));

        while (await timer.WaitForNextTickAsync(token))
        {
            foreach (Aircraft ac in Aircraft.Values) // use a copy of the list to prevent rewriting
            {
                ac.PredictTrajectory();
            }

            List<(Aircraft, Aircraft)> conflictList = _conflictDetection.GetBroadphaseConflicts(Aircraft.Values.ToList());

            await _radarScreenUpdater.UpdateConflictPairs(conflictList);
        }
    }

    public Aircraft? FindAircraft(string selectedAircraftId)
    {
        return Aircraft[selectedAircraftId];
    }

    public void EndSimulation()
    {
        _cts.Cancel();
        Task.WaitAll(new[] { _positionLoop, _trajectoryLoop }, TimeSpan.FromSeconds(5));
        _cts.Dispose();
    }
}