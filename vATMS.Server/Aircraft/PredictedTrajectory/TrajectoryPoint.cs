public class TrajectoryPoint
{
    public DateTime Time { get; init; }
    public DecimalCoordinate Position { get; init; }

    public double Altitude { get; init; }

    public TrajectoryPoint(
        DateTime time,
        DecimalCoordinate position,
        double altitude
    )
    {
        Time = time;
        Position = position;
        Altitude = altitude;
    }
}