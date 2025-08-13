namespace vATMS.Aircraft;

public class FlightPlanAssignedData
{
    private readonly FlightPlan _flightPlan;
    private string? _intentionCode;
    public int CFL { get; set; }
    public int ASP { get; set; }
    public int AHDG { get; set; }
    public int? AVS { get; set; }

    public string getIntentionCode()
    {
        // TODO: Convert to JSON-able rules
        // TODO: implement this properly
        if (_intentionCode == null)
        {
            return _flightPlan.DEST.Substring(_flightPlan.DEST.Length - 2); // TEMP: get last two chars
        }
        else
        {
            return _intentionCode;
        }
    }

    public FlightPlanAssignedData(FlightPlan flightPlan)
    {
        _flightPlan = flightPlan;
    }
}