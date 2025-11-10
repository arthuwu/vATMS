namespace vATMS.NavData;

/// <summary>
/// A waypoint instance
/// </summary>
public struct Waypoint
{
    public DecimalCoordinate Coordinate;
    public string Name;
    public Waypoint(DecimalCoordinate coord, string name)
    {
        Coordinate = coord;

        Name = name;
    }
}