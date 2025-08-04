public interface ICoordinate<T>
{
    public T Lat { get; set; }
    public T Lon { get; set; }
}

public struct WGS84Coordinate : ICoordinate<string>
{
    public string Lat { get; set; }
    public string Lon { get; set; }

    // TODO: Validation?
}

public struct DecimalCoordinate : ICoordinate<double>
{
    public double Lat { get; set; }
    public double Lon { get; set; }

    public DecimalCoordinate(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90 || longitude < -180 || longitude > 180) // check for validity
        {
            throw new ArgumentOutOfRangeException("Invalid coordinate");
        }
        Lat = latitude;
        Lon = longitude;
    }
}