namespace vATMS.Aircraft;

public class RadarTarget
{
    public Guid Id { get; set; } // required for react key
    // public Aircraft Aircraft { get; set; }
    // TESTING: REMOVED FOR JSON 
    public FlightPlan? CorrelatedFlightPlan { get; set; }

    /* 
    public RadarTarget(Aircraft aircraft)
    {

        Aircraft = aircraft;
        Id = Guid.NewGuid();
    }
    */
    public RadarTarget()
    {
        Id = Guid.NewGuid();
    }
}