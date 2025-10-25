namespace vATMS.Aircraft;

public class FlightPlan
{
    // TODO: Make some of these enums
    public string Callsign { get; set; }
    public string FlightRules { get; set; }

    public string ATYP { get; set; }
    public string WTC { get; set; }
    public string Equipment { get; set; }
    public string DEP { get; set; }
    public string DEST { get; set; }
    public int EOBT { get; set; }
    public int EET { get; set; }
    public string ALTN { get; set; }
    public string RMK { get; set; }

    public int RFL { get; set; }
    public int SPD { get; set; }

    public List<Waypoint> Route { get; set; }

    public FlightPlanAssignedData flightPlanAssignedData { get; }

    public int TestingValue { get; set; } // TODO: TESTING - Remove

    public FlightPlan
    (
        string callsign,
        string flightrules,
        string atyp,
        string wtc,
        string equipment,
        string dep,
        string dest,
        int eobt,
        int eet,
        string altn,
        string rmk,
        int rfl,
        int spd,
        List<Waypoint> route
    )
    {
        Callsign = callsign;
        FlightRules = flightrules;
        ATYP = atyp;
        WTC = wtc;
        Equipment = equipment;
        DEP = dep;
        DEST = dest;
        EOBT = eobt;
        EET = eet;
        ALTN = altn;
        RMK = rmk;
        TestingValue = 0;
        RFL = rfl;
        SPD = spd;

        Route = route;
        flightPlanAssignedData = new(this);
    }
}