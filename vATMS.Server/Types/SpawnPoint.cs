public struct SpawnPoint
{
    // public string Id { get; set; } // internal UUID
    public string? NextWaypoint { get; set; }
    public string? Airway { get; set; }

    public DecimalCoordinate Coordinate { get; set; }

    public SpawnPoint(DecimalCoordinate coordinate)
    {
        Coordinate = coordinate;
    }
}