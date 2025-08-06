namespace vATMS.Aircraft;

public class FlightPlan
{
    public string Callsign { get; set; }

    public int TestingValue { get; set; }

    public FlightPlan(string callsign)
    {
        Callsign = callsign;
        TestingValue = 0;
    }
}