using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;

namespace vATMS.Aircraft;

public class Aircraft
{
    public RadarTarget? RadarTarget { get; set; }
    public FlightPlan FlightPlan { get; set; }

    public DecimalCoordinate Position { get; set; }
    public PredictedTrajectory Trajectory { get; set; }

    private NavigationData _navigationData;

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

    public Aircraft(
        FlightPlan flightPlan,
        DecimalCoordinate position,
        int track,
        int pressureAltitude,
        int indicatedAirspeed,
        int finalApchSpeed,
        int minimumCleanSpeed,
        NavigationData navigationData
        )
    {
        _navigationData = navigationData;
        // TODO: TESTING - force creation of radar target with no radar head
        RadarTarget = new RadarTarget();
        FlightPlan = flightPlan;
        Position = position;
        Track = track;
        PressureAltitude = pressureAltitude;
        _indicatedAirspeed = indicatedAirspeed;

        FinalApchSpeed = finalApchSpeed;
        MinimumCleanSpeed = minimumCleanSpeed;

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

        if (FlightPlan.flightPlanAssignedData.ASP != null)
        {
            if (_indicatedAirspeed != FlightPlan.flightPlanAssignedData.ASP)
            {
                FlySpeed((double)FlightPlan.flightPlanAssignedData.ASP);
            }
        }
        else
        {

        }

        if (FlightPlan.flightPlanAssignedData.AHDG != null) // isonownnav broken
        {
            if (_heading != FlightPlan.flightPlanAssignedData.AHDG)
                TurnToHeading((double)FlightPlan.flightPlanAssignedData.AHDG!);
        }
        else if (FlightPlan.flightPlanAssignedData.DirectToPoint != null)
        {
            // can only direct to fix on route
            if (FlightPlan.Route.Any(wp => wp.Name == FlightPlan.flightPlanAssignedData.DirectToPoint))
            {
                FlightPlan.resolveDirectRoute();
                TurnViaRoute();
            }
            else
            {
                TurnViaDirect(_navigationData.WpList[FlightPlan.flightPlanAssignedData.DirectToPoint!].Coordinate);
                // this is for testing ONLY
                // TODO: replace with resetting to null
            }
        }
        else
        {
            TurnViaRoute();
        }
        // user input limitations will prevent directing to unknown waypoints

        // TEMP:
        Groundspeed = _indicatedAirspeed;

        const double KNOT_TO_MS = 0.514444;
        const double R = 6371e+003;

        double old_lat = Position.Lat * Math.PI / 180;
        double old_lon = Position.Lon * Math.PI / 180;
        double brng = _heading * Math.PI / 180; // rad
        double magnitude = Groundspeed * KNOT_TO_MS; // ms^-1

        double new_lat = Math.Asin(Math.Sin(old_lat) * Math.Cos(magnitude / R) + Math.Cos(old_lat) * Math.Sin(magnitude / R) * Math.Cos(brng));
        double new_lon = old_lon + Math.Atan2(Math.Sin(brng) * Math.Sin(magnitude / R) * Math.Cos(old_lat), Math.Cos(magnitude / R) - Math.Sin(old_lat) * Math.Sin(new_lat));


        Position = new() { Lat = new_lat * 180 / Math.PI, Lon = new_lon * 180 / Math.PI };

    }

    private void TurnToHeading(double newHeading)
    {
        const double G = 9.81;
        const double KNOT_TO_MS = 0.514444;

        int bankAngle = _indicatedAirspeed > 300 ? 20 : 25; // TODO: make this not instantaneous, and compensate with margin
        double turnRate = G * Math.Tan(bankAngle * Math.PI / 180) / (_indicatedAirspeed * KNOT_TO_MS);
        turnRate = turnRate * 180 / Math.PI;

        double diff = (newHeading - _heading + 360) % 360;
        int turnDir = (diff > 180) ? -1 : 1;

        double step = Math.Min(turnRate, (turnDir == 1 ? diff : 360 - diff));
        _heading = (_heading + turnDir * step + 360) % 360;
    }

    private bool IsDirectPossible(DecimalCoordinate coordinate)
    {
        return false;
    }

    private void TurnDirectToCoordinate(DecimalCoordinate coordinate)
    {
        Console.WriteLine($"DEBUG: TURNING TO {coordinate.Lat}, {coordinate.Lon}");
        double brng = NavTools.BearingBetweenTwoPoints(Position, coordinate);
        Console.WriteLine($"DEBUG: HEADING REQUIRED {brng}, ACTUAL {_heading}");

        // TODO: track instead of heading
        if (_heading != brng)
        {
            TurnToHeading(brng);
        }
    }

    private void TurnViaDirect(DecimalCoordinate coordinate)
    {
        // TODO: first check if direct is possible - DO THIS IN THE MENU

        Console.WriteLine($"DEBUG: TURNING TO {coordinate.Lat}, {coordinate.Lon}");


        double brng = NavTools.BearingBetweenTwoPoints(Position, coordinate);
        Console.WriteLine($"DEBUG: HEADING REQUIRED {brng}, ACTUAL {_heading}");

        // TODO: track instead of heading

        TurnToHeading(brng);

        if (_heading == brng)
        {
            FlightPlan.flightPlanAssignedData.DirectToPoint = null;
        }
    }

    private void TurnViaRoute()
    {
        const double G = 9.81;
        const double M_TO_NM = 0.000539957;
        const double KNOT_TO_MS = 0.514444;

        // here, the "prev" point is the one we are currently going towards, which would become the previous point
        DecimalCoordinate prev = FlightPlan.Route.ElementAt(0).Coordinate;
        // check element 1 actually exists first!!!!!!
        DecimalCoordinate next = FlightPlan.Route.ElementAt(1).Coordinate;

        // heading for the leg (between previous point and next point) using haversine formula
        double currBrng = NavTools.BearingBetweenTwoPoints(Position, prev);
        double nextBrng = NavTools.BearingBetweenTwoPoints(prev, next);

        double dist = NavTools.DistanceBetweenTwoPoints(Position, prev);

        Console.WriteLine($"DEBUG: TURNING TO {prev.Lat}, {prev.Lon}");
        Console.WriteLine($"DEBUG: HEADING REQUIRED {currBrng}, ACTUAL {_heading}");

        TurnToHeading(currBrng);

        if (_heading == currBrng)
        {
            Console.WriteLine("DEBUG: Established on track");

            int bankAngle = _indicatedAirspeed > 300 ? 20 : 25;
            double turnRadius = Math.Pow(_indicatedAirspeed * KNOT_TO_MS, 2) / (G * Math.Tan(bankAngle * Math.PI / 180));
            Console.WriteLine(turnRadius);

            double turnAngle = (Math.Abs(nextBrng - (currBrng + 180)) + 360) % 360;
            if (turnAngle > 180)
            {
                turnAngle = 360 - turnAngle;
            }

            double distanceRequired = turnRadius / Math.Tan(turnAngle * Math.PI / 180 / 2) * M_TO_NM;

            Console.WriteLine($"DEBUG: distanceRequired: {distanceRequired}, dist = {dist}, to turn from {_heading} to {nextBrng}");

            if (dist <= distanceRequired)
            {
                Console.WriteLine($"DEBUG: dequeuing {FlightPlan.Route.ElementAt(0).Name}");
                FlightPlan.Route.Dequeue();
            }
        }

        // must calculate first in case there is no straight segment
        // then we decide whether we want to turn or not

        // TODO: track instead of heading

        //if (_heading != nextBrng)
        //{
        //    TurnDirectToCoordinate(next);
        //}

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