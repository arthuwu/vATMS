using vATMS.Aircraft;

public class Simulation
{
    // int? rate, int? pilotQuality
    private readonly System.Threading.Timer _timer;
    private readonly TargetGenerator _targetGenerator;
    private readonly RadarScreenUpdater _radarScreenUpdater;

    private List<Aircraft> _aircraft;
    // list<Controllers> controllers;

    public Simulation(RadarScreenUpdater radarScreenUpdater)
    {
        _aircraft = new List<Aircraft>();

        _targetGenerator = new TargetGenerator(1, 1, _aircraft); // TODO: test, demo values
        _radarScreenUpdater = radarScreenUpdater;

        _timer = new System.Threading.Timer(UpdateOnTimerElapsed, null, 0, 1000); // every second


        Console.WriteLine("DEBUG: new Simulation instance");
    }

    private async void UpdateOnTimerElapsed(object? state)
    {
        foreach (Aircraft ac in _aircraft)
        {
            // do stuff to aircraft
            // remember to check if it can be displayed using RadarHead, but otherwise
            // Console.WriteLine($"DEBUG: UpdateOnTimerElapsed, {ac.FlightPlan.Callsign} at {ac.Position.Lat}, {ac.Position.Lon}");
        }

        await _radarScreenUpdater.UpdateAircraftPosition(_aircraft);
    }
}