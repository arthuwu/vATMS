public class PredictedTrajectory
{
    public List<(DecimalCoordinate coord, DateTime timestamp)> PredictedPoints;
    public PredictedTrajectory()
    {
        PredictedPoints = new();
    }

    public void Update()
    {
    }
}