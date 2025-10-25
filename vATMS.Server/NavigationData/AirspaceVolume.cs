using System.Drawing;

public class AirspaceVolume
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public int LowerLimit { get; init; }
    public int UpperLimit { get; init; }

    public List<DecimalCoordinate> Polygon { get; init; }

    public AirspaceVolume(
        string name,
        List<DecimalCoordinate> polygonCoordinates,
        int lowerLimit,
        int upperLimit
    )
    {
        Name = name;
        Polygon = polygonCoordinates;

        LowerLimit = lowerLimit;
        UpperLimit = upperLimit;

        Id = Guid.NewGuid();
    }

    // TODO: make this an interface, and have children "ControlledAirspace" and "SectorAirspace"

    public bool IsInside(DecimalCoordinate position, int level)
    {
        int count = 0;

        for (int i = 0; i < Polygon.Count; i++)
        {
            DecimalCoordinate p1 = Polygon[i];
            DecimalCoordinate p2 = Polygon[(i + 1) % Polygon.Count];

            if ((position.Lat < p1.Lat) != (position.Lat < p2.Lat)
            && position.Lon < p1.Lon + ((position.Lat - p1.Lat) / (p2.Lat - p1.Lat) * (p2.Lon - p1.Lon)))
            {
                count++;
            }
        }

        return (count % 2 == 1) && (level >= LowerLimit) && (level <= UpperLimit);
    }
}