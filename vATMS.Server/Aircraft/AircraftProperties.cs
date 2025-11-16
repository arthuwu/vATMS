namespace vATMS.Aircraft;

/// <summary>
/// This class is used for JSON deserialisation
/// </summary>
public class AircraftJson
{
    public required List<AircraftType> AircraftTypes { get; init; }
}

/// <summary>
/// This class is used for JSON deserialisation
/// Attributes from this class are stored in FlightPlan and AircraftPerformance directly
/// </summary>
public class AircraftType
{
    public required string Type { get; init; }
    public required string WTC { get; init; }
    public required AircraftPerformance AircraftPerformance { get; init; }
}

/// <summary>
/// Each Aircraft instance has an AircraftPerformance instance which provides/calculates its physical performance
/// This class is initialised by parsing a JSON file
/// </summary>
public class AircraftPerformance
{
    private int _earlyClimbSpeed;
    private double _machClimbSpeed;
    private double _cruiseSpeed;
    private double _machDescentSpeed;
    private int _lateDescentSpeed;

    private int _earlyRateOfClimb;
    private int _machRateOfClimb;
    private int _machRateOfDescent;
    private int _lateRateOfDescent;

    private int _cruiseCeiling;

    /// <summary>
    /// Constructor for the AircraftPerformance class
    /// </summary>
    /// <param name="earlyClimbSpeed">climb speed up to FL240</param>
    /// <param name="machClimbSpeed">climb speed above FL240</param>
    /// <param name="cruiseSpeed">cruise speed</param>
    /// <param name="machDescentSpeed">descent speed above FL240</param>
    /// <param name="lateDescentSpeed"><descent speed below FL240/param>
    /// <param name="earlyRateOfClimb">rate of climb below FL240</param>
    /// <param name="machRateOfClimb">rate of climb above FL240</param>
    /// <param name="machRateOfDescent">rate of descent above FL240</param>
    /// <param name="lateRateOfDescent">rate of descent below FL240</param>
    /// <param name="cruiseCeiling">cruise ceiling</param>
    public AircraftPerformance(
        int earlyClimbSpeed,
        double machClimbSpeed,
        double cruiseSpeed,
        double machDescentSpeed,
        int lateDescentSpeed,
        int earlyRateOfClimb,
        int machRateOfClimb,
        int machRateOfDescent,
        int lateRateOfDescent,
        int cruiseCeiling
    )
    {
        _earlyClimbSpeed = earlyClimbSpeed;
        _machClimbSpeed = machClimbSpeed;
        _cruiseSpeed = cruiseSpeed;
        _machDescentSpeed = machDescentSpeed;
        _lateDescentSpeed = lateDescentSpeed;
        _earlyRateOfClimb = earlyRateOfClimb;
        _machRateOfClimb = machRateOfClimb;
        _machRateOfDescent = machRateOfDescent;
        _lateRateOfDescent = lateRateOfDescent;
        _cruiseCeiling = cruiseCeiling;
    }

    /// <summary>
    /// Calculates the indicated airspeed under normal operations
    /// </summary>
    /// <param name="pressureAltitude">current pressure altitude</param>
    /// <param name="isCruising">true if the aircraft is currently cruising</param>
    /// <returns>required indicated airspeed</returns>
    public double GetNormalIndicatedAirspeed(double pressureAltitude, double clearedAltitude)
    {
        if (pressureAltitude < 27000) // derive directly from IAS
        {
            if (clearedAltitude < pressureAltitude) // descending
            {
                return _lateDescentSpeed;
            }
            else // climbing or cruising
            {
                return _earlyClimbSpeed;
            }
        }
        else // derive from mach
        {
            if (pressureAltitude == clearedAltitude)
            {
                return GetIndicatedAirspeedFromMach(pressureAltitude, _cruiseSpeed);
            }
            else if (clearedAltitude > pressureAltitude) // climbing
            {
                return GetIndicatedAirspeedFromMach(pressureAltitude, _machClimbSpeed);
            }
            else // descending 
            {
                return GetIndicatedAirspeedFromMach(pressureAltitude, _machDescentSpeed);
            }
        }
    }

    /// <summary>
    /// Calcualtes the rate of climb
    /// </summary>
    /// <param name="pressureAltitude">current pressure altitude</param>
    /// <returns>required rate of climb</returns>
    public int GetRateOfClimb(double pressureAltitude)
    {
        if (pressureAltitude > _cruiseCeiling)
            return 0;

        if (pressureAltitude < 24000)
        {
            return _earlyRateOfClimb / 60;
        }
        else
        {
            return _machRateOfClimb / 60;
        }
    }

    /// <summary>
    /// Calculates the rate of descent
    /// </summary>
    /// <param name="pressureAltitude">current pressure altitude</param>
    /// <returns>required rate of descent</returns>
    public int GetRateOfDescent(double pressureAltitude)
    {
        if (pressureAltitude < 24000)
        {
            return _lateRateOfDescent / 60;
        }
        else
        {
            return _machRateOfDescent / 60;
        }
    }

    /// <summary>
    /// Calculates the speed the aircraft should fly at given the speed that is assigned
    /// </summary>
    /// <param name="pressureAltitude">current pressure altitude</param>
    /// <param name="assignedSpeed">assigned speed</param>
    /// <returns>the assigned speed if it is within range, an appropriate speed if it is not</returns>
    public double GetNormalisedSpeed(double pressureAltitude, double assignedSpeed)
    {
        if (pressureAltitude < 24000)
        {
            if (assignedSpeed > _lateDescentSpeed + 50)
            {
                return _lateDescentSpeed + 50;
            }
            else if (assignedSpeed < _earlyClimbSpeed - 60)
            {
                return _earlyClimbSpeed - 60;
            }
            else
            {
                return assignedSpeed;
            }
        }
        else
        {
            if (assignedSpeed > _lateDescentSpeed)
            {
                return _lateDescentSpeed;
            }
            else if (assignedSpeed < _earlyClimbSpeed - 60)
            {
                return _earlyClimbSpeed - 60;
            }
            else
            {
                return assignedSpeed;
            }
        }
    }

    /// <summary>
    /// Calculates the mach number the aircraft should fly at given the mach number that is assigned
    /// </summary>
    /// <param name="pressureAltitude">current pressure altitude</param>
    /// <param name="assignedMach">assigned mach number</param>
    /// <returns>the assigned mach number if it is within range, an appropriate mach number if it is not</returns>
    public double GetNormalisedMachNumber(double assignedMach)
    {
        if (assignedMach > _machDescentSpeed + 0.2)
        {
            return _machDescentSpeed + 0.2;
        }
        else if (assignedMach < _machClimbSpeed - 0.5)
        {
            return _machClimbSpeed - 0.5;
        }
        else
        {
            return assignedMach;
        }
    }

    /// <summary>
    /// Calculates the true airspeed corresponding to a mach number
    /// We want to calculate everything using the indicated airspeed even though it may seem a bit redundant (as we need to convert it from mach to TAS to IAS then back to TAS/GS)
    /// This is because all the physical movement of the aircraft should be done in terms of IAS, otherwise there may be data inconsistency
    /// </summary>
    /// <param name="pressureAltitude">current pressure altitude</param>
    /// <param name="assignedMach">assigned mach number</param>
    /// <returns>the required true airspeed</returns>
    public double GetIndicatedAirspeedFromMach(double pressureAltitude, double assignedMach)
    {
        const double MS_TO_KNOTS = 1.94384;

        double metricAlt = pressureAltitude / 3.281;
        double temperature = 288.15 - (0.00649 * metricAlt);

        double speedOfSound = Math.Sqrt(1.4 * 8.3145 * temperature / 0.0289645);

        double pressure = 101325 * Math.Pow((1 - 0.0000225577 * metricAlt), 5.25588); ;
        double airDensity = pressure / (287.05 * temperature);

        double trueAirspeed = assignedMach * speedOfSound * MS_TO_KNOTS;

        return trueAirspeed * Math.Sqrt(airDensity / 1.225);
    }
}