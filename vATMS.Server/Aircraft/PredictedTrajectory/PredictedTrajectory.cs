public class PredictedTrajectory
{
    public List<TrajectoryPoint> PredictedPoints { get; set; } // generate and render this
    // public List<TrajectorySegments> PredictedSegments { get; private set; } // sort this
    public PredictedTrajectory()
    {
        PredictedPoints = new();
        // PredictedSegments = new();
    }

    public void GetSegments()
    {
    }
}