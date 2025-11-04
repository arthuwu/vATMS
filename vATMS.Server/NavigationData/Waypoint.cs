namespace vATMS.NavData;

public struct Waypoint
{
    public DecimalCoordinate Coordinate; //workaround
    public string Name;
    public Waypoint(DecimalCoordinate coord, string name)
    {
        Coordinate = coord;

        Name = name;
    }
}