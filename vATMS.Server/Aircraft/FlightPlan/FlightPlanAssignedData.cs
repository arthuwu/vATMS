namespace vATMS.Aircraft;

/// <summary>
/// Each FlightPlan instance has a single FlightPlanAssignedData instance.
/// This contains everything that would be assigned by ATC and associated with a single flight plan
/// </summary>
public class FlightPlanAssignedData
{
    private int _cfl;
    private double? _asp;
    private int? _ahdg;
    private string? _directToPoint;

    public List<StandingAgreement> StandingAgreements { get; init; }
    public int XFL { get; set; }

    // AHDG, DirectToPoint and ASP have explicit getters and setters as the other value has to be changed when one value is set

    public double? AHDG
    {
        get => _ahdg;
        set
        {
            _ahdg = value != null ? Convert.ToInt32(value) : null;
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

    public double? ASP
    {
        get => _asp;
        set
        {
            if (value == 0)
            {
                _asp = null;
            }
            else if (value < 1) // mach number
            {
                _asp = value;
            }
            else if (value > 220 && value < 350)
            {
                _asp = value;
            }
        }
    }

    public double CFL
    {
        get => _cfl;
        set
        {
            _cfl = Convert.ToInt32(value);
        }
    }

    public FlightPlanAssignedData(List<StandingAgreement> standingAgreements)
    {
        StandingAgreements = standingAgreements;
    }
}