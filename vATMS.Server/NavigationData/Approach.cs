namespace vATMS.NavData;

/// <summary>
/// Not used, as its intended use case was not implemented
/// </summary>
class Approach
{
    public DecimalCoordinate Coordinate; //Use GP coordinate
    public string Type;
    public string Name;
    public Approach(double lat, double lon, string type, string name)
    {
        Coordinate = new DecimalCoordinate(lat, lon);
        Type = type;
        Name = name;
    }
}