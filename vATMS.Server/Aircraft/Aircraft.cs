using vATMS.NavData;

namespace vATMS.Aircraft;

/// <summary>
/// Each Aircraft instance represents an aircraft that has been spawned in to the simulation.
/// This is where all operations regarding the movement of an aircraft take place
/// </summary>
public class Aircraft
{
    public RadarTarget? RadarTarget { get; set; }
    public FlightPlan FlightPlan { get; set; }

    public DecimalCoordinate Position { get; set; }
    public PredictedTrajectory Trajectory { get; set; }

    private NavigationData _navigationData;
    private AircraftPerformance _aircraftPerformance;

    private double _heading;
    private double _indicatedAirspeed;

    public double Track { get; private set; }
    public double PressureAltitude { get; private set; }
    public double QNHAltitude { get; private set; }
    public double Groundspeed { get; private set; }
    public int VerticalSpeed { get; private set; }

    /// <summary>
    /// Constructor for the Aircraft class
    /// </summary>
    /// <param name="flightPlan">the FlightPlan instance associated with the Aircraft</param>
    /// <param name="position">the initial position of the aircraft</param>
    /// <param name="track">the initial track of the aircraft</param>
    /// <param name="pressureAltitude">the initial pressure altitude of the aircraft</param>
    /// <param name="navigationData">the simulation NavigationData</param>
    /// <param name="aircraftPerformance">the AircraftPerformance instance associated with the Aircraft</param>
    public Aircraft(
        FlightPlan flightPlan,
        DecimalCoordinate position,
        double track,
        int pressureAltitude,
        NavigationData navigationData,
        AircraftPerformance aircraftPerformance
        )
    {
        _aircraftPerformance = aircraftPerformance;
        _navigationData = navigationData;
        RadarTarget = new RadarTarget(); // Not in use
        FlightPlan = flightPlan;
        Position = position;
        _heading = track;
        Track = track;
        PressureAltitude = pressureAltitude;
        _indicatedAirspeed = _aircraftPerformance.GetNormalIndicatedAirspeed(pressureAltitude, pressureAltitude);

        FlightPlan.flightPlanAssignedData.CFL = pressureAltitude;

        Trajectory = new();

        Groundspeed = GetGroundspeed(_indicatedAirspeed, pressureAltitude);
    }

    /// <summary>
    /// Converts an indicated airspeed into a true airspeed (Groundspeed without wind)
    /// </summary>
    /// <param name="indicatedAirspeed"></param>
    /// <param name="pressureAltitude"></param>
    /// <returns>the required groundspeed</returns>
    private double GetGroundspeed(double indicatedAirspeed, double pressureAltitude)
    {
        double metricAlt = pressureAltitude / 3.281;

        double pressure = 101325 * Math.Pow((1 - 0.0000225577 * metricAlt), 5.25588);

        double temperature = 288.15 - (0.00649 * metricAlt);

        double airDensity = pressure / (287.05 * temperature);

        return indicatedAirspeed * Math.Sqrt(1.225 / airDensity);
    }

    /// <summary>
    /// Provides logic for the lateral movement of aircraft
    /// </summary>
    private void LateralMovement()
    {

        if (FlightPlan.flightPlanAssignedData.ASP != null)
        {
            if (FlightPlan.flightPlanAssignedData.ASP < 1) // is mach number
            {
                FlySpeed(_aircraftPerformance.GetIndicatedAirspeedFromMach(PressureAltitude, _aircraftPerformance.GetNormalisedMachNumber((double)FlightPlan.flightPlanAssignedData.ASP)));
            }
            else
            {
                FlySpeed(_aircraftPerformance.GetNormalisedSpeed(PressureAltitude, (double)FlightPlan.flightPlanAssignedData.ASP));
            }
        }
        else
        {
            FlySpeed(_aircraftPerformance.GetNormalIndicatedAirspeed(PressureAltitude, FlightPlan.flightPlanAssignedData.CFL));
        }

        if (FlightPlan.flightPlanAssignedData.AHDG != null)
        {
            if (_heading != FlightPlan.flightPlanAssignedData.AHDG)
                TurnToHeading((double)FlightPlan.flightPlanAssignedData.AHDG!);
        }
        else if (FlightPlan.flightPlanAssignedData.DirectToPoint != null)
        {
            // can only direct to fix on route
            if (FlightPlan.Route.Any(wp => wp.Name == FlightPlan.flightPlanAssignedData.DirectToPoint))
            {
                FlightPlan.ResolveDirectRoute();
                TurnViaRoute();
            }
            else
            {
                FlightPlan.flightPlanAssignedData.DirectToPoint = null;
            }
        }
        else
        {
            TurnViaRoute();
        }

        Position = NavTools.TranslatePoint(Position, _heading, Groundspeed / 3600);
    }

    /// <summary>
    /// Turns the aircraft in increments to the required heading
    /// </summary>
    /// <param name="newHeading">(magnetic) heading to turn to</param>
    private void TurnToHeading(double newHeading)
    {
        const double G = 9.81;
        const double KNOT_TO_MS = 0.514444;

        int bankAngle = _indicatedAirspeed > 300 ? 20 : 25;
        double turnRate = G * Math.Tan(bankAngle * Math.PI / 180) / (_indicatedAirspeed * KNOT_TO_MS);
        turnRate = turnRate * 180 / Math.PI;

        double diff = (newHeading - _heading + 360) % 360;
        int turnDir = (diff > 180) ? -1 : 1;

        double step = Math.Min(turnRate, (turnDir == 1 ? diff : 360 - diff));
        _heading = (_heading + turnDir * step + 360) % 360;
    }

    /// <summary>
    /// Turns the aircraft in increments direct to a particular coordinate
    /// </summary>
    /// <param name="coordinate">coordinate to turn to</param>
    private void TurnViaDirect(DecimalCoordinate coordinate)
    {
        //Console.WriteLine($"DEBUG: TURNING TO {coordinate.Lat}, {coordinate.Lon}");


        double brng = NavTools.BearingBetweenTwoPoints(Position, coordinate);
        //Console.WriteLine($"DEBUG: HEADING REQUIRED {brng}, ACTUAL {_heading}");


        TurnToHeading(brng);

        if (_heading == brng)
        {
            FlightPlan.flightPlanAssignedData.DirectToPoint = null;
        }
    }

    /// <summary>
    /// Turns the aircraft in increments via its flight planned route
    /// </summary>
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
                double turnRadius = Math.Pow(Groundspeed * KNOT_TO_MS, 2) / (G * Math.Tan(bankAngle * Math.PI / 180));

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
            DecimalCoordinate curr = FlightPlan.Route.ElementAt(0).Coordinate;

            double dist = NavTools.DistanceBetweenTwoPoints(Position, curr);

            if (dist <= 0.2 || dist <= Groundspeed / 3600) // if it'll reach the waypoint within the next update-ish
            {
                FlightPlan.Route.Dequeue();
                FlightPlan.flightPlanAssignedData.AHDG = Convert.ToInt32(_heading);
                return;
            }

            TurnViaDirect(FlightPlan.Route.ElementAt(0).Coordinate);
        }
        else
        {
            FlightPlan.flightPlanAssignedData.AHDG = Convert.ToInt32(_heading);
        }
    }

    /// <summary>
    /// Accelerates/decelerates an aircraft in increments to a specific indicated airspeed
    /// The speed may be resolved if outside the capabilities of the aircraft
    /// Assumes a constant acceleration/deceleration of 1 knot s^-2
    /// </summary>
    /// <param name="newSpeed">indicated airspeed to fly</param>
    private void FlySpeed(double newSpeed)
    {
        if (_indicatedAirspeed > newSpeed)
        {
            // decelerate
            _indicatedAirspeed = Math.Max(_indicatedAirspeed - 1, _aircraftPerformance.GetNormalisedSpeed(PressureAltitude, newSpeed));
        }
        else if (_indicatedAirspeed < newSpeed)
        {
            // accelerate
            _indicatedAirspeed = Math.Min(_indicatedAirspeed + 1, _aircraftPerformance.GetNormalisedSpeed(PressureAltitude, newSpeed));
        }

        Groundspeed = GetGroundspeed(_indicatedAirspeed, PressureAltitude);
    }

    /// <summary>
    /// Provides logic for the vertical movement of aircraft
    /// </summary>
    private void VerticalMovement()
    {
        // automatic adherence to standing agreements if current sector is not controlled
        if (PressureAltitude == FlightPlan.flightPlanAssignedData.CFL)
        {
            if (Trajectory.CurrentSector != null && !Trajectory.CurrentSector.IsUserControlled())
            {
                if (FlightPlan.flightPlanAssignedData.StandingAgreements.Count() > 0)
                {
                    Queue<Waypoint> routeCopy = new(FlightPlan.Route);
                    StandingAgreement sa = FlightPlan.flightPlanAssignedData.StandingAgreements[0];

                    if (!string.IsNullOrEmpty(sa.Point) && routeCopy.Contains(_navigationData.WpList[sa.Point]))
                    { // the queue should always containg the standing agreement point, but just in case
                        double distanceToRun = NavTools.DistanceBetweenTwoPoints(Position, FlightPlan.Route.Peek().Coordinate);

                        while (routeCopy.Peek().Name != sa.Point)
                        {
                            DecimalCoordinate prev = routeCopy.Dequeue().Coordinate;
                            distanceToRun += NavTools.DistanceBetweenTwoPoints(prev, routeCopy.Peek().Coordinate);
                        }

                        // we checked for current level = cfl earlier, so in case of mismatch between this "estimate" and the real "simulation", the path shouldn't jump around, and is more efficient
                        double requiredDistance = (PressureAltitude - FlightPlan.flightPlanAssignedData.StandingAgreements[0].Level) / 1000 * (Groundspeed / 60);

                        if (distanceToRun < requiredDistance)
                        {
                            FlightPlan.flightPlanAssignedData.CFL = FlightPlan.flightPlanAssignedData.StandingAgreements[0].Level;
                        }
                    }
                }
            }
        }

        if (PressureAltitude > FlightPlan.flightPlanAssignedData.CFL)
        {
            PressureAltitude = Math.Max(PressureAltitude - _aircraftPerformance.GetRateOfDescent(PressureAltitude), FlightPlan.flightPlanAssignedData.CFL);
        }
        else if (PressureAltitude < FlightPlan.flightPlanAssignedData.CFL)
        {
            PressureAltitude = Math.Min(PressureAltitude + _aircraftPerformance.GetRateOfClimb(PressureAltitude), FlightPlan.flightPlanAssignedData.CFL);
        }
    }

    /// <summary>
    /// Updates position of an Aircraft instance
    /// </summary>
    public void UpdatePosition()
    {
        LateralMovement();
        VerticalMovement();
    }

    /// <summary>
    /// Predicts the future trajectory of the aircraft within 15 minutes
    /// </summary>
    public void PredictTrajectory()
    {
        const double M_TO_NM = 0.000539957;
        const double KNOT_TO_MS = 0.514444;

        DecimalCoordinate simPosition = Position;
        Queue<Waypoint> simRoute = new(FlightPlan.Route);
        double simHeading = _heading;
        double simAltitude = PressureAltitude;

        Trajectory.PredictedPoints.Clear();

        for (int t = 0; t < 30; t++)
        {
            // Lateral

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

            // Vertical

            if (simAltitude > FlightPlan.flightPlanAssignedData.CFL)
            {
                simAltitude = Math.Max(simAltitude - _aircraftPerformance.GetRateOfDescent(simAltitude) * 15, FlightPlan.flightPlanAssignedData.CFL);
            }
            else if (simAltitude < FlightPlan.flightPlanAssignedData.CFL)
            {
                simAltitude = Math.Min(simAltitude + _aircraftPerformance.GetRateOfClimb(simAltitude) * 15, FlightPlan.flightPlanAssignedData.CFL);
            }

            Trajectory.PredictedPoints.Add(new TrajectoryPoint(DateTime.UtcNow.Add(new(0, 0, 30 * t)), simPosition, simAltitude));
        }

        // Predict next sector

        Trajectory.CurrentSector = null;
        Trajectory.NextSector = null;

        foreach (Sector av in _navigationData.SectorList.Select(s => s.Value))
        {
            if (av.SectorVolume.IsInside(Position, PressureAltitude))
            {
                Trajectory.CurrentSector = av;
            }
        }

        foreach (TrajectoryPoint tp in Trajectory.PredictedPoints)
        {
            foreach (Sector av in _navigationData.SectorList.Select(s => s.Value))
            {
                if (av.SectorVolume.IsInside(tp.Position, tp.Altitude) && av != Trajectory.CurrentSector)
                {
                    Trajectory.NextSector = av;
                    return; //return early once next sector found
                }
            }
        }
    }

    /// <summary>
    /// Checks if the aircraft's current position satisfies the given standing agreement
    /// </summary>
    /// <param name="sa">the required standing agreement</param>
    /// <returns>true if the standing agreement has been satisfied</returns>
    public bool SatisfiedStandingAgreement(StandingAgreement sa)
    {
        if (PressureAltitude > sa.Level - 500 && PressureAltitude < sa.Level + 500) // this is less computationally intensive, so we check this first
        {
            if (sa.Point == "")
            {
                FlightPlan.flightPlanAssignedData.StandingAgreements.Remove(sa);
                return true;
            }

            if (NavTools.DistanceBetweenTwoPoints(Position, _navigationData.WpList[sa.Point].Coordinate) <= 2)
            {
                return true;
            }
        }
        else if (sa.Point == "") // don't penalise RFL agreements
        {
            return true;
        }

        return false;
    }
}