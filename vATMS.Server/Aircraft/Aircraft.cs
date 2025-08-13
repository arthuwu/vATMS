namespace vATMS.Aircraft;

public class Aircraft
{
    public RadarTarget? RadarTarget { get; set; }
    public FlightPlan FlightPlan { get; set; }

    public DecimalCoordinate Position { get; set; }

    private int _heading;
    private int _indicatedAirspeed;

    public int Track { get; }
    public int PressureAltitude { get; }
    public int QNHAltitude { get; }
    public int Groundspeed { get; }
    public int VerticalSpeed { get; }

    public Aircraft(
        FlightPlan flightPlan,
        DecimalCoordinate position,
        int track,
        int pressureAltitude,
        int indicatedAirspeed
        )
    {
        // TODO: TESTING - force creation of radar target with no radar head
        RadarTarget = new RadarTarget();
        FlightPlan = flightPlan;
        Position = position;
        Track = track;
        PressureAltitude = pressureAltitude;
        _indicatedAirspeed = indicatedAirspeed;

        // TODO: TESTING - implement this properly
        Groundspeed = _indicatedAirspeed;
    }
}