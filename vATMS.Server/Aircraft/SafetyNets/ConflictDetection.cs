using vATMS.Aircraft;

public class ConflictDetection
{
    private readonly SimulationManager _simulationManager;
    public ConflictDetection(SimulationManager simulationManager)
    {
        _simulationManager = simulationManager;
    }

    public List<(Aircraft, Aircraft)> GetBroadphaseConflicts()
    {
        if (_simulationManager.IsSimulationStarted())
        {
            List<Aircraft> aircraftList = _simulationManager.CurrentSimulation!.Aircraft;
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

            if (conflictAABBPairs.Count() > 0)
            {
                foreach ((AABB box1, AABB box2) in conflictAABBPairs)
                {
                    Console.WriteLine($"conflict between {box1.Callsign ?? "null"} {box2.Callsign ?? "null"}");
                }
            }

            List<(Aircraft, Aircraft)> conflictPairs = new();

            foreach ((AABB box1, AABB box2) in conflictAABBPairs)
            {
                Aircraft? ac1 = null;
                Aircraft? ac2 = null;
                if (box1.Callsign != null)
                {
                    ac1 = _simulationManager.CurrentSimulation!.FindAircraft(box1.Callsign!);
                }
                if (box2.Callsign != null)
                {
                    ac2 = _simulationManager.CurrentSimulation!.FindAircraft(box2.Callsign!);
                }

                if (ac1 != null && ac2 != null) conflictPairs.Add((ac1, ac2));
            }

            return conflictPairs;
        }

        return new();
    }
}