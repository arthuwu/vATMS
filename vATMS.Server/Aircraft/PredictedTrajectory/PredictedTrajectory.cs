public class PredictedTrajectory
{
    public List<TrajectoryPoint> PredictedPoints { get; set; } // generate and render this
    public PredictedTrajectory()
    {
        PredictedPoints = new();
    }

    public (double, double)[][] GetSegments()
    {
        (double, double)[][] segments = new (double, double)[PredictedPoints.Count() - 1][];

        for (int i = 0; i < PredictedPoints.Count() - 1; i++)
        {
            TrajectoryPoint p1 = PredictedPoints[i];
            TrajectoryPoint p2 = PredictedPoints[i + 1];

            (double, double) latPair = (Math.Min(p1.Position.Lat, p2.Position.Lat) - 0.0833, Math.Max(p1.Position.Lat, p2.Position.Lat) + 0.0833);
            (double, double) lonPair = (
                Math.Min(p1.Position.Lon, p2.Position.Lon) - 0.0833 * Math.Cos(Math.Min(p1.Position.Lon, p2.Position.Lon) * Math.PI / 180),
                Math.Max(p1.Position.Lon, p2.Position.Lon) + 0.0833 * Math.Cos(Math.Max(p1.Position.Lon, p2.Position.Lon) * Math.PI / 180)
            );
            (double, double) altPair = (Math.Min(p1.Altitude, p2.Altitude) - 500, Math.Max(p1.Altitude, p2.Altitude) + 500);
            (double, double) timePair = (Math.Min(p1.Time.ToOADate(), p2.Time.ToOADate()), Math.Max(p1.Time.ToOADate(), p2.Time.ToOADate()));

            segments[i] = new (double, double)[4] { latPair, lonPair, altPair, timePair };
        }

        return segments;
    }
}