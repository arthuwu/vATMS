class RadarTarget
{
    public Aircraft Aircraft { get; set; }
    // TESTING: RT needs this for position
    public FlightPlan? CorrelatedFlightPlan { get; set; }

    public RadarTarget(Aircraft aircraft)
    {

        Aircraft = aircraft;
    }
}