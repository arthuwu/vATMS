using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using vATMS.NavData;

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
        double track,
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
        _heading = track;
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

        Position = NavTools.TranslatePoint(Position, _heading, Groundspeed / 3600);

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

    private void TurnViaDirect(DecimalCoordinate coordinate)
    {
        // TODO: first check if direct is possible - DO THIS IN THE MENU

        //Console.WriteLine($"DEBUG: TURNING TO {coordinate.Lat}, {coordinate.Lon}");


        double brng = NavTools.BearingBetweenTwoPoints(Position, coordinate);
        //Console.WriteLine($"DEBUG: HEADING REQUIRED {brng}, ACTUAL {_heading}");

        // TODO: track instead of heading

        TurnToHeading(brng);

        if (_heading == brng)
        {
            FlightPlan.flightPlanAssignedData.DirectToPoint = null;
        }
    }

    private void TurnViaRoute()
    {
        if (FlightPlan.Route.Count() > 1)
        {
            // here, the "prev" point is the one we are currently going towards, which would become the previous point
            DecimalCoordinate prev = FlightPlan.Route.ElementAt(0).Coordinate;
            DecimalCoordinate next = FlightPlan.Route.ElementAt(1).Coordinate;

            // heading for the leg (between previous point and next point) using haversine formula
            double currBrng = NavTools.BearingBetweenTwoPoints(Position, prev);
            double nextBrng = NavTools.BearingBetweenTwoPoints(prev, next);

            double dist = NavTools.DistanceBetweenTwoPoints(Position, prev);

            //Console.WriteLine($"DEBUG: TURNING TO {prev.Lat}, {prev.Lon}");
            //Console.WriteLine($"DEBUG: HEADING REQUIRED {currBrng}, ACTUAL {_heading}");

            TurnToHeading(currBrng);

            if (dist <= 0.2 || dist <= _indicatedAirspeed / 3600)  // if the distance required is smaller than the update distance, it won't dequeue properly, so this is required
            {
                FlightPlan.Route.Dequeue();
                return;
            }

            if (_heading == currBrng)
            {
                //Console.WriteLine("DEBUG: Established on track");

                const double G = 9.81;
                const double M_TO_NM = 0.000539957;
                const double KNOT_TO_MS = 0.514444;

                int bankAngle = _indicatedAirspeed > 300 ? 20 : 25;
                double turnRadius = Math.Pow(_indicatedAirspeed * KNOT_TO_MS, 2) / (G * Math.Tan(bankAngle * Math.PI / 180));

                double turnAngle = (Math.Abs(nextBrng - (currBrng + 180)) + 360) % 360;
                if (turnAngle > 180)
                {
                    turnAngle = 360 - turnAngle;
                }

                double distanceRequired = turnRadius / Math.Tan(turnAngle * Math.PI / 180 / 2) * M_TO_NM;

                // Console.WriteLine($"DEBUG: distanceRequired: {distanceRequired}, dist = {dist}, to turn from {_heading} to {nextBrng}");

                if (dist <= distanceRequired)
                {
                    //Console.WriteLine($"DEBUG: dequeuing {FlightPlan.Route.ElementAt(0).Name}");
                    FlightPlan.Route.Dequeue();
                }
            }
        }
        else if (FlightPlan.Route.Count() == 1)
        {
            TurnViaDirect(FlightPlan.Route.ElementAt(0).Coordinate); // holds at last waypoint
        }
        else
        {
            FlightPlan.flightPlanAssignedData.AHDG = Convert.ToInt32(_heading);
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

        //Console.WriteLine($"DEBUG: {PressureAltitude}, {FlightPlan.flightPlanAssignedData.CFL}, {climbRate}");
    }

    public void UpdatePosition()
    {
        LateralMovement();
        VerticalMovement();
    }

    public void PredictTrajectory()
    {
        const double M_TO_NM = 0.000539957;
        const double KNOT_TO_MS = 0.514444;

        // TODO: perhaps get some sort of energy profile in. But it's not important, as it doesn't really affect the actual CD
        DecimalCoordinate simPosition = Position;
        Queue<Waypoint> simRoute = new(FlightPlan.Route);
        double simHeading = _heading;
        double simAltitude = PressureAltitude;

        Trajectory.PredictedPoints.Clear();

        for (int t = 0; t < 30; t++)
        {
            double segmentDistance = Groundspeed * KNOT_TO_MS * 30 * M_TO_NM; // s = ut; t = 30 seconds; in nm

            while (segmentDistance > 0)
            {
                if (FlightPlan.flightPlanAssignedData.AHDG != null || simRoute.Count() < 1)
                {
                    simPosition = NavTools.TranslatePoint(simPosition, simHeading, segmentDistance);
                    segmentDistance = 0;
                    continue;
                }

                DecimalCoordinate curr = simRoute.ElementAt(0).Coordinate;

                double currBrng = NavTools.BearingBetweenTwoPoints(simPosition, curr); //deg

                double currentLegDist = NavTools.DistanceBetweenTwoPoints(simPosition, curr); //nm

                // my attempts at making the curves smooth were to no avail - so this is if the curves are sharp...
                if (segmentDistance >= currentLegDist)
                {
                    segmentDistance -= currentLegDist;
                    simPosition = curr;
                    simRoute.Dequeue();
                }
                else
                {
                    simPosition = NavTools.TranslatePoint(simPosition, currBrng, segmentDistance);
                    simHeading = currBrng;
                    segmentDistance = 0;
                }
            }

            Trajectory.PredictedPoints.Add(new TrajectoryPoint(DateTime.UtcNow.Add(new(0, 0, 30 * t)), simPosition, simAltitude));
        }

        foreach (Sector av in _navigationData.SectorList.Select(s => s.Value))
        {
            Trajectory.CurrentSector = "";
            Trajectory.NextSector = "";
            if (av.SectorVolume.IsInside(Position, PressureAltitude))
            {
                Trajectory.CurrentSector = av.Id;
            }
            if (av.SectorVolume.IsInside(simPosition, simAltitude))
            {
                Trajectory.NextSector = av.Id;
            }
        }
    }
}