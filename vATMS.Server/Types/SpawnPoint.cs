// TODO: TYPES

public struct SpawnPoint
{
    // public string Id { get; set; } // internal UUID
    public string? NextWaypoint { get; set; }
    public string? Airway { get; set; }
    public int[] AvailableLevels { get; set; }
    public int InitialTrack { get; set; }

    public DecimalCoordinate Coordinate { get; set; }

    public SpawnPoint(DecimalCoordinate coordinate, int[] availableLevels, int initialTrack)
    {
        Coordinate = coordinate;
        AvailableLevels = availableLevels;
        InitialTrack = initialTrack;
    }

    public int getSpawnLevel()
    {
        // TODO: actually implement this
        return AvailableLevels[0];
    }
}