using vATMS.Aircraft;
using vATMS.NavData;


/// <summary>
/// A conflict between two aircraft's predicted trajectory
/// </summary>
public struct PredictedConflictPair
{
    public string Callsign1 { get; init; }
    public string Callsign2 { get; init; }
    public double ClosestPointOfApproach { get; init; }
    public DateTime StartTime { get; init; }
    public List<string> SectorsInvolved { get; init; }

    public PredictedConflictPair(string callsign1, string callsign2, double cpa, DateTime startTime, List<string> sectorsInvolved)
    {
        Callsign1 = callsign1;
        Callsign2 = callsign2;
        ClosestPointOfApproach = cpa;
        StartTime = startTime;
        SectorsInvolved = sectorsInvolved;
    }
}

/// <summary>
/// This is an ASP.NET singleton class responsible for detecting conflicts between predicted trajectories
/// </summary>
public class ConflictDetection
{
    private readonly SimulationManager _simulationManager;
    private List<PredictedConflictPair> _conflictList = new();
    private HashSet<string> _prevStca = new();

    public int LossesOfSeparation { get; private set; }

    public ConflictDetection(SimulationManager simulationManager)
    {
        _simulationManager = simulationManager;
    }

    /// <summary>
    /// Uses AABB trees to detect medium term conflicts between predicted trajectory of aircraft
    /// </summary>
    /// <param name="aircraft">List of all aircraft in the simulation</param>
    /// <returns>The list of conflicting pairs</returns>
    public List<PredictedConflictPair> GetBroadphaseConflicts(List<Aircraft> aircraft)
    {
        if (_simulationManager.IsSimulationStarted())
        {
            List<Aircraft> aircraftList = aircraft.ToList(); // use a copy instead of a reference
            List<AABB> trajectoryList = new();

            foreach (Aircraft ac in aircraftList)
            {
                (double, double)[][] segments = ac.Trajectory.GetSegments();
                foreach ((double, double)[] segment in segments)
                {
                    trajectoryList.Add(new AABB(ac.FlightPlan.Callsign, segment));
                }
            }

            AABBTree trajectoryTree = new(trajectoryList[0]);

            foreach (AABB box in trajectoryList.Skip(1))
            {
                trajectoryTree.AddNode(box);
            }

            List<(AABB, AABB)> conflictAABBPairs = trajectoryTree.GetOverlappingPairs();

            // we only want the first (earliest) conflict for a specific pair

            Dictionary<string, PredictedConflictPair> conflictPairs = new();

            foreach ((AABB box1, AABB box2) in conflictAABBPairs)
            {
                if (box1.Callsign == null || box2.Callsign == null) throw new Exception("AABB does not contain callsign, may be invalid");

                string[] callsignPair = [box1.Callsign, box2.Callsign];
                Array.Sort(callsignPair);
                string callsignKey = string.Join("", callsignPair); // unique key to use the dictionary as a set

                if (conflictPairs.ContainsKey(callsignKey))
                {
                    if (conflictPairs[callsignKey].StartTime < box1.GetStartTime()) // already newer
                    {
                        continue;
                    }
                }

                double cpa = GetClosestPointOfApproach(box1, box2);

                HashSet<string> involvedSectors = new();

                Aircraft? ac1 = _simulationManager.CurrentSimulation!.FindAircraft(box1.Callsign);
                Aircraft? ac2 = _simulationManager.CurrentSimulation!.FindAircraft(box1.Callsign);

                if (ac1 != null && ac2 != null) // we need to use the current simulation list here to ensure the conflicts are distributed to the correct sector
                {
                    string? sector1 = ac1.Trajectory.CurrentSector?.Id;
                    if (sector1 != null) involvedSectors.Add(sector1);

                    string? sector2 = ac1.Trajectory.NextSector?.Id;
                    if (sector2 != null) involvedSectors.Add(sector2);

                    string? sector3 = ac2.Trajectory.CurrentSector?.Id;
                    if (sector3 != null) involvedSectors.Add(sector3);

                    string? sector4 = ac2.Trajectory.NextSector?.Id;
                    if (sector4 != null) involvedSectors.Add(sector4);
                }

                PredictedConflictPair pcp = new(box1.Callsign, box2.Callsign, cpa, box1.GetStartTime(), involvedSectors.ToList());

                conflictPairs[callsignKey] = pcp;
            }

            _conflictList = conflictPairs.Values.ToList();
            return _conflictList;
        }

        return new();
    }

    /// <summary>
    /// Uses the data derived from broadphase conflict detction to determine if a pair of aircraft do not have sufficient separation
    /// </summary>
    /// <param name="aircraft">dictonary of all aircraft in the simulation</param>
    /// <returns>list of callsigns</returns>
    public List<string> GetShortTermConflicts(Dictionary<string, Aircraft> aircraft)
    {
        List<string> conflicts = new();
        HashSet<string> internalConflicts = new();

        foreach (PredictedConflictPair pcp in _conflictList)
        {
            Aircraft ac1 = aircraft[pcp.Callsign1];
            Aircraft ac2 = aircraft[pcp.Callsign2];
            DecimalCoordinate pos1 = ac1.Position;
            DecimalCoordinate pos2 = ac2.Position;

            if (NavTools.DistanceBetweenTwoPoints(pos1, pos2) < 5)
            {
                conflicts.Add(pcp.Callsign1);
                conflicts.Add(pcp.Callsign2);

                string[] callsignPair = [ac1.FlightPlan.Callsign, ac2.FlightPlan.Callsign];
                Array.Sort(callsignPair);

                string callsignKey = string.Join("", callsignPair);
                internalConflicts.Add(callsignKey); // unique key for HashSet

                if (!_prevStca.Contains(callsignKey))
                {
                    if ((ac1.Trajectory.CurrentSector != null && ac1.Trajectory.CurrentSector.IsUserControlled()) ||
                    (ac2.Trajectory.CurrentSector != null && ac2.Trajectory.CurrentSector.IsUserControlled()))
                    {
                        LossesOfSeparation++;
                    }
                }
            }
        }

        _prevStca = internalConflicts;
        return conflicts;
    }

    /// <summary>
    /// Calculates the distance between the midpoint of two AABBs
    /// </summary>
    /// <param name="box1"></param>
    /// <param name="box2"></param>
    /// <returns>the great circle distance</returns>
    private double GetClosestPointOfApproach(AABB box1, AABB box2)
    {
        // As a rough estimate, we will use the centre of the AABBs, since the AABB doesn't store the data for the actual trajectory
        // this will be "good enough" pratically as the MTCD has a greater margin vs actual loss of separation

        DecimalCoordinate centre1 = box1.GetCentre();
        DecimalCoordinate centre2 = box2.GetCentre();

        return NavTools.DistanceBetweenTwoPoints(centre1, centre2);
    }
}