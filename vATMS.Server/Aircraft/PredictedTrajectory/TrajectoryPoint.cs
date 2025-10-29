public class TrajectoryPoint
{
    public DateTime Time { get; init; }
    public DecimalCoordinate Position { get; init; }

    public TrajectoryPoint(
        DateTime time,
        DecimalCoordinate position
    )
    {
        Time = time;
        Position = position;
    }
}