class Waypoint
{
    public DecimalCoordinate Coordinate;
    public string Name;
    public Waypoint(double lat, double lon, string name)
    {
        Coordinate = new DecimalCoordinate(lat, lon);

        Name = name;
    }
}