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
    private string? _intentionCode;

    public int RFL { get; set; }
    public int SPD { get; set; }

    public Queue<Waypoint> Route { get; set; }

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
        Queue<Waypoint> route
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
        flightPlanAssignedData = new();
    }

    public string getIntentionCode()
    {
        // TODO: Convert to JSON-able rules
        // TODO: implement this properly
        if (_intentionCode == null)
        {
            return DEST.Substring(DEST.Length - 2); // TEMP: get last two chars
        }
        else
        {
            return _intentionCode;
        }
    }

    public void resolveDirectRoute()
    {
        // condition checked in Aircraft.cs
        while (Route.Count > 0 && !(Route.Peek().Name == flightPlanAssignedData.DirectToPoint))
        {
            Route.Dequeue();
        }
    }
}