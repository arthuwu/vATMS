namespace vATMS.Aircraft;

public class Aircraft
{
    public RadarTarget? RadarTarget { get; set; }
    public FlightPlan FlightPlan { get; set; }

    public DecimalCoordinate Position { get; set; }

    private double _heading;
    private double _indicatedAirspeed;

    public double Track { get; private set; }
    public double PressureAltitude { get; private set; }
    public double QNHAltitude { get; private set; }
    public double Groundspeed { get; private set; }
    public int VerticalSpeed { get; private set; }

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

    private void LateralMovement()
    {
        // TODO: Implement this properly
        // This is TEMPORARY, UNREALISTIC code taken from a 2023 project. 
        // This has been put here only so things can be tested

        // Speed changes
        // acceleration/deceleration always 2kt s^-1

        if (_indicatedAirspeed > FlightPlan.flightPlanAssignedData.ASP)
        {
            // decelerate
            _indicatedAirspeed = Math.Max(_indicatedAirspeed - 2, FlightPlan.flightPlanAssignedData.ASP);
        }
        else if (_indicatedAirspeed < FlightPlan.flightPlanAssignedData.ASP)
        {
            // accelerate
            _indicatedAirspeed = Math.Min(_indicatedAirspeed + 2, FlightPlan.flightPlanAssignedData.ASP);
        }

        double G = 9.81;

        // Turning
        double turnRate = G * Math.Tan((Math.PI / 180) * 22) / (_indicatedAirspeed * 0.514444);
        turnRate = turnRate * (180 / Math.PI);

        if (_heading != FlightPlan.flightPlanAssignedData.AHDG)
        {
            double diff = (FlightPlan.flightPlanAssignedData.AHDG - _heading + 360) % 360;
            int turnDir = (diff > 180) ? -1 : 1;

            double step = Math.Min(turnRate, (turnDir == 1 ? diff : 360 - diff));
            _heading = (_heading + turnDir * step + 360) % 360;
        }

        DecimalCoordinate positionOffset = new()
        {
            Lat = (Math.Cos((Math.PI / 180) * _heading) * _indicatedAirspeed * 0.514444) / 111320.0,
            Lon = (Math.Sin((Math.PI / 180) * _heading) * _indicatedAirspeed * 0.514444) / 111320.0 * Math.Cos(Position.Lat * Math.PI / 180),
        };

        // Position += positionOffset; 
        // TODO: make this possible

        Position = new() { Lat = Position.Lat + positionOffset.Lat, Lon = Position.Lon + positionOffset.Lon };

        // TEMP:
        Groundspeed = _indicatedAirspeed;
    }

    private void VerticalMovement()
    {
        // TODO: Implement this properly
        // This is TEMPORARY, UNREALISTIC code taken from a 2023 project. 
        // This has been put here only so things can be tested

        double climbRate = (Groundspeed * 5) / 60;

        if (PressureAltitude > FlightPlan.flightPlanAssignedData.CFL)
        {
            PressureAltitude = Math.Max(PressureAltitude - climbRate, FlightPlan.flightPlanAssignedData.CFL);
        }
        else if (PressureAltitude < FlightPlan.flightPlanAssignedData.CFL)
        {
            PressureAltitude = Math.Min(PressureAltitude + climbRate, FlightPlan.flightPlanAssignedData.CFL);
        }

        Console.WriteLine($"DEBUG: {PressureAltitude}, {FlightPlan.flightPlanAssignedData.CFL}, {climbRate}");
    }

    public void updatePosition()
    {
        LateralMovement();
        VerticalMovement();
    }
}