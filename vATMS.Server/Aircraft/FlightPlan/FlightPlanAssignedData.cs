namespace vATMS.Aircraft;

public class FlightPlanAssignedData
{
    public int CFL { get; set; }
    public int ASP { get; set; }
    public int? AHDG { get; set; }
    public int? AVS { get; set; }
    public string? DirectToPoint { get; set; }

    public FlightPlanAssignedData() { }

}