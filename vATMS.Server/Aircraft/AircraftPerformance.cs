namespace vATMS.Aircraft;

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