using System.Runtime.CompilerServices;

namespace vATMS.Aircraft;

public class Aircraft
{
    public RadarTarget? RadarTarget { get; set; }
    public FlightPlan FlightPlan { get; set; }

    public DecimalCoordinate Position { get; set; }
    public PredictedTrajectory Trajectory { get; set; }

    private double _heading;
    private double _indicatedAirspeed;
    private double _pilotSelectedAlitude;

    public double Track { get; private set; }
    public double PressureAltitude { get; private set; }
    public double QNHAltitude { get; private set; }
    public double Groundspeed { get; private set; }
    public int VerticalSpeed { get; private set; }

    public double FinalApchSpeed { get; init; }
    public double MinimumCleanSpeed { get; init; }

    public bool IsOnOwnNavigation { get; set; }
    public Waypoint NextWaypoint { get; set; }

    public bool IsOnNormalSpeed { get; set; }

    public Aircraft(
        FlightPlan flightPlan,
        DecimalCoordinate position,
        int track,
        int pressureAltitude,
        int indicatedAirspeed,
        int finalApchSpeed,
        int minimumCleanSpeed
        )
    {
        // TODO: TESTING - force creation of radar target with no radar head
        RadarTarget = new RadarTarget();
        FlightPlan = flightPlan;
        Position = position;
        Track = track;
        PressureAltitude = pressureAltitude;
        _indicatedAirspeed = indicatedAirspeed;

        FinalApchSpeed = finalApchSpeed;
        MinimumCleanSpeed = minimumCleanSpeed;

        IsOnOwnNavigation = false;
        NextWaypoint = flightPlan.Route.Peek();

        Trajectory = new();

        // TODO: TESTING - implement this properly
        Groundspeed = _indicatedAirspeed;
    }

    // Getting values
    public double GetMaximumForwardSpeed()
    {
        return 320.0d;
    }

    public double GetMaximumMachNumber()
    {
        return 0.8d;
    }

    public double GetMachNumber()
    {
        return 0.0d;
    }

    private double DetermineAppropriateSpeed()
    {
        return 220.0d;
    }

    private void LateralMovement()
    {
        // TODO: Implement this properly
        // This is TEMPORARY, UNREALISTIC code taken from a 2023 project. 
        // This has been put here only so things can be tested

        // Speed changes
        // acceleration/deceleration always 2kt s^-1

        if (IsOnNormalSpeed != true && _indicatedAirspeed != FlightPlan.flightPlanAssignedData.ASP)
        {
            FlySpeed(FlightPlan.flightPlanAssignedData.ASP);
        }
        else
        {

        }

        if (IsOnOwnNavigation != true && _heading != FlightPlan.flightPlanAssignedData.AHDG)
        {
            TurnToHeading(FlightPlan.flightPlanAssignedData.AHDG);
        }
        else
        {

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

    private void TurnToHeading(double newHeading)
    {
        const double G = 9.81;

        double turnRate = G * Math.Tan((Math.PI / 180) * 22) / (_indicatedAirspeed * 0.514444);
        turnRate = turnRate * (180 / Math.PI);

        double diff = (newHeading - _heading + 360) % 360;
        int turnDir = (diff > 180) ? -1 : 1;

        double step = Math.Min(turnRate, (turnDir == 1 ? diff : 360 - diff));
        _heading = (_heading + turnDir * step + 360) % 360;
    }

    private bool HasFlownOverCoordinate(DecimalCoordinate coordinate)
    {
        return false;
    }

    private void TurnDirectToCoordinate()
    {

    }

    private void TurnViaRoute()
    {
        // here, the "next" point is the one we are currently going towards, which would become the previous point
        DecimalCoordinate prev = NextWaypoint.Coordinate;
        DecimalCoordinate next = FlightPlan.Route.ElementAt(1).Coordinate;

        // heading for the leg (between previous point and next point) using haversine formula
        double brng = NavTools.BearingBetweenTwoPoints(prev, next);
        double dist = NavTools.DistanceBetweenTwoPoints(prev, next);

    }

    private void FlySpeed(double newSpeed)
    {
        if (_indicatedAirspeed > newSpeed)
        {
            // decelerate
            _indicatedAirspeed = Math.Max(_indicatedAirspeed - 2, newSpeed);
        }
        else if (_indicatedAirspeed < newSpeed)
        {
            // accelerate
            _indicatedAirspeed = Math.Min(_indicatedAirspeed + 2, newSpeed);
        }
    }

    private void VerticalMovement()
    {
        // TODO: Implement this properly
        // This is TEMPORARY, UNREALISTIC code taken from a 2023 project. 
        // This has been put here only so things can be tested

        double climbRate = (Groundspeed * 5) / 60;

        _pilotSelectedAlitude = FlightPlan.flightPlanAssignedData.CFL;

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

    public void predictTrajectory()
    {
        Trajectory.Update();
    }
}