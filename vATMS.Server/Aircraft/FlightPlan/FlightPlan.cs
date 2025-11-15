namespace vATMS.Aircraft;

using vATMS.NavData;

/// <summary>
/// Each Aircraft instance has a single FlightPlan instance.
/// This contains everything that would, in the real world, be associated with an aircraft's ICAO flight plan
/// </summary>
public class FlightPlan
{
    // TODO: Make some of these enums and/or init only
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
    public string DisplayRoute { get; set; }
    public string IntentionCode { get; set; }

    public int RFL { get; set; }
    public int SPD { get; set; }

    public Queue<Waypoint> Route { get; set; }

    public FlightPlanAssignedData flightPlanAssignedData { get; }

    /// <summary>
    /// Constructor of the FlightPlan class
    /// </summary>
    /// <param name="callsign">Callsign. Must be unique in the simulation</param> //TODO: enforce this
    /// <param name="flightrules">Not used</param>
    /// <param name="atyp">Aircraft type</param>
    /// <param name="wtc">Wake turbulence category</param>
    /// <param name="equipment">ICAO equipment code</param>
    /// <param name="dep">Departure airport</param>
    /// <param name="dest">Destination airport</param>
    /// <param name="eobt">Estimated off block time</param>
    /// <param name="eet">Estimated enroute time</param>
    /// <param name="altn">Alternate</param>
    /// <param name="rmk">Remarks</param>
    /// <param name="rfl">Requested flight level</param>
    /// <param name="spd">Requested speed</param>
    /// <param name="route">Parsed route</param>
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
        Queue<Waypoint> route,
        string displayRoute,
        string? intentionCode,
        List<StandingAgreement> standingAgreements
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
        RFL = rfl;
        SPD = spd;

        Route = route;
        DisplayRoute = displayRoute;
        flightPlanAssignedData = new(standingAgreements.ToList()); // use a new copy, otherwise strange things happen

        IntentionCode = intentionCode ?? "";
    }

    /// <summary>
    /// Removes all waypoints on FlightPlan.Route preceeding the point being directed to
    /// </summary>
    public void ResolveDirectRoute()
    {
        // condition checked in Aircraft.cs
        while (Route.Count > 0 && !(Route.Peek().Name == flightPlanAssignedData.DirectToPoint))
        {
            Route.Dequeue();
        }
    }

    /// <summary>
    /// Removes all standing agreements which are no longer valid
    /// </summary>
    /// <returns>The StandingAgreement if a standing agreement has been removed, if not null</returns>
    public StandingAgreement? ResolveStandingAgreements()
    {
        // each route will not normally have more than 4 standing agreements, so I believe iteration will be ok here
        foreach (StandingAgreement sa in flightPlanAssignedData.StandingAgreements)
        {
            if (sa.Point == "")
            {
                if (flightPlanAssignedData.CFL == sa.Level)
                {
                    return sa;
                }
            }

            if (!Route.Select(e => e.Name).ToList().Contains(sa.Point))
            {
                flightPlanAssignedData.StandingAgreements.Remove(sa); // then it must have just passed that standing agreement, and so we can
                return sa;
            }
        }

        return null;
    }

    /// <summary>
    /// Determines the XFL (transfer flight level) to be displayed
    /// </summary>
    public void DetermineXFL()
    {
        if (flightPlanAssignedData.StandingAgreements.Count() > 0)
        {
            flightPlanAssignedData.XFL = flightPlanAssignedData.StandingAgreements[0].Level;
        }
        else
        {
            flightPlanAssignedData.XFL = RFL;
        }
    }
}