namespace vATMS.Aircraft;

/// <summary>
/// Each FlightPlan instance has a single FlightPlanAssignedData instance.
/// This contains everything that would be assigned by ATC and associated with a single flight plan
/// </summary>
public class FlightPlanAssignedData
{
    public int CFL { get; set; }
    public int? _asp;
    public int? AVS { get; set; }

    private int? _ahdg;
    private string? _directToPoint;

    public List<StandingAgreement> StandingAgreements { get; init; }
    public int XFL { get; set; }

    // AHDG and DirectToPoint have explicit getters and setters as the other value has to be changed when one value is set

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

    public int? ASP
    {
        get => _asp;
        set
        {
            if (value == 0)
            {
                _asp = null;
            }
            else
            {
                _asp = value;
            }
        }
    }

    public FlightPlanAssignedData(List<StandingAgreement> standingAgreements)
    {
        StandingAgreements = standingAgreements;
    }
}