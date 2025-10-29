public class TrajectorySegments
{
    public TrajectoryPoint StartPoint { get; init; }
    public TrajectoryPoint EndPoint { get; init; }

    public TrajectorySegments(
        TrajectoryPoint startPoint,
        TrajectoryPoint endPoint
    )
    {
        StartPoint = startPoint;
        EndPoint = endPoint;
    }
}