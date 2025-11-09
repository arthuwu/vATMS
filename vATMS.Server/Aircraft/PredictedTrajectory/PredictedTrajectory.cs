/// <summary>
/// Each Aircraft instance has a single PredictedTrajectory instance
/// This is used to store everything related to the future trajectory of the aircraft and/or should be updated every 15 seconds.
/// </summary>
public class PredictedTrajectory
{
    public List<TrajectoryPoint> PredictedPoints { get; set; } // generate and render this
    public string CurrentSector { get; set; }
    public string NextSector { get; set; }
    public DateTime? NextSectorEntryTime { get; set; }

    public PredictedTrajectory()
    {
        PredictedPoints = new();
        CurrentSector = "";
        NextSector = "";
    }

    /// <summary>
    /// Calculates the 4D AABB bounds used for conflict prediction, based on this.PredictedPoints, with margins (+-5nm laterally, +- 1000ft vertically) added.
    /// </summary>
    /// <returns>an array of 4-dimensional (min, max) tuples represented as (double, double)[4]</returns>
    public (double, double)[][] GetSegments()
    {
        const double FIVE_NM = 0.0833;

        (double, double)[][] segments = new (double, double)[PredictedPoints.Count() - 1][];

        for (int i = 0; i < PredictedPoints.Count() - 1; i++)
        {
            TrajectoryPoint p1 = PredictedPoints[i];
            TrajectoryPoint p2 = PredictedPoints[i + 1];

            (double, double) latPair = (Math.Min(p1.Position.Lat, p2.Position.Lat) - FIVE_NM, Math.Max(p1.Position.Lat, p2.Position.Lat) + FIVE_NM);
            (double, double) lonPair = (
                Math.Min(p1.Position.Lon, p2.Position.Lon) - FIVE_NM * Math.Cos(Math.Min(p1.Position.Lon, p2.Position.Lon) * Math.PI / 180),
                Math.Max(p1.Position.Lon, p2.Position.Lon) + FIVE_NM * Math.Cos(Math.Max(p1.Position.Lon, p2.Position.Lon) * Math.PI / 180)
            );
            (double, double) altPair = (Math.Min(p1.Altitude, p2.Altitude) - 500, Math.Max(p1.Altitude, p2.Altitude) + 500);
            (double, double) timePair = (Math.Min(p1.Time.ToOADate(), p2.Time.ToOADate()), Math.Max(p1.Time.ToOADate(), p2.Time.ToOADate()));

            segments[i] = new (double, double)[4] { latPair, lonPair, altPair, timePair };
        }

        return segments;
    }
}