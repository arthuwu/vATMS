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

    private int _missedStandingAgreements;

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

        _targetGenerator = new TargetGenerator(webEnv, Aircraft, _navigationData);
        _radarScreenUpdater = radarScreenUpdater;

        _cts = new CancellationTokenSource();
        _positionLoop = Task.Run(() => UpdatePositionLoop(_cts.Token));
        _trajectoryLoop = Task.Run(() => UpdateTrajectoryLoop(_cts.Token));

        Console.WriteLine("DEBUG: new Simulation instance");
    }

    /// <summary>
    /// Iteration to update the position of all Aircraft
    /// </summary>
    private async Task UpdatePositionLoop(CancellationToken token)
    {
        UpdatePosition();

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(1));

        while (await timer.WaitForNextTickAsync(token))
        {
            UpdatePosition();
        }
    }

    /// <summary>
    /// The body of the UpdatePositionLoop, so we can make it run once directly after initialisation
    /// </summary>
    private async void UpdatePosition()
    {
        foreach (Aircraft ac in Aircraft.Values)
        {
            // Console.WriteLine($"DEBUG: UpdateOnTimerElapsed, {ac.FlightPlan.Callsign} at {ac.Position.Lat}, {ac.Position.Lon}");
            ac.UpdatePosition();

            StandingAgreement? sa = ac.FlightPlan.ResolveStandingAgreements();

            if (sa != null)
            {
                if (ac.SatisfiedStandingAgreement(sa))
                {
                    if (sa.NextLevel != 0)
                    {
                        ac.FlightPlan.flightPlanAssignedData.CFL = sa.NextLevel;
                        await _radarScreenUpdater.UpdateRedundantAircraft(ac.FlightPlan.Callsign, sa.FromSector);
                    }
                    // Console.WriteLine("DEBUG: satisfied standing agreement");
                }
                else if (_navigationData.SectorList[sa.FromSector].IsUserControlled())
                {
                    _missedStandingAgreements += 1;
                }
            }

            ac.FlightPlan.DetermineXFL();
        }

        await _radarScreenUpdater.UpdateAircraftPosition(Aircraft.Values.ToList());

        List<string> stcaList = _conflictDetection.GetShortTermConflicts(new(Aircraft));

        await _radarScreenUpdater.UpdateShortTermConflictPairs(stcaList);
    }

    /// <summary>
    /// Iteration to update the trajectory of all Aircraft, or other infrequent tasks
    /// </summary>
    private async Task UpdateTrajectoryLoop(CancellationToken token)
    {
        await Task.Delay(100); // wait for the other loop to finish first

        UpdateTrajectory();

        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(15.1)); // to avoid coinciding with the other update loop as often

        while (await timer.WaitForNextTickAsync(token))
        {
            UpdateTrajectory();
        }
    }

    /// <summary>
    /// The body of the UpdateTrajectoryLoop, so we can make it run once directly after initialisation
    /// </summary>
    private async void UpdateTrajectory()
    {
        if (Aircraft.Values.Count() > 0)
        {
            foreach (Aircraft ac in Aircraft.Values) // use a copy of the list to prevent rewriting
            {
                _targetGenerator.DespawnIfOutsideRange(); // despawning isn't urgent, so we put it here for performance
                ac.PredictTrajectory();
            }

            List<PredictedConflictPair> conflictList = _conflictDetection.GetBroadphaseConflicts(Aircraft.Values.ToList());

            await _radarScreenUpdater.UpdateConflictPairs(conflictList);
        }

        await UpdateSimulationInfo();
    }

    /// <summary>
    /// Updates data related to the simulation info popup window
    /// It is separated from the other loop in order to expose it to the RadarScreenHub
    /// </summary>
    public async Task UpdateSimulationInfo()
    {
        await _radarScreenUpdater.UpdateSimulationInfo(_targetGenerator.GetSpawnRate(), _conflictDetection.LossesOfSeparation, _missedStandingAgreements);
    }

    /// <summary>
    /// Finds the aircraft instance referenced by its callsign
    /// </summary>
    /// <param name="selectedAircraftId">the required callsign</param>
    /// <returns>the required aircraft</returns>
    public Aircraft? FindAircraft(string selectedAircraftId)
    {
        return Aircraft[selectedAircraftId];
    }

    /// <summary>
    /// Changes the spawn rate of the simulation's target generator
    /// </summary>
    /// <param name="rate">the new rate</param>
    public void UpdateSpawnRate(int rate)
    {
        _targetGenerator.SetupTimer(rate);
    }

    /// <summary>
    /// Ends the simulation
    /// </summary>
    public void EndSimulation()
    {
        _cts.Cancel();
        Task.WaitAll(new[] { _positionLoop, _trajectoryLoop }, TimeSpan.FromSeconds(5));
        _cts.Dispose();
    }
}