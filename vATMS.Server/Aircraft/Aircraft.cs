class Aircraft
{
    public RadarTarget? RadarTarget { get; set; }
    public required FlightPlan FlightPlan { get; set; }

    public DecimalCoordinate Position { get; set; }

    public Aircraft()
    {
        // TESTING: force creation of radar target with no radar head
        RadarTarget = new RadarTarget(this);
        FlightPlan = new FlightPlan("ABC123");
    }
}   