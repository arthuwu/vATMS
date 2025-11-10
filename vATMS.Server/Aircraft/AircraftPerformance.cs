namespace vATMS.Aircraft;

/// <summary>
/// Each Aircraft instance has an AircraftPerformance instance which provides/calculates its physical performance
/// </summary>
public class AircraftPerformance
{
    private int _earlyClimbSpeed;
    private int _machClimbSpeed;
    private int _cruiseSpeed;
    private int _machDescentSpeed;
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
        int machClimbSpeed,
        int cruiseSpeed,
        int machDescentSpeed,
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
    public int GetNormalIndicatedAirspeed(double pressureAltitude, bool isCruising)
    {
        Random random = new();
        if (isCruising && pressureAltitude > 27000)
            return _cruiseSpeed;

        if (pressureAltitude < 24000)
        {
            return random.Next(_earlyClimbSpeed, _lateDescentSpeed);
        }
        else
        {
            return random.Next(_machClimbSpeed, _machDescentSpeed);
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
    /// <returns>the assigned speed if it is within range, the bounds if it is not</returns>
    public double GetNormalisedSpeed(double pressureAltitude, double assignedSpeed)
    {
        if (pressureAltitude < 24000)
        {
            if (assignedSpeed > _lateDescentSpeed + 20)
            {
                return _lateDescentSpeed + 20;
            }
            else if (assignedSpeed < _earlyClimbSpeed - 30)
            {
                return _lateDescentSpeed - 30;
            }
            else
            {
                return assignedSpeed;
            }
        }
        else
        {
            if (assignedSpeed > _machDescentSpeed + 20)
            {
                return _machDescentSpeed + 20;
            }
            else if (assignedSpeed < _machClimbSpeed - 30)
            {
                return _machDescentSpeed - 30;
            }
            else
            {
                return assignedSpeed;
            }
        }
    }
}