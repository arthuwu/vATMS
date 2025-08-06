namespace vATMS.Aircraft;

public class RadarTarget
{
    public Guid Id { get; set; } // required for react key
    public Aircraft Aircraft { get; set; }
    // TESTING: RT needs this for position
    public FlightPlan? CorrelatedFlightPlan { get; set; }

    public RadarTarget(Aircraft aircraft)
    {

        Aircraft = aircraft;
        Id = Guid.NewGuid();
    }
}