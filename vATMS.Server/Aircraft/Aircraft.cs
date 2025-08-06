namespace vATMS.Aircraft;

public class Aircraft
{
    public RadarTarget? RadarTarget { get; set; }
    public FlightPlan FlightPlan { get; set; }

    public DecimalCoordinate Position { get; set; }

    public Aircraft()
    {
        // TESTING: force creation of radar target with no radar head
        RadarTarget = new RadarTarget(this);
        FlightPlan = new FlightPlan("ABC123");
    }
}