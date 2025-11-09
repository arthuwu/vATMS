using vATMS.Aircraft;
using vATMS.NavData;

public struct PredictedConflictPair
{
    public string Callsign1 { get; init; }
    public string Callsign2 { get; init; }
    public double ClosestPointOfApproach { get; init; }
    public DateTime StartTime { get; init; }

    public PredictedConflictPair(string callsign1, string callsign2, double cpa, DateTime startTime)
    {
        Callsign1 = callsign1;
        Callsign2 = callsign2;
        ClosestPointOfApproach = cpa;
        StartTime = startTime;
    }
}

public class ConflictDetection
{
    private readonly SimulationManager _simulationManager;
    public ConflictDetection(SimulationManager simulationManager)
    {
        _simulationManager = simulationManager;
    }

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
                string callsignKey = string.Join("", callsignPair);

                if (conflictPairs.ContainsKey(callsignKey))
                {
                    if (conflictPairs[callsignKey].StartTime < box1.GetStartTime()) // already newer
                    {
                        continue;
                    }
                }

                double cpa = GetClosestPointOfApproach(box1, box2);

                PredictedConflictPair pcp = new(box1.Callsign, box2.Callsign, cpa, box1.GetStartTime());

                conflictPairs[callsignKey] = pcp;
            }

            return conflictPairs.Values.ToList();
        }

        return new();
    }


    private double GetClosestPointOfApproach(AABB box1, AABB box2)
    {
        // As a rough estimate, we will use the centre of the AABBs, since the AABB doesn't store the data for the actual trajectory
        // this will be "good enough" pratically as the MTCD has a greater margin vs actual loss of separation

        DecimalCoordinate centre1 = box1.GetCentre();
        DecimalCoordinate centre2 = box2.GetCentre();

        return NavTools.DistanceBetweenTwoPoints(centre1, centre2);
    }
}