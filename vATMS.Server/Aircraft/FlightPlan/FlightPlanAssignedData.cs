namespace vATMS.Aircraft;

public class FlightPlanAssignedData
{
    public int CFL { get; set; }
    public int? ASP { get; set; }
    public int? AVS { get; set; }

    private int? _ahdg { get; set; }
    private string? _directToPoint { get; set; }

    public int? AHDG
    {
        get => _ahdg;
        set
        {
            _ahdg = value;
            _directToPoint = null;
        }
    }

    public string? DirectToPoint
    {
        get => _directToPoint;
        set
        {
            _directToPoint = value;
            _ahdg = null;
        }
    }


    public FlightPlanAssignedData() { }

}