namespace vATMS.NavData;

class Airport
{
    public DecimalCoordinate Coordinate;
    public string Name;
    public Airport(double lat, double lon, string name)
    {
        Coordinate = new DecimalCoordinate(lat, lon);
        Name = name;
    }
}